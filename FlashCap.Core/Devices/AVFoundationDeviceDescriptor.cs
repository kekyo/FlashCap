////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Yoh Deadfall (@YohDeadfall)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Devices;

public sealed class AVFoundationDeviceDescriptor : CaptureDeviceDescriptor
{
    private readonly string uniqueId;

    internal AVFoundationDeviceDescriptor(
        string uniqueId, string modelId, string localizedName,
        VideoCharacteristics[] characteristics,
        BufferPool defaultBufferPool) :
        base(modelId, localizedName, characteristics, defaultBufferPool) =>
        this.uniqueId = uniqueId;

    public override object Identity =>
        this.uniqueId;

    public override DeviceTypes DeviceType =>
        DeviceTypes.AVFoundation;

    protected override Task<CaptureDevice> OnOpenWithFrameProcessorAsync(
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        FrameProcessor frameProcessor,
        CancellationToken ct) =>
        this.InternalOnOpenWithFrameProcessorAsync(
            new AVFoundationDevice(this.uniqueId, this.Name),
            characteristics, transcodeFormat, frameProcessor, ct);
}
