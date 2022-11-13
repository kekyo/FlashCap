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
using System.Threading.Tasks;
using static FlashCap.Internal.NativeMethods_V4L2;
using static FlashCap.Internal.V4L2.NativeMethods_V4L2_Interop;

namespace FlashCap.Devices;

public sealed class V4L2Device : CaptureDevice
{
    private const int BufferCount = 2;

    private readonly TimestampCounter counter = new();
    private string devicePath;
    private bool transcodeIfYUV;
    private FrameProcessor frameProcessor;

    private long frameIndex;
    
    private int fd;
    private IntPtr pBih;
    private IntPtr[] pBuffers = new IntPtr[BufferCount];
    private int[] bufferLength = new int[BufferCount];
    private Thread thread;
    private int abortrfd;
    private int abortwfd;

#pragma warning disable CS8618
    internal V4L2Device()
#pragma warning restore CS8618
    {
    }

    protected override unsafe Task OnInitializeAsync(
        object identity,
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        FrameProcessor frameProcessor,
        CancellationToken ct)
    {
        this.devicePath = (string)identity;
        this.Characteristics = characteristics;
        this.transcodeIfYUV = transcodeIfYUV;
        this.frameProcessor = frameProcessor;

        if (!NativeMethods.GetCompressionAndBitCount(
            characteristics.PixelFormat, out var compression, out var bitCount))
        {
            throw new ArgumentException(
                $"FlashCap: Couldn't set video format [1]: DevicePath={this.devicePath}");
        }

        var pix_fmts = GetPixelFormats(
            characteristics.PixelFormat);
        if (pix_fmts.Length == 0)
        {
            throw new ArgumentException(
                $"FlashCap: Couldn't set video format [2]: DevicePath={this.devicePath}");
        }

        if (open(this.devicePath, OPENBITS.O_RDWR) is { } fd && fd < 0)
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
                var fmt_pix = Interop.Create_v4l2_pix_format();
                fmt_pix.width = (uint)characteristics.Width;
                fmt_pix.height = (uint)characteristics.Height;
                fmt_pix.pixelformat = pix_fmt;
                fmt_pix.field = (uint)v4l2_field.ANY;
                
                var format = Interop.Create_v4l2_format();
                format.type = (uint)v4l2_buf_type.VIDEO_CAPTURE;
                format.fmt_pix = fmt_pix;
                
                if (ioctl(fd, Interop.VIDIOC_S_FMT, format) == 0)
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

            var requestbuffers = Interop.Create_v4l2_requestbuffers();
            requestbuffers.count = BufferCount;   // Flipping
            requestbuffers.type = (uint)v4l2_buf_type.VIDEO_CAPTURE;
            requestbuffers.memory = (uint)v4l2_memory.MMAP;

            if (ioctl(fd, Interop.VIDIOC_REQBUFS, requestbuffers) < 0)
            {
                var code = Marshal.GetLastWin32Error();
                throw new ArgumentException(
                    $"FlashCap: Couldn't allocate video buffer: Code={code}, DevicePath={this.devicePath}");
            }

            for (var index = 0; index < requestbuffers.count; index++)
            {
                var buffer = Interop.Create_v4l2_buffer();
                buffer.type = (uint)v4l2_buf_type.VIDEO_CAPTURE;
                buffer.memory = (uint)v4l2_memory.MMAP;
                buffer.index = (uint)index;
                
                if (ioctl(fd, Interop.VIDIOC_QUERYBUF, buffer) < 0)
                {
                    var code = Marshal.GetLastWin32Error();
                    throw new ArgumentException(
                        $"FlashCap: Couldn't assign video buffer: Code={code}, DevicePath={this.devicePath}");
                }
                
                if (mmap(IntPtr.Zero, buffer.length, PROT.READ, MAP.SHARED,
                    fd, buffer.m_offset) is { } pBuffer &&
                    pBuffer == MAP_FAILED)
                {
                    var code = Marshal.GetLastWin32Error();
                    throw new ArgumentException(
                        $"FlashCap: Couldn't map video buffer: Code={code}, DevicePath={this.devicePath}");
                }

                pBuffers[index] = pBuffer;
                bufferLength[index] = (int)buffer.length;

                if (ioctl(fd, Interop.VIDIOC_QBUF, buffer) < 0)
                {
                    var code = Marshal.GetLastWin32Error();
                    throw new ArgumentException(
                        $"FlashCap: Couldn't enqueue video buffer: Code={code}, DevicePath={this.devicePath}");
                }
            }

            var abortfds = new int[2];
            if (pipe(abortfds) < 0)
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
            close(fd);
            throw;
        }

        return TaskCompat.CompletedTask;
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
                        ioctl(
                            this.fd, Interop.VIDIOC_STREAMOFF,
                            (int)v4l2_buf_type.VIDEO_CAPTURE);
                    }
                }
                
                write(this.abortwfd, new byte[] { 0x01 }, 1);
                
                this.thread.Join();
                close(this.abortwfd);
                this.abortwfd = -1;
            }
        }
        else
        {
            if (this.abortwfd != -1)
            {
                write(this.abortwfd, new byte[] { 0x01 }, 1);
                close(this.abortwfd);
                this.abortwfd = -1;
            }
        }
    }

    private void ThreadEntry()
    {
        static bool IsIgnore(int code) =>
            code switch
            {
                EINTR => true,
                EINVAL => true,
                _ => false,
            };
        
        var fds = new[]
        {
            new pollfd
            {
                fd = this.abortrfd,
                events = POLLBITS.POLLIN,
            },
            new pollfd
            {
                fd = this.fd,
                events = POLLBITS.POLLIN,
            }
        };
        var buffer = Interop.Create_v4l2_buffer();
        buffer.type = (uint)v4l2_buf_type.VIDEO_CAPTURE;
        buffer.memory = (uint)v4l2_memory.MMAP;

        try
        {
            while (true)
            {
                var result = poll(fds, fds.Length, -1);
                if (result == 0)
                {
                    break;
                }
                if (result != 1)
                {
                    var code = Marshal.GetLastWin32Error();
                    if (code == EINTR)
                    {
                        continue;
                    }
                    // Couldn't get with EINVAL, maybe discarding.
                    if (code == EINVAL)
                    {
                        break;
                    }
                    throw new ArgumentException(
                        $"FlashCap: Couldn't get fd status: Code={code}, DevicePath={this.devicePath}");
                }

                if (ioctl(this.fd, Interop.VIDIOC_DQBUF, buffer) < 0)
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
                    (int)buffer.bytesused,
                    // buffer.timestamp is untrustworthy.
                    this.counter.ElapsedMicroseconds,
                    this.frameIndex++);
            
                if (ioctl(this.fd, Interop.VIDIOC_QBUF, buffer) < 0)
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
            close(this.abortrfd);
            close(this.fd);
            NativeMethods.FreeMemory(this.pBih);
            this.abortrfd = -1;
            this.fd = -1;
            this.pBih = IntPtr.Zero;
        }
    }

    protected override void OnStart()
    {
        lock (this)
        {
            if (!this.IsRunning)
            {
                this.frameIndex = 0;
                this.counter.Restart();

                if (ioctl(
                    this.fd, Interop.VIDIOC_STREAMON,
                    (int)v4l2_buf_type.VIDEO_CAPTURE) < 0)
                {
                    var code = Marshal.GetLastWin32Error();
                    throw new ArgumentException(
                        $"FlashCap: Couldn't start capture: Code={code}, DevicePath={this.devicePath}");
                }
                this.IsRunning = true;
            }
        }
    }

    protected override void OnStop()
    {
        lock (this)
        {
            if (this.IsRunning)
            {
                this.IsRunning = false;
                if (ioctl(
                    this.fd, Interop.VIDIOC_STREAMOFF,
                    (int)v4l2_buf_type.VIDEO_CAPTURE) < 0)
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
