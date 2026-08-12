#if FLASHCAP_MEDIAFOUNDATION
////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using FlashCap.Internal.MediaFoundation;

namespace FlashCap.Devices;

[SupportedOSPlatform("windows6.1")]
public sealed class MediaFoundationDeviceDescriptor : CaptureDeviceDescriptor
{
    private readonly string symbolicLink;
    private readonly IReadOnlyDictionary<VideoCharacteristics, MediaFoundationInterop.FormatKey> characteristicToFormatLookup;

    internal MediaFoundationDeviceDescriptor(
        string symbolicLink,
        string name,
        string description,
        IReadOnlyDictionary<VideoCharacteristics, MediaFoundationInterop.FormatKey> characteristicToFormatLookup,
        BufferPool defaultBufferPool) :
        base(name, description, characteristicToFormatLookup.Keys.ToArray(), defaultBufferPool)
    {
        this.symbolicLink = symbolicLink;
        this.characteristicToFormatLookup = characteristicToFormatLookup;
    }

    public override object Identity => this.symbolicLink;

    public override DeviceTypes DeviceType => DeviceTypes.MediaFoundation;

    protected override Task<CaptureDevice> OnOpenWithFrameProcessorAsync(
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        FrameProcessor frameProcessor,
        CancellationToken ct)
    {
        if (!this.characteristicToFormatLookup.TryGetValue(characteristics, out var formatKey))
        {
            throw new System.ArgumentException(
                "FlashCap: The selected Media Foundation format is not available.",
                nameof(characteristics));
        }

        return this.InternalOnOpenWithFrameProcessorAsync(
            new MediaFoundationDevice(this.symbolicLink, this.Name, formatKey),
            characteristics,
            transcodeFormat,
            frameProcessor,
            ct);
    }
}
#endif
