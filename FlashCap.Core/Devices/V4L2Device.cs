////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using System;
using System.Diagnostics;
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
    private TranscodeFormats transcodeFormat;
    private FrameProcessor frameProcessor;
    private uint[] pix_fmts;

    private long frameIndex;
    
    private IntPtr[] pBuffers = new IntPtr[BufferCount];
    private int[] bufferLength = new int[BufferCount];
    
    private int fd;
    private IntPtr pBih;
    private Task task;
    private int abortrfd;
    private int abortwfd;

#pragma warning disable CS8618
    internal V4L2Device(object identity, string name) :
        base(identity, name)
#pragma warning restore CS8618
    {
    }

    protected override unsafe Task OnInitializeAsync(
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        FrameProcessor frameProcessor,
        CancellationToken ct)
    {
        this.devicePath = (string)this.Identity;
        this.Characteristics = characteristics;
        this.transcodeFormat = transcodeFormat;
        this.frameProcessor = frameProcessor;

        if (!NativeMethods.GetCompressionAndBitCount(
            characteristics.PixelFormat, out var compression, out var bitCount))
        {
            throw new ArgumentException(
                $"FlashCap: Couldn't set video format [1]: DevicePath={this.devicePath}");
        }

        this.pix_fmts = GetPixelFormats(
            characteristics.PixelFormat);
        if (this.pix_fmts.Length == 0)
        {
            throw new ArgumentException(
                $"FlashCap: Couldn't set video format [2]: DevicePath={this.devicePath}");
        }

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

            this.pBih = pih;
        }
        catch
        {
            NativeMethods.FreeMemory(pih);
            throw;
        }

        return TaskCompat.CompletedTask;
    }

    ~V4L2Device()
    {
        if (this.abortwfd != -1)
        {
            write(this.abortwfd, new byte[] { 0x01 }, 1);
            close(this.abortwfd);
            this.abortwfd = -1;
        }
        
        if (this.abortrfd != -1)
        {
            close(this.abortrfd);
            this.abortrfd = -1;
        }

        if (this.pBuffers is { } pBuffers &&
            this.bufferLength is { } bufferLength)
        {
            for (var index = 0; index < pBuffers.Length; index++)
            {
                if (pBuffers[index] != default &&
                    bufferLength[index] != default)
                {
                    munmap(pBuffers[index], (ulong)bufferLength[index]);
                    pBuffers[index] = default;
                    bufferLength[index] = default;
                }
            }

            this.pBuffers = null!;
            this.bufferLength = null!;
        }
        
        if (this.fd != -1)
        {
            close(this.fd);
            this.fd = -1;
        }

        if (this.pBih != IntPtr.Zero)
        {
            NativeMethods.FreeMemory(this.pBih);
            this.pBih = IntPtr.Zero;
        }

        this.IsRunning = false;
    }

    protected override async Task OnDisposeAsync()
    {
        if (this.IsRunning)
        {
            await this.frameProcessor.DisposeAsync().
                ConfigureAwait(false);

            await this.InternalStopAsync(default).
                ConfigureAwait(false);
        }
    }

    protected override Task OnStartAsync(CancellationToken ct)
    {
        if (!this.IsRunning)
        {
            return Task.Factory.StartNew(() =>
            {
                if (open(this.devicePath, OPENBITS.O_RDWR) is { } fd && fd < 0)
                {
                    var code = Marshal.GetLastWin32Error();
                    throw new ArgumentException(
                        $"FlashCap: Couldn't open video device: Code={code}, DevicePath={this.devicePath}");
                }

                try
                {
                    var applied = false;
                    foreach (var pix_fmt in this.pix_fmts)
                    {
                        var fmt_pix = Interop.Create_v4l2_pix_format();
                        fmt_pix.width = (uint)this.Characteristics.Width;
                        fmt_pix.height = (uint)this.Characteristics.Height;
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

                        this.pBuffers[index] = pBuffer;
                        this.bufferLength[index] = (int)buffer.length;

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

                    if (ioctl(
                        fd, Interop.VIDIOC_STREAMON,
                        (int)v4l2_buf_type.VIDEO_CAPTURE) < 0)
                    {
                        var code = Marshal.GetLastWin32Error();
                        throw new ArgumentException(
                            $"FlashCap: Couldn't start capture: Code={code}, DevicePath={this.devicePath}");
                    }

                    this.abortrfd = abortfds[0];
                    this.abortwfd = abortfds[1];
                    this.fd = fd;
 
                    this.frameIndex = 0;
                    this.counter.Restart();
 
                    this.IsRunning = true;
                    this.task = Task.Factory.StartNew(
                        this.ThreadEntry, TaskCreationOptions.LongRunning);
                }
                catch
                {
                    for (var index = 0; index < pBuffers.Length; index++)
                    {
                        if (this.pBuffers[index] != default &&
                            this.bufferLength[index] != default)
                        {
                            munmap(this.pBuffers[index], (ulong)this.bufferLength[index]);
                            this.pBuffers[index] = default;
                            this.bufferLength[index] = default;
                        }
                    }
                    
                    close(fd);
                    throw;
                }
            });
        }

        return TaskCompat.CompletedTask;
    }

    protected override async Task OnStopAsync(CancellationToken ct)
    {
        if (this.IsRunning)
        {
            if (ioctl(
                this.fd, Interop.VIDIOC_STREAMOFF,
                (int)v4l2_buf_type.VIDEO_CAPTURE) < 0)
            {
                var code = Marshal.GetLastWin32Error();
                throw new ArgumentException(
                    $"FlashCap: Couldn't stop capture: Code={code}, DevicePath={this.devicePath}");
            }
                
            write(this.abortwfd, new byte[] { 0x01 }, 1);

            var task = Interlocked.Exchange(ref this.task, null!);
            await task.ConfigureAwait(false);

            close(this.abortwfd);
            this.abortwfd = -1;

            for (var index = 0; index < this.pBuffers.Length; index++)
            {
                if (this.pBuffers[index] != default &&
                    this.bufferLength[index] != default)
                {
                    munmap(this.pBuffers[index], (ulong)this.bufferLength[index]);
                    this.pBuffers[index] = default;
                    this.bufferLength[index] = default;
                }
            }
            
            close(this.abortrfd);
            close(this.fd);
            
            this.abortrfd = -1;
            this.fd = -1;

            this.IsRunning = false;
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
                events = POLLBITS.POLLIN | POLLBITS.POLLHUP | POLLBITS.POLLRDHUP | POLLBITS.POLLERR,
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
            while (this.IsRunning)
            {
                var pr = poll(fds, fds.Length, -1);
                if (pr < 0)
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
                if (pr >= 1)
                {
                    if ((fds[0].revents & POLLBITS.POLLIN) == POLLBITS.POLLIN ||
                        (fds[0].revents & POLLBITS.POLLHUP) == POLLBITS.POLLHUP ||
                        (fds[0].revents & POLLBITS.POLLRDHUP) == POLLBITS.POLLRDHUP ||
                        (fds[0].revents & POLLBITS.POLLERR) == POLLBITS.POLLERR)
                    {
                        break;
                    }
                    if ((fds[1].revents & POLLBITS.POLLIN) == POLLBITS.POLLIN)
                    {
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

                        if ((buffer.flags & Interop.V4L2_BUF_FLAG_ERROR) != Interop.V4L2_BUF_FLAG_ERROR)
                        {
                            try
                            {
                                this.frameProcessor.OnFrameArrived(
                                    this,
                                    this.pBuffers[buffer.index],
                                    (int)buffer.bytesused,
                                    // buffer.timestamp is untrustworthy.
                                    this.counter.ElapsedMicroseconds,
                                    this.frameIndex++);
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine(ex);
                            }
                        }

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
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    protected override void OnCapture(
        IntPtr pData, int size,
        long timestampMicroseconds, long frameIndex,
        PixelBuffer buffer) =>
        buffer.CopyIn(this.pBih, pData, size, timestampMicroseconds, frameIndex, this.transcodeFormat);
}
