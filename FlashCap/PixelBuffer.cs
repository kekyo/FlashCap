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
using System.Diagnostics;

namespace FlashCap
{
    public sealed class PixelBuffer
    {
        private byte[]? imageContainer;
        private byte[]? transcodedImageContainer = null;
        private double timestampMilliseconds;
        private bool transcodeIfYUV;

        internal unsafe void CopyIn(
            IntPtr pih, IntPtr pData, int size,
            double timestampMilliseconds, bool transcodeIfYUV)
        {
            this.timestampMilliseconds = timestampMilliseconds;

            var pBih = (NativeMethods.BITMAPINFOHEADER*)pih.ToPointer();

            var totalSize = pBih->biCompression switch
            {
                PixelFormats.MJPG => size,
                PixelFormats.JPEG => size,
                PixelFormats.PNG => size,
                _ => sizeof(NativeMethods.BITMAPFILEHEADER) +
                    pBih->biSize + size
            };

            lock (this)
            {
                if (this.imageContainer == null ||
                    this.imageContainer.Length != totalSize)
                {
                    this.imageContainer = new byte[totalSize];
                    this.transcodedImageContainer = null;
                }

                fixed (byte* pImageContainer = this.imageContainer!)
                {
                    if (pBih->biCompression == PixelFormats.MJPG ||
                        pBih->biCompression == PixelFormats.JPEG ||
                        pBih->biCompression == PixelFormats.PNG)
                    {
                        NativeMethods.CopyMemory(
                            (IntPtr)pImageContainer,
                            pData,
                            (IntPtr)size);

                        this.transcodeIfYUV = false;
                    }
                    else
                    {
                        var pBfhTo = (NativeMethods.BITMAPFILEHEADER*)pImageContainer;
                        pBfhTo->bfType0 = 0x42;
                        pBfhTo->bfType1 = 0x4d;
                        pBfhTo->bfSize = totalSize;

                        pBfhTo->bfOffBits =
                            sizeof(NativeMethods.BITMAPFILEHEADER) +
                            pBih->biSize;

                        var pBihTo = (NativeMethods.BITMAPINFOHEADER*)(pBfhTo + 1);

                        NativeMethods.CopyMemory(
                            (IntPtr)pBihTo,
                            (IntPtr)pBih,
                            (IntPtr)(pBih->biSize));

                        NativeMethods.CopyMemory(
                            (IntPtr)(pImageContainer + pBfhTo->bfOffBits),
                            pData,
                            (IntPtr)size);

                        this.transcodeIfYUV = transcodeIfYUV;
                    }
                }
            }
        }

        public TimeSpan Timestamp =>
            TimeSpan.FromMilliseconds(this.timestampMilliseconds);

        public unsafe byte[] ExtractImage()
        {
            lock (this)
            {
                if (this.imageContainer == null)
                {
                    throw new InvalidOperationException("Extracted before capture.");
                }

                if (this.transcodeIfYUV)
                {
                    fixed (byte* pImageContainer = this.imageContainer)
                    {
                        var pBfh = (NativeMethods.BITMAPFILEHEADER*)pImageContainer;
                        var pBih = (NativeMethods.BITMAPINFOHEADER*)(pBfh + 1);

                        if (BitmapTranscoder.GetRequiredBufferSize(
                            pBih->biWidth, pBih->biHeight, pBih->biCompression) is { } sizeImage)
                        {
                            var totalSize =
                                sizeof(NativeMethods.BITMAPFILEHEADER) +
                                sizeof(NativeMethods.BITMAPINFOHEADER) +
                                sizeImage;

                            if (this.transcodedImageContainer == null ||
                                this.transcodedImageContainer.Length != totalSize)
                            {
                                this.transcodedImageContainer = new byte[totalSize];
                            }

                            fixed (byte* pTranscodedImageContainer = this.transcodedImageContainer)
                            {
                                var pBfhTo = (NativeMethods.BITMAPFILEHEADER*)pTranscodedImageContainer;
                                var pBihTo = (NativeMethods.BITMAPINFOHEADER*)(pBfhTo + 1);

                                pBfhTo->bfType0 = 0x42;
                                pBfhTo->bfType1 = 0x4d;
                                pBfhTo->bfSize = totalSize;

                                pBfhTo->bfOffBits =
                                    sizeof(NativeMethods.BITMAPFILEHEADER) +
                                    sizeof(NativeMethods.BITMAPINFOHEADER);

                                pBihTo->biSize = sizeof(NativeMethods.BITMAPINFOHEADER);
                                pBihTo->biWidth = pBih->biWidth;
                                pBihTo->biHeight = pBih->biHeight;
                                pBihTo->biPlanes = 1;
                                pBihTo->biBitCount = 24;   // B8G8R8
                                pBihTo->biCompression = PixelFormats.RGB;
                                pBihTo->biSizeImage = sizeImage;
#if DEBUG
                                var sw = new Stopwatch();
                                sw.Start();
#endif
                                BitmapTranscoder.Transcode(
                                    pBih->biWidth, pBih->biHeight,
                                    pBih->biCompression, false,
                                    pImageContainer + pBfh->bfOffBits,
                                    pTranscodedImageContainer + pBfhTo->bfOffBits);

#if DEBUG
                                Debug.WriteLine($"Transcoded: Elapsed={sw.Elapsed}");
#endif
                            }

                            return this.transcodedImageContainer!;
                        }
                    }
                }

                return this.imageContainer;
            }
        }
    }
}
