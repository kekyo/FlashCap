using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;


namespace FlashCap.Avalonia.Utilities;



public static class ImageTools
{
    public enum GuessedPixelFormat
    {
        Unknown,
        Grayscale8,  // 8 bits per pixel (1 byte per pixel)
        RGB24,       // 24-bit (3 bytes per pixel, no alpha)
        RGBA32,      // 32-bit (4 bytes per pixel, includes alpha)
        ARGB32,      // 32-bit (ARGB order)
        Grayscale16, // 16-bit grayscale (2 bytes per pixel)
        CMYK32,      // 32-bit CMYK (common in print)
    }
    
    public static GuessedPixelFormat DetectPixelFormat(ArraySegment<byte> imageData, int width, int height)
    {
        if (imageData.Array == null || imageData.Count == 0)
            return GuessedPixelFormat.Unknown;

        int byteCount = imageData.Count;
        int expectedSize = width * height;

        // Determine probable bytes per pixel
        int bpp = byteCount / expectedSize;

        switch (bpp)
        {
            case 1:
                return GuessedPixelFormat.Grayscale8; // 1 byte per pixel (grayscale)
            case 2:
                return GuessedPixelFormat.Grayscale16; // 2 bytes per pixel (high-precision grayscale)
            case 3:
                return IsGrayscale(imageData, width, height, 3) ? GuessedPixelFormat.Grayscale8 : GuessedPixelFormat.RGB24;
            case 4:
                return DetectRGBAorARGB(imageData, width, height);
                //return HasAlphaChannel(imageData, width, height, 4) ? GuessedPixelFormat.RGBA32 : GuessedPixelFormat.RGB24;
            default:
                return GuessedPixelFormat.Unknown;
        }
    }
    
    private static GuessedPixelFormat DetectRGBAorARGB(ArraySegment<byte> imageData, int width, int height)
    {
        int pixelCount = width * height;
        int offset = imageData.Offset;
        byte[] data = imageData.Array;

        int rgbaMatch = 0;
        int argbMatch = 0;

        for (int i = 0; i < pixelCount; i++)
        {
            int index = offset + i * 4;
            byte a = data[index];     // Alpha (ARGB first byte)
            byte r = data[index + 1]; // Red
            byte g = data[index + 2]; // Green
            byte b = data[index + 3]; // Blue

            byte r2 = data[index];     // Red (RGBA first byte)
            byte g2 = data[index + 1]; // Green
            byte b2 = data[index + 2]; // Blue
            byte a2 = data[index + 3]; // Alpha (RGBA last byte)

            // Checking ARGB Pattern: If the first byte (A) is either 0 (transparent) or 255 (fully opaque) often
            if ((a == 0 || a == 255) && r != 0 && g != 0 && b != 0)
            {
                argbMatch++;
            }

            // Checking RGBA Pattern: If the last byte (A2) follows the alpha behavior
            if ((a2 == 0 || a2 == 255) && r2 != 0 && g2 != 0 && b2 != 0)
            {
                rgbaMatch++;
            }
        }

        return (argbMatch > rgbaMatch) ? GuessedPixelFormat.ARGB32 : GuessedPixelFormat.RGBA32;
    }

    private static bool IsGrayscale(ArraySegment<byte> imageData, int width, int height, int bpp)
    {
        int pixelCount = width * height;

        for (int i = 0; i < pixelCount; i++)
        {
            int index = i * bpp;
            byte r = imageData.Array[imageData.Offset + index];
            byte g = imageData.Array[imageData.Offset + index + 1];
            byte b = imageData.Array[imageData.Offset + index + 2];

            if (r != g || g != b)
                return false; // If R, G, B are not equal, it's not grayscale
        }
        return true;
    }

    private static bool HasAlphaChannel(ArraySegment<byte> imageData, int width, int height, int bpp)
    {
        int pixelCount = width * height;

        for (int i = 0; i < pixelCount; i++)
        {
            int index = i * bpp;
            byte alpha = imageData.Array[imageData.Offset + index + 3];

            if (alpha != 255) // If alpha is not fully opaque anywhere, it has an alpha channel
                return true;
        }
        return false;
    }
    
    public static Bitmap DecodeYUYV(byte[] yuyvData, int width, int height)
    {
        if (yuyvData.Length != width * height * 2)
            throw new ArgumentException("Invalid YUYV data size for given width and height.");

        // Create an array for RGB pixel data (3 bytes per pixel: R, G, B)
        byte[] rgbData = new byte[width * height * 3];

        int yuyvIndex = 0;
        int rgbIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x += 2)
            {
                // Extract YUYV values (each pair of pixels shares U and V)
                byte Y0 = yuyvData[yuyvIndex];
                byte U = yuyvData[yuyvIndex + 1];
                byte Y1 = yuyvData[yuyvIndex + 2];
                byte V = yuyvData[yuyvIndex + 3];

                yuyvIndex += 4; // Move to the next YUYV block

                // Convert first pixel (Y0, U, V)
                (rgbData[rgbIndex], rgbData[rgbIndex + 1], rgbData[rgbIndex + 2]) = YUVToRGB(Y0, U, V);
                rgbIndex += 3;

                // Convert second pixel (Y1, U, V)
                (rgbData[rgbIndex], rgbData[rgbIndex + 1], rgbData[rgbIndex + 2]) = YUVToRGB(Y1, U, V);
                rgbIndex += 3;
            }
        }

        // Create an Avalonia Bitmap from RGB data
        return CreateAvaloniaBitmap(rgbData, width, height);
    }
    
    public static SKBitmap DecodeYuyvSkBitmap(byte[] yuyvData, int width, int height)
    {
        //if (yuyvData.Length != width * height * 2)
        //    throw new ArgumentException("Invalid YUYV data size for given width and height.");

        // Create a pixel buffer for RGB format (3 bytes per pixel: R, G, B)
        byte[] rgbData = new byte[width * height * 3];

        int yuyvIndex = 0;
        int rgbIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x += 2)
            {
                // Extract YUYV values (each pair of pixels shares U and V)
                byte Y0 = yuyvData[yuyvIndex];
                byte U = yuyvData[yuyvIndex + 1];
                byte Y1 = yuyvData[yuyvIndex + 2];
                byte V = yuyvData[yuyvIndex + 3];

                yuyvIndex += 4; // Move to the next YUYV block

                // Convert first pixel (Y0, U, V)
                (rgbData[rgbIndex], rgbData[rgbIndex + 1], rgbData[rgbIndex + 2]) = YUVToRGB(Y0, U, V);
                rgbIndex += 3;

                // Convert second pixel (Y1, U, V)
                (rgbData[rgbIndex], rgbData[rgbIndex + 1], rgbData[rgbIndex + 2]) = YUVToRGB(Y1, U, V);
                rgbIndex += 3;
            }
        }

        // Create an SKBitmap from RGB data
        return CreateSKBitmap(rgbData, width, height);
    }

    private static (byte R, byte G, byte B) YUVToRGB(byte y, byte u, byte v)
    {
        int C = y;
        int D = u - 128;
        int E = v - 128;

        // Convert YUV to RGB using standard formula
        int R = Clamp(C + (int)(1.402 * E));
        int G = Clamp(C - (int)(0.344136 * D) - (int)(0.714136 * E));
        int B = Clamp(C + (int)(1.772 * D));

        return ((byte)R, (byte)G, (byte)B);
    }

    private static int Clamp(int value)
    {
        return Math.Max(0, Math.Min(255, value));
    }
    

    private static SKBitmap CreateSKBitmap(byte[] rgbData, int width, int height)
    {
        // Create a SkiaSharp bitmap
        SKBitmap bitmap = new SKBitmap(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque);

        // Pin the byte array in memory
        GCHandle handle = GCHandle.Alloc(rgbData, GCHandleType.Pinned);
        IntPtr ptr = handle.AddrOfPinnedObject();

        try
        {
            // Copy pixel data into the bitmap
            bitmap.InstallPixels(new SKImageInfo(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque), ptr);
        }
        finally
        {
            handle.Free(); // Ensure we release the memory
        }

        return bitmap;
    }
    
    public static SKBitmap CreateSKBitmap32(byte[] rgb32Data, int width, int height)
    {
        if (rgb32Data.Length < (width * height * 4)) // 4 bytes per pixel (RGBA)
            throw new ArgumentException("Invalid RGB32 data size for given width and height.");

        // Create an SkiaSharp bitmap
        SKBitmap bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

        // Pin the byte array in memory
        GCHandle handle = GCHandle.Alloc(rgb32Data, GCHandleType.Pinned);
        IntPtr ptr = handle.AddrOfPinnedObject();

        try
        {
            // Copy pixel data into the bitmap
            bitmap.InstallPixels(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul), ptr);
        }
        finally
        {
            handle.Free(); // Ensure we release the memory
        }

        return bitmap;
    }
    
    public static SKBitmap CreateSKBitmapARGB(ArraySegment<byte> argb32Data, int width, int height)
    {
        try
        {
            Span<byte> span = argb32Data.AsSpan();
            
            //if (argb32Data.Length != width * height * 4) // 4 bytes per pixel (ARGB)
            //    throw new ArgumentException("Invalid ARGB32 data size for given width and height.");

            // Convert ARGB32 to RGBA32 (SkiaSharp expects RGBA format)
            //byte[] rgba32Data = new byte[argb32Data.Length];
            byte[] rgba32Data = new byte[width * height * 4];

            for (int i = 0; i < width * height; i++)
            {
                int index = i * 4;
                byte R = span[index]; // Alpha
                byte G = span[index + 1]; // Red
                byte A = span[index + 2]; // Green
                byte B = span[index + 3]; // Blue

                // Rearrange to RGBA format for SkiaSharp
                rgba32Data[index] = R;
                rgba32Data[index + 1] = G;
                rgba32Data[index + 2] = B;
                rgba32Data[index + 3] = A;
            }

            // Allocate SkiaSharp bitmap
            SKBitmap bitmap = new SKBitmap();
            SKImageInfo info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

            // Pin the byte array in memory
            GCHandle handle = GCHandle.Alloc(rgba32Data, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();


            try
            {
                // Install pixels with a release delegate
                bitmap.InstallPixels(info, ptr, info.RowBytes, (addr, context) =>
                {
                    // FIX: Properly cast object to IntPtr before using FromIntPtr
                    if (context is IntPtr ctxPtr)
                    {
                        GCHandle gch = GCHandle.FromIntPtr(ctxPtr);
                        if (gch.IsAllocated)
                        {
                            gch.Free(); // Free the memory safely
                        }
                    }
                }, GCHandle.ToIntPtr(handle)); 
            }
            catch
            {
                handle.Free(); // Ensure memory is freed in case of failure
                throw;
            }

            return bitmap;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error converting image: " + ex.Message);
            throw;
        }
    }

    private static Bitmap CreateAvaloniaBitmap(byte[] rgbData, int width, int height)
    {
        // Create an Avalonia Bitmap using a memory stream
        using var memoryStream = new MemoryStream(rgbData);
        var writeableBitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Rgb32, AlphaFormat.Opaque);

        using (var framebuffer = writeableBitmap.Lock())
        {
            Marshal.Copy(rgbData, 0, framebuffer.Address, rgbData.Length);
        }

        return writeableBitmap;
    }
}