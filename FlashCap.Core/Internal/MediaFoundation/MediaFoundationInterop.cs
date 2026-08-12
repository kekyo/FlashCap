#if FLASHCAP_MEDIAFOUNDATION
////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Media.MediaFoundation;
using Windows.Win32.System.Com;
using FlashCap.Utilities;
using static FlashCap.Internal.MediaFoundation.MediaFoundationHelpers;

namespace FlashCap.Internal.MediaFoundation;

[SupportedOSPlatform("windows6.1")]
internal static unsafe class MediaFoundationInterop
{
    internal const uint VideoStreamIndex =
        unchecked((uint)MF_SOURCE_READER_CONSTANTS.MF_SOURCE_READER_FIRST_VIDEO_STREAM);

    internal readonly record struct FormatKey(
        uint MediaTypeIndex,
        Guid Subtype,
        int Width,
        int Height,
        Fraction FrameRate);

    internal readonly record struct Format(
        FormatKey Key,
        VideoCharacteristics Characteristics);

    internal readonly record struct DeviceInfo(
        string SymbolicLink,
        string Name,
        IReadOnlyDictionary<VideoCharacteristics, FormatKey> Formats);

    internal readonly record struct FrameLayout(
        int RowLength,
        int Rows,
        int SourceStride,
        int TargetStride,
        bool BottomUp)
    {
        internal int TargetLength => checked(this.TargetStride * this.Rows);
    }

    internal delegate void FrameHandler(
        byte* data,
        int length,
        int? defaultStride,
        long timestampMicroseconds,
        long frameIndex);

    /// <summary>
    /// Initializes COM as MTA and starts Media Foundation on the current thread.
    /// After this method succeeds, call <see cref="Uninitialize"/> in a <c>finally</c> block on the same thread.
    /// </summary>
    internal static void Initialize()
    {
        var result = PInvoke.CoInitializeEx(COINIT.COINIT_MULTITHREADED);
        ThrowIfFailed(result, nameof(PInvoke.CoInitializeEx));

        result = PInvoke.MFStartup(PInvoke.MF_VERSION, PInvoke.MFSTARTUP_FULL);
        if (result.Failed)
        {
            PInvoke.CoUninitialize();
            ThrowIfFailed(result, nameof(PInvoke.MFStartup));
        }
    }

    internal static void Uninitialize()
    {
        _ = PInvoke.MFShutdown();
        PInvoke.CoUninitialize();
    }

    internal static IMFAttributes* CreateVideoCaptureAttributes()
    {
        IMFAttributes* attributes = null;
        ThrowIfFailed(PInvoke.MFCreateAttributes(&attributes, 1), nameof(PInvoke.MFCreateAttributes));
        try
        {
            ThrowIfFailed(
                attributes->SetGUID(
                    in PInvoke.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE,
                    in PInvoke.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID),
                "IMFAttributes.SetGUID");
            return attributes;
        }
        catch
        {
            Release(attributes);
            throw;
        }
    }

    internal static IMFActivate** EnumerateDeviceSources(out uint count)
    {
        IMFAttributes* attributes = CreateVideoCaptureAttributes();
        try
        {
            ThrowIfFailed(
                PInvoke.MFEnumDeviceSources(attributes, out var devices, out count),
                nameof(PInvoke.MFEnumDeviceSources));
            return devices;
        }
        finally
        {
            Release(attributes);
        }
    }

    internal static DeviceInfo[] EnumerateDevices()
    {
        Initialize();

        IMFActivate** devices = null;
        uint count = 0;
        try
        {
            devices = EnumerateDeviceSources(out count);
            var results = new List<DeviceInfo>(checked((int)count));
            for (uint index = 0; index < count; index++)
            {
                var activate = devices[index];
                devices[index] = null;
                if (activate is null)
                {
                    continue;
                }

                try
                {
                    var symbolicLink = GetAllocatedString(
                        activate,
                        in PInvoke.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK).Trim();
                    if (string.IsNullOrEmpty(symbolicLink))
                    {
                        continue;
                    }

                    var name = GetAllocatedString(
                        activate,
                        in PInvoke.MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME).Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        name = "Media Foundation camera";
                    }

                    var formats = EnumerateDeviceFormats(activate);
                    if (formats.Count != 0)
                    {
                        results.Add(new DeviceInfo(symbolicLink, name, formats));
                    }
                }
                catch (Exception exception)
                {
                    TraceFailure("device inspection", exception);
                }
                finally
                {
                    _ = activate->ShutdownObject();
                    Release(activate);
                }
            }
            return results.ToArray();
        }
        finally
        {
            FreeActivateArray(devices, count);
            Uninitialize();
        }
    }

    private static IReadOnlyDictionary<VideoCharacteristics, FormatKey> EnumerateDeviceFormats(
        IMFActivate* activate)
    {
        IMFMediaSource* mediaSource = null;
        IMFSourceReader* reader = null;
        try
        {
            mediaSource = ActivateMediaSource(activate);
            reader = CreateSourceReader(mediaSource);
            var formats = new Dictionary<VideoCharacteristics, FormatKey>();
            foreach (var format in EnumerateFormats(reader))
            {
                if (!formats.ContainsKey(format.Characteristics))
                {
                    formats.Add(format.Characteristics, format.Key);
                }
            }
            return formats;
        }
        finally
        {
            Release(reader);
            if (reader is null && mediaSource is not null)
            {
                _ = mediaSource->Shutdown();
            }
            Release(mediaSource);
        }
    }

    internal static string GetAllocatedString(IMFActivate* activate, in Guid key)
    {
        var result = activate->GetAllocatedString(in key, out var value, out _);
        if (result.Failed || value.Value is null)
        {
            return string.Empty;
        }

        try
        {
            return Marshal.PtrToStringUni((IntPtr)value.Value) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeCoTaskMem((IntPtr)value.Value);
        }
    }

    internal static IMFMediaSource* ActivateMediaSource(IMFActivate* activate)
    {
        ThrowIfFailed(
            activate->ActivateObject(in IMFMediaSource.IID_Guid, out var value),
            "IMFActivate.ActivateObject"
        );
        if (value is null)
        {
            throw new InvalidOperationException("FlashCap: Media Foundation returned no media source.");
        }
        return (IMFMediaSource*)value;
    }

    internal static IMFSourceReader* CreateSourceReader(IMFMediaSource* mediaSource)
        => CreateSourceReader(mediaSource, null);

    internal static IMFSourceReader* CreateSourceReader(IMFMediaSource* mediaSource, IUnknown* callback)
    {
        IMFSourceReader* reader = null;
        IMFAttributes* attributes = null;
        try
        {
            if (callback is not null)
            {
                ThrowIfFailed(PInvoke.MFCreateAttributes(&attributes, 1), nameof(PInvoke.MFCreateAttributes));
                ThrowIfFailed(
                    attributes->SetUnknown(in PInvoke.MF_SOURCE_READER_ASYNC_CALLBACK, callback),
                    "IMFAttributes.SetUnknown(async callback)");
            }
            ThrowIfFailed(
                PInvoke.MFCreateSourceReaderFromMediaSource(mediaSource, attributes, &reader),
                nameof(PInvoke.MFCreateSourceReaderFromMediaSource));
        }
        finally
        {
            Release(attributes);
        }
        if (reader is null)
        {
            throw new InvalidOperationException("FlashCap: Media Foundation returned no source reader.");
        }
        return reader;
    }

    internal static List<Format> EnumerateFormats(IMFSourceReader* reader)
    {
        var formats = new List<Format>();
        for (uint index = 0; ; index++)
        {
            IMFMediaType* mediaType = null;
            var result = reader->GetNativeMediaType(VideoStreamIndex, index, &mediaType);
            if (result == HRESULT.MF_E_NO_MORE_TYPES)
            {
                break;
            }
            ThrowIfFailed(result, "IMFSourceReader.GetNativeMediaType");
            try
            {
                if (mediaType is not null && TryCreateFormat(mediaType, index, out var format))
                {
                    formats.Add(format);
                }
            }
            finally
            {
                Release(mediaType);
            }
        }
        return formats;
    }

    internal static bool TryCreateFormat(IMFMediaType* mediaType, uint index, out Format format)
    {
        format = default;
        if (mediaType->GetGUID(in PInvoke.MF_MT_MAJOR_TYPE, out var majorType).Failed ||
            majorType != PInvoke.MFMediaType_Video ||
            mediaType->GetGUID(in PInvoke.MF_MT_SUBTYPE, out var subtype).Failed ||
            mediaType->GetUINT64(in PInvoke.MF_MT_FRAME_SIZE, out var frameSize).Failed ||
            mediaType->GetUINT64(in PInvoke.MF_MT_FRAME_RATE, out var frameRate).Failed)
        {
            return false;
        }

        var widthValue = (uint)(frameSize >> 32);
        var heightValue = (uint)frameSize;
        var numerator = (uint)(frameRate >> 32);
        var denominator = (uint)frameRate;
        if (widthValue is 0 or > int.MaxValue ||
            heightValue is 0 or > int.MaxValue ||
            numerator is 0 or > int.MaxValue ||
            denominator is 0 or > int.MaxValue ||
            !TryMapPixelFormat(subtype, out var pixelFormat, out var name))
        {
            return false;
        }

        var rate = new Fraction((int)numerator, (int)denominator);
        var characteristics = new VideoCharacteristics(
            pixelFormat, (int)widthValue, (int)heightValue, rate, name, true, name);
        format = new Format(
            new FormatKey(index, subtype, characteristics.Width, characteristics.Height, rate),
            characteristics);
        return true;
    }

    internal static bool TryMapPixelFormat(Guid subtype, out PixelFormats format, out string name)
    {
        if (subtype == PInvoke.MFVideoFormat_RGB24) { format = PixelFormats.RGB24; name = "RGB24"; return true; }
        if (subtype == PInvoke.MFVideoFormat_RGB32) { format = PixelFormats.RGB32; name = "RGB32"; return true; }
        if (subtype == PInvoke.MFVideoFormat_ARGB32) { format = PixelFormats.ARGB32; name = "ARGB32"; return true; }
        if (subtype == PInvoke.MFVideoFormat_RGB555) { format = PixelFormats.RGB15; name = "RGB555"; return true; }
        if (subtype == PInvoke.MFVideoFormat_RGB565) { format = PixelFormats.RGB16; name = "RGB565"; return true; }
        if (subtype == PInvoke.MFVideoFormat_MJPG) { format = PixelFormats.JPEG; name = "MJPG"; return true; }
        if (subtype == PInvoke.MFVideoFormat_UYVY) { format = PixelFormats.UYVY; name = "UYVY"; return true; }
        if (subtype == PInvoke.MFVideoFormat_YUY2) { format = PixelFormats.YUYV; name = "YUY2"; return true; }
        if (subtype == PInvoke.MFVideoFormat_NV12) { format = PixelFormats.NV12; name = "NV12"; return true; }
        format = PixelFormats.Unknown;
        name = subtype.ToString("D");
        return false;
    }

    internal static FrameLayout GetFrameLayout(
        PixelFormats format,
        int width,
        int height,
        int? defaultStride,
        int bufferLength)
    {
        var rowLength = format switch
        {
            PixelFormats.RGB24 => checked(width * 3),
            PixelFormats.RGB32 or PixelFormats.ARGB32 => checked(width * 4),
            PixelFormats.RGB15 or PixelFormats.RGB16 or PixelFormats.UYVY or PixelFormats.YUYV =>
                checked(width * 2),
            PixelFormats.NV12 => width,
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };
        var rows = checked(height + (format == PixelFormats.NV12 ? (height + 1) / 2 : 0));
        var rgb = format is PixelFormats.RGB15 or PixelFormats.RGB16 or
            PixelFormats.RGB24 or PixelFormats.RGB32 or PixelFormats.ARGB32;
        var bottomUp = defaultStride < 0 || defaultStride is null && rgb;
        var sourceStride = defaultStride is { } stride && stride != 0 ? Math.Abs(stride) :
            bufferLength % rows == 0 && bufferLength / rows >= rowLength ? bufferLength / rows : rowLength;
        var targetStride = rgb ? checked((rowLength + 3) & ~3) : rowLength;
        if (sourceStride < rowLength || (long)sourceStride * rows > bufferLength)
        {
            throw new ArgumentException("The frame buffer is truncated.", nameof(bufferLength));
        }
        return new FrameLayout(rowLength, rows, sourceStride, targetStride, bottomUp);
    }

    internal static void RepackFrame(
        ReadOnlySpan<byte> source,
        Span<byte> target,
        FrameLayout layout,
        bool reverseRows)
    {
        if ((long)layout.SourceStride * layout.Rows > source.Length || layout.TargetLength > target.Length)
        {
            throw new ArgumentException("The frame buffer is truncated.");
        }

        target.Slice(0, layout.TargetLength).Clear();
        for (var row = 0; row < layout.Rows; row++)
        {
            var sourceRow = reverseRows ? layout.Rows - row - 1 : row;
            source.Slice(sourceRow * layout.SourceStride, layout.RowLength).
                CopyTo(target.Slice(row * layout.TargetStride, layout.RowLength));
        }
    }

    

    internal static void FreeActivateArray(IMFActivate** devices, uint count)
    {
        if (devices is null)
        {
            return;
        }
        for (uint index = 0; index < count; index++)
        {
            Release(devices[index]);
        }
        Marshal.FreeCoTaskMem((IntPtr)devices);
    }

    internal static void Release<T>(T* value) where T : unmanaged
    {
        if (value is not null)
        {
            _ = ((IUnknown*)value)->Release();
        }
    }
}
#endif
