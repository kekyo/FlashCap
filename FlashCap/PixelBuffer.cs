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
using System.Runtime.InteropServices;

namespace FlashCap
{
    public sealed class PixelBuffer
    {
        private byte[]? imageContainer;
        private byte[]? alternateImageContainer = null;
        private bool holdRawData;

        public PixelBuffer()
        {
        }

        internal unsafe void CopyIn(
            in NativeMethods.BITMAPINFOHEADER bih, IntPtr pData, int size, bool holdRawData)
        {
            var totalSize = bih.biCompression switch
            {
                NativeMethods.CompressionModes.BI_JPEG => size,
                NativeMethods.CompressionModes.BI_PNG => size,
                _ => sizeof(NativeMethods.BITMAPFILEHEADER) + size
            };

            lock (this)
            {
                if (this.imageContainer == null ||
                    this.imageContainer.Length != totalSize)
                {
                    this.imageContainer = new byte[totalSize];
                    this.alternateImageContainer = null;
                }

                if (bih.biCompression == NativeMethods.CompressionModes.BI_JPEG ||
                    bih.biCompression == NativeMethods.CompressionModes.BI_PNG)
                {
                    Marshal.Copy(
                        pData,
                        this.imageContainer!,
                        0,
                        size);

                    this.holdRawData = true;
                }
                else
                {
                    fixed (byte* pImageContainer = &this.imageContainer![0])
                    {
                        var pBfh = (NativeMethods.BITMAPFILEHEADER*)pImageContainer;
                        pBfh->bfType0 = 0x42;
                        pBfh->bfType1 = 0x4d;
                        pBfh->bfSize = totalSize;
                        pBfh->bfOffBits = sizeof(NativeMethods.BITMAPFILEHEADER);
                        pBfh->bih = bih;
                    }

                    Marshal.Copy(
                        pData,
                        this.imageContainer!,
                        sizeof(NativeMethods.BITMAPFILEHEADER),
                        size);

                    this.holdRawData = holdRawData;
                }
            }
        }

        public unsafe byte[] ExtractImage()
        {
            lock (this)
            {
                if (!this.holdRawData)
                {
                    fixed (byte* pImageContainer = &this.imageContainer![0])
                    {
                        var pBfh = (NativeMethods.BITMAPFILEHEADER*)pImageContainer;

                        if (BitmapConverter.GetRequiredBufferSize(
                            pBfh->bih.biWidth, pBfh->bih.biHeight, pBfh->bih.biCompression) is { } size)
                        {
                            var totalSize = sizeof(NativeMethods.BITMAPFILEHEADER) + size;

                            if (this.alternateImageContainer == null ||
                                this.alternateImageContainer.Length != totalSize)
                            {
                                this.alternateImageContainer = new byte[totalSize];
                            }

                            fixed (byte* pAlternateImageContainer = &this.alternateImageContainer![0])
                            {
                                var pBfhAlternate = (NativeMethods.BITMAPFILEHEADER*)pAlternateImageContainer;

                                *pBfhAlternate = *pBfh;

                                pBfhAlternate->bfSize = totalSize;
                                pBfhAlternate->bih.biSize = sizeof(NativeMethods.BITMAPINFOHEADER);
                                pBfhAlternate->bih.biPlanes = 1;
                                pBfhAlternate->bih.biBitCount = 24;   // B8G8R8
                                pBfhAlternate->bih.biCompression = NativeMethods.CompressionModes.BI_RGB;
                                pBfhAlternate->bih.biSizeImage = size;
                                pBfhAlternate->bih.biClrImportant = 0;
                                pBfhAlternate->bih.biClrUsed = 0;
#if DEBUG
                                var sw = new Stopwatch();
                                sw.Start();
#endif
                                BitmapConverter.Convert(
                                    pBfh->bih.biWidth, pBfh->bih.biHeight,
                                    pBfh->bih.biCompression, false,
                                    pImageContainer + sizeof(NativeMethods.BITMAPFILEHEADER),
                                    pAlternateImageContainer + sizeof(NativeMethods.BITMAPFILEHEADER));

#if DEBUG
                                Debug.WriteLine($"Convert: {sw.Elapsed}");
#endif
                            }

                            return this.alternateImageContainer!;
                        }
                    }
                }
                return this.imageContainer!;
            }
        }
    }
}
