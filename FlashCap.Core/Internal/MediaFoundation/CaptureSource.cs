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
using System.Runtime.Versioning;
using static FlashCap.Internal.NativeMethods_MediaFoundation;
using static FlashCap.Internal.MediaFoundation.MediaFoundationInterop;

namespace FlashCap.Internal.MediaFoundation;

/// <summary>
/// Owns the camera and its controls between OpenAsync and DisposeAsync, independently of capture sessions.
/// All methods run on the device's Media Foundation worker.
/// </summary>
[SupportedOSPlatform("windows6.1")]
internal sealed unsafe class CaptureSource : IDisposable
{
    private IMFActivate* activate;
    private IMFMediaSource* mediaSource;
    private CameraControls? controls;

    internal CameraControls Controls => this.controls ?? throw new ObjectDisposedException(nameof(CaptureSource));

    internal static CaptureSource Open(string symbolicLink)
    {
        var source = new CaptureSource();
        try
        {
            source.activate = FindActivate(symbolicLink);
            source.mediaSource = ActivateMediaSource(source.activate);
            source.controls = new CameraControls((IUnknown*)source.mediaSource);
            return source;
        }
        catch
        {
            source.Dispose();
            throw;
        }
    }

    internal CaptureSession OpenSession(FormatKey formatKey, FrameHandler frameHandler)
    {
        if (this.mediaSource is null)
        {
            throw new ObjectDisposedException(nameof(CaptureSource));
        }
        return CaptureSession.Open(this.mediaSource, formatKey, frameHandler);
    }

    public void Dispose()
    {
        this.controls?.Dispose();
        this.controls = null;
        if (this.activate is not null)
        {
            _ = this.activate->ShutdownObject();
        }
        Release(this.mediaSource);
        this.mediaSource = null;
        Release(this.activate);
        this.activate = null;
    }

    private static IMFActivate* FindActivate(string symbolicLink)
    {
        IMFActivate** devices = null;
        uint count = 0;
        try
        {
            devices = EnumerateDeviceSources(out count);
            for (uint index = 0; index < count; index++)
            {
                var activate = devices[index];
                if (activate is not null && string.Equals(
                        GetAllocatedString(activate, in MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK).Trim(),
                        symbolicLink, StringComparison.OrdinalIgnoreCase))
                {
                    devices[index] = null;
                    return activate;
                }
            }
        }
        finally
        {
            FreeActivateArray(devices, count);
        }
        throw new InvalidOperationException("FlashCap: The Media Foundation device is no longer available.");
    }
}
#endif
