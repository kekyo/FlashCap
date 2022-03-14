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
        private bool transcodeIfYUV;

        internal unsafe void CopyIn(
            IntPtr pih, IntPtr pData, int size, bool transcodeIfYUV)
        {
            var pBih = (NativeMethods.RAW_BITMAPINFOHEADER*)pih.ToPointer();

            var totalSize = pBih->biCompression switch
            {
                PixelFormats.MJPG => size,
                PixelFormats.JPEG => size,
                PixelFormats.PNG => size,
                _ => sizeof(NativeMethods.RAW_BITMAPFILEHEADER) +
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
                        var pBfhTo = (NativeMethods.RAW_BITMAPFILEHEADER*)pImageContainer;
                        pBfhTo->bfType0 = 0x42;
                        pBfhTo->bfType1 = 0x4d;
                        pBfhTo->bfSize = totalSize;

                        pBfhTo->bfOffBits =
                            sizeof(NativeMethods.RAW_BITMAPFILEHEADER) +
                            pBih->biSize;

                        var pBihTo = (NativeMethods.RAW_BITMAPINFOHEADER*)(pBfhTo + 1);

                        NativeMethods.CopyMemory(
                            (IntPtr)pBihTo,
                            (IntPtr)pBih,
                            (IntPtr)(pBih->biSize));

                        NativeMethods.CopyMemory(
                            (IntPtr)(pImageContainer + pBfhTo->bfOffBits),
                            pData,
                            (IntPtr)size);
                    }

                    this.transcodeIfYUV = transcodeIfYUV;
                }
            }
        }

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
                        var pBfh = (NativeMethods.RAW_BITMAPFILEHEADER*)pImageContainer;
                        var pBih = (NativeMethods.RAW_BITMAPINFOHEADER*)(pBfh + 1);

                        if (BitmapConverter.GetRequiredBufferSize(
                            pBih->biWidth, pBih->biHeight, pBih->biCompression) is { } sizeImage)
                        {
                            var totalSize =
                                sizeof(NativeMethods.RAW_BITMAPFILEHEADER) +
                                sizeof(NativeMethods.RAW_BITMAPINFOHEADER) +
                                sizeImage;

                            if (this.transcodedImageContainer == null ||
                                this.transcodedImageContainer.Length != totalSize)
                            {
                                this.transcodedImageContainer = new byte[totalSize];
                            }

                            fixed (byte* pTranscodedImageContainer = this.transcodedImageContainer)
                            {
                                var pBfhTo = (NativeMethods.RAW_BITMAPFILEHEADER*)pTranscodedImageContainer;
                                var pBihTo = (NativeMethods.RAW_BITMAPINFOHEADER*)(pBfhTo + 1);

                                pBfhTo->bfType0 = 0x42;
                                pBfhTo->bfType1 = 0x4d;
                                pBfhTo->bfSize = totalSize;

                                pBfhTo->bfOffBits =
                                    sizeof(NativeMethods.RAW_BITMAPFILEHEADER) +
                                    sizeof(NativeMethods.RAW_BITMAPINFOHEADER);

                                pBihTo->biSize = sizeof(NativeMethods.RAW_BITMAPINFOHEADER);
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
                                BitmapConverter.Convert(
                                    pBih->biWidth, pBih->biHeight,
                                    pBih->biCompression, false,
                                    pImageContainer + pBfh->bfOffBits,
                                    pTranscodedImageContainer + pBfhTo->bfOffBits);

#if DEBUG
                                Debug.WriteLine($"Convert: {sw.Elapsed}");
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
