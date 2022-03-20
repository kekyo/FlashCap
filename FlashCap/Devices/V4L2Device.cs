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

namespace FlashCap.Devices
{
    public sealed class V4L2Device : ICaptureDevice
    {
        private readonly string devicePath;
        private readonly bool transcodeIfYUV;
        private int fd;
        private IntPtr pBih;

        internal unsafe V4L2Device(
            string devicePath, VideoCharacteristics characteristics, bool transcodeIfYUV)
        {
            this.devicePath = devicePath;
            this.Characteristics = characteristics;
            this.transcodeIfYUV = transcodeIfYUV;
          
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
                throw new ArgumentException(
                    $"FlashCap: Couldn't open video device: DevicePath={this.devicePath}");
            }

            try
            {
                var applied = false;
                foreach (var pix_fmt in pix_fmts)
                {
                    var format = new NativeMethods_V4L2.v4l2_format
                    {
                        type = NativeMethods_V4L2.v4l2_buf_type.VIDEO_CAPTURE,
                    };
                    format.fmt.pix.width = characteristics.Width;
                    format.fmt.pix.height = characteristics.Height;
                    format.fmt.pix.pixelformat = pix_fmt;
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

        ~V4L2Device() =>
            this.Dispose();

        public void Dispose()
        {
            if (this.fd != -1)
            {
                NativeMethods_V4L2.close(this.fd);
                NativeMethods.FreeMemory(this.pBih);
                this.fd = -1;
                this.pBih = IntPtr.Zero;
            }
        }

        public VideoCharacteristics Characteristics { get; }
        public bool IsRunning { get; }

        public event EventHandler<FrameArrivedEventArgs>? FrameArrived;

        public void Start()
        {
            this.FrameArrived?.Invoke(this, null!);
        }

        public void Stop()
        {
        }

        public void Capture(FrameArrivedEventArgs e, PixelBuffer buffer) =>
            buffer.CopyIn(
                this.pBih, e.pData, e.size,
                e.timestampMilliseconds, this.transcodeIfYUV);
    }
}
