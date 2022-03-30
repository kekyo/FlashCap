////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace FlashCap.Devices
{
    public sealed class V4L2Device : CaptureDevice
    {
        private const int BufferCount = 2;
        
        private readonly string devicePath;
        private readonly bool transcodeIfYUV;
        private readonly FrameProcessor frameProcessor;
        private readonly TimestampCounter counter = new();
        private long frameIndex;
        
        private int fd;
        private IntPtr pBih;
        private IntPtr[] pBuffers = new IntPtr[BufferCount];
        private int[] bufferLength = new int[BufferCount];
        private Thread thread;
        private int abortrfd;
        private int abortwfd;

        internal unsafe V4L2Device(
            string devicePath, VideoCharacteristics characteristics, bool transcodeIfYUV,
            FrameProcessor frameProcessor)
        {
            this.devicePath = devicePath;
            this.Characteristics = characteristics;
            this.transcodeIfYUV = transcodeIfYUV;
            this.frameProcessor = frameProcessor;

            if (!NativeMethods.GetCompressionAndBitCount(
                characteristics.PixelFormat, out var compression, out var bitCount))
            {
                throw new ArgumentException(
                    $"FlashCap: Couldn't set video format [1]: DevicePath={this.devicePath}");
            }

            var pix_fmts = NativeMethods_V4L2.GetPixelFormats(
                characteristics.PixelFormat);
            if (pix_fmts.Length == 0)
            {
                throw new ArgumentException(
                    $"FlashCap: Couldn't set video format [2]: DevicePath={this.devicePath}");
            }

            if (NativeMethods_V4L2.open(
                this.devicePath, NativeMethods_V4L2.OPENBITS.O_RDWR) is { } fd && fd < 0)
            {
                var code = Marshal.GetLastWin32Error();
                throw new ArgumentException(
                    $"FlashCap: Couldn't open video device: Code={code}, DevicePath={this.devicePath}");
            }

            try
            {
                var applied = false;
                foreach (var pix_fmt in pix_fmts)
                {
                    var format = new NativeMethods_V4L2.v4l2_format();
                    format.type.type = NativeMethods_V4L2.v4l2_buf_type.VIDEO_CAPTURE;
                    format.fmt.pix.width = characteristics.Width;
                    format.fmt.pix.height = characteristics.Height;
                    format.fmt.pix.pixelformat = pix_fmt;
                    format.fmt.pix.field = NativeMethods_V4L2.v4l2_field.ANY;
                    if (NativeMethods_V4L2.ioctls(fd, in format) == 0)
                    {
                        applied = true;
                        break;
                    }
                }
                if (!applied)
                {
                    throw new ArgumentException(
                        $"FlashCap: Couldn't set video format [3]: DevicePath={this.devicePath}");
                }

                var requestbuffers = new NativeMethods_V4L2.v4l2_requestbuffers
                {
                    count = BufferCount,   // Flipping
                    type = NativeMethods_V4L2.v4l2_buf_type.VIDEO_CAPTURE,
                    memory = NativeMethods_V4L2.v4l2_memory.MMAP,
                };
                if (NativeMethods_V4L2.ioctl(fd, ref requestbuffers) < 0)
                {
                    var code = Marshal.GetLastWin32Error();
                    throw new ArgumentException(
                        $"FlashCap: Couldn't allocate video buffer: Code={code}, DevicePath={this.devicePath}");
                }

                for (var index = 0; index < requestbuffers.count; index++)
                {
                    var buffer = new NativeMethods_V4L2.v4l2_buffer
                    {
                        type = NativeMethods_V4L2.v4l2_buf_type.VIDEO_CAPTURE,
                        memory = NativeMethods_V4L2.v4l2_memory.MMAP,
                        index = index,
                    };
                    if (NativeMethods_V4L2.ioctl_querybuf(fd, ref buffer) < 0)
                    {
                        var code = Marshal.GetLastWin32Error();
                        throw new ArgumentException(
                            $"FlashCap: Couldn't assign video buffer: Code={code}, DevicePath={this.devicePath}");
                    }

                    if (NativeMethods_V4L2.mmap(
                        IntPtr.Zero,
                        (IntPtr)buffer.length,
                        NativeMethods_V4L2.PROT.READ,
                        NativeMethods_V4L2.MAP.SHARED,
                        fd,
                        buffer.m.offset) is { } pBuffer &&
                        pBuffer == NativeMethods_V4L2.MAP_FAILED)
                    {
                        var code = Marshal.GetLastWin32Error();
                        throw new ArgumentException(
                            $"FlashCap: Couldn't map video buffer: Code={code}, DevicePath={this.devicePath}");
                    }

                    pBuffers[index] = pBuffer;
                    bufferLength[index] = buffer.length;

                    if (NativeMethods_V4L2.ioctl_qbuf(fd, buffer) < 0)
                    {
                        var code = Marshal.GetLastWin32Error();
                        throw new ArgumentException(
                            $"FlashCap: Couldn't enqueue video buffer: Code={code}, DevicePath={this.devicePath}");
                    }
                }

                var abortfds = new int[2];
                if (NativeMethods_V4L2.pipe(abortfds) < 0)
                {
                    var code = Marshal.GetLastWin32Error();
                    throw new ArgumentException(
                        $"FlashCap: Couldn't open pipe: Code={code}, DevicePath={this.devicePath}");
                }
                this.abortrfd = abortfds[0];
                this.abortwfd = abortfds[1];

                var pih = NativeMethods.AllocateMemory((IntPtr)sizeof(NativeMethods.BITMAPINFOHEADER));
                try
                {
                    var pBih = (NativeMethods.BITMAPINFOHEADER*)pih.ToPointer();

                    pBih->biSize = sizeof(NativeMethods.BITMAPINFOHEADER);
                    pBih->biCompression = compression;
                    pBih->biPlanes = 1;
                    pBih->biBitCount = bitCount;
                    pBih->biWidth = characteristics.Width;
                    pBih->biHeight = characteristics.Height;
                    pBih->biSizeImage = pBih->CalculateImageSize();
                    
                    this.fd = fd;
                    this.pBih = pih;

                    this.thread = new Thread(this.ThreadEntry);
                    this.thread.IsBackground = true;
                    this.thread.Start();
                }
                catch
                {
                    NativeMethods.FreeMemory(pih);
                    throw;
                }
            }
            catch
            {
                NativeMethods_V4L2.close(fd);
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.abortwfd != -1)
                {
                    lock (this)
                    {
                        if (this.IsRunning)
                        {
                            NativeMethods_V4L2.ioctl_streamoff(
                                this.fd,
                                NativeMethods_V4L2.v4l2_buf_type.VIDEO_CAPTURE);
                        }
                    }
                    
                    NativeMethods_V4L2.write(
                        this.abortwfd, new byte[] { 0x01 }, 1);
                    
                    this.thread.Join();
                    NativeMethods_V4L2.close(this.abortwfd);
                    this.abortwfd = -1;
                }
            }
            else
            {
                if (this.abortwfd != -1)
                {
                    NativeMethods_V4L2.write(
                        this.abortwfd, new byte[] { 0x01 }, 1);
                    NativeMethods_V4L2.close(this.abortwfd);
                    this.abortwfd = -1;
                }
            }
        }

        private void ThreadEntry()
        {
            static bool IsIgnore(int code) =>
                code switch
                {
                    NativeMethods_V4L2.EINTR => true,
                    NativeMethods_V4L2.EINVAL => true,
                    _ => false,
                };
            
            var fds = new[]
            {
                new NativeMethods_V4L2.pollfd
                {
                    fd = this.abortrfd,
                    events = NativeMethods_V4L2.POLLBITS.POLLIN,
                },
                new NativeMethods_V4L2.pollfd
                {
                    fd = this.fd,
                    events = NativeMethods_V4L2.POLLBITS.POLLIN,
                }
            };
            var buffer = new NativeMethods_V4L2.v4l2_buffer
            {
                type = NativeMethods_V4L2.v4l2_buf_type.VIDEO_CAPTURE,
                memory = NativeMethods_V4L2.v4l2_memory.MMAP,
            };

            try
            {
                while (true)
                {
                    var result = NativeMethods_V4L2.poll(fds, fds.Length, -1);
                    if (result == 0)
                    {
                        break;
                    }
                    if (result != 1)
                    {
                        var code = Marshal.GetLastWin32Error();
                        if (code == NativeMethods_V4L2.EINTR)
                        {
                            continue;
                        }
                        // Couldn't get with EINVAL, maybe discarding.
                        if (code == NativeMethods_V4L2.EINVAL)
                        {
                            break;
                        }
                        throw new ArgumentException(
                            $"FlashCap: Couldn't get fd status: Code={code}, DevicePath={this.devicePath}");
                    }

                    if (NativeMethods_V4L2.ioctl_dqbuf(this.fd, buffer) < 0)
                    {
                        // Couldn't get, maybe discarding.
                        if (Marshal.GetLastWin32Error() is { } code && IsIgnore(code))
                        {
                            continue;
                        }
                        throw new ArgumentException(
                            $"FlashCap: Couldn't dequeue video buffer: Code={code}, DevicePath={this.devicePath}");
                    }

                    this.frameProcessor.OnFrameArrived(
                        this,
                        this.pBuffers[buffer.index],
                        buffer.bytesused,
                        // buffer.timestamp is untrustworthy.
                        this.counter.ElapsedMicroseconds,
                        this.frameIndex++);
                
                    if (NativeMethods_V4L2.ioctl_qbuf(this.fd, buffer) < 0)
                    {
                        // Couldn't get, maybe discarding.
                        if (Marshal.GetLastWin32Error() is { } code && IsIgnore(code))
                        {
                            continue;
                        }
                        throw new ArgumentException(
                            $"FlashCap: Couldn't enqueue video buffer: Code={code}, DevicePath={this.devicePath}");
                    }
                }
            }
            finally
            {
                NativeMethods_V4L2.close(this.abortrfd);
                NativeMethods_V4L2.close(this.fd);
                NativeMethods.FreeMemory(this.pBih);
                this.abortrfd = -1;
                this.fd = -1;
                this.pBih = IntPtr.Zero;
            }
        }
        
        public override void Start()
        {
            lock (this)
            {
                if (!this.IsRunning)
                {
                    this.frameIndex = 0;
                    this.counter.Restart();

                    if (NativeMethods_V4L2.ioctl_streamon(
                        this.fd,
                        NativeMethods_V4L2.v4l2_buf_type.VIDEO_CAPTURE) < 0)
                    {
                        var code = Marshal.GetLastWin32Error();
                        throw new ArgumentException(
                            $"FlashCap: Couldn't start capture: Code={code}, DevicePath={this.devicePath}");
                    }
                    this.IsRunning = true;
                }
            }
        }

        public override void Stop()
        {
            lock (this)
            {
                if (this.IsRunning)
                {
                    this.IsRunning = false;
                    if (NativeMethods_V4L2.ioctl_streamoff(
                        this.fd,
                        NativeMethods_V4L2.v4l2_buf_type.VIDEO_CAPTURE) < 0)
                    {
                        var code = Marshal.GetLastWin32Error();
                        this.IsRunning = true;
                        throw new ArgumentException(
                            $"FlashCap: Couldn't stop capture: Code={code}, DevicePath={this.devicePath}");
                    }
                }
            }
        }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected override void OnCapture(
            IntPtr pData, int size,
            long timestampMicroseconds, long frameIndex,
            PixelBuffer buffer) =>
            buffer.CopyIn(this.pBih, pData, size, timestampMicroseconds, frameIndex, this.transcodeIfYUV);
    }
}
