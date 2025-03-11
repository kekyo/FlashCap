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
    
    public static SKBitmap CreateSKBitmapARGB(byte[] argb32Data, int width, int height)
    {
        //if (argb32Data.Length != width * height * 4) // 4 bytes per pixel (ARGB)
        //    throw new ArgumentException("Invalid ARGB32 data size for given width and height.");

        // Convert ARGB32 to RGBA32 (SkiaSharp expects RGBA format)
        byte[] rgba32Data = new byte[argb32Data.Length];

        for (int i = 0; i < width * height; i++)
        {
            int index = i * 4;
            byte R = argb32Data[index];     // Alpha
            byte G = argb32Data[index + 1]; // Red
            byte A = argb32Data[index + 2]; // Green
            byte B = argb32Data[index + 3]; // Blue

            // Rearrange to RGBA format for SkiaSharp
            rgba32Data[index] = R;
            rgba32Data[index + 1] = G;
            rgba32Data[index + 2] = B;
            rgba32Data[index + 3] = A;
        }

        // Create a SkiaSharp bitmap
        SKBitmap bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

        // Pin the byte array in memory
        GCHandle handle = GCHandle.Alloc(rgba32Data, GCHandleType.Pinned);
        IntPtr ptr = handle.AddrOfPinnedObject();

        try
        {
            // Copy pixel data into the bitmap
            bitmap.InstallPixels(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul), ptr);
        }
        finally
        {
            handle.Free(); // Release memory after use
        }

        return bitmap;
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