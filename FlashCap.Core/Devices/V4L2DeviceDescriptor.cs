////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Devices;

public sealed class V4L2DeviceDescriptor : CaptureDeviceDescriptor
{
    private readonly string devicePath;

    internal V4L2DeviceDescriptor(
        string devicePath, string name, string description,
        VideoCharacteristics[] characteristics,
        BufferPool defaultBufferPool) :
        base(name, description, characteristics, defaultBufferPool) =>
        this.devicePath = devicePath;

    public override object Identity =>
        this.devicePath;

    public override DeviceTypes DeviceType =>
        DeviceTypes.V4L2;

    protected override Task<CaptureDevice> OnOpenWithFrameProcessorAsync(
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        FrameProcessor frameProcessor,
        CancellationToken ct) =>
        this.InternalOnOpenWithFrameProcessorAsync(
            new V4L2Device(this.devicePath, this.Name),
            characteristics, transcodeFormat, frameProcessor, ct);
}
