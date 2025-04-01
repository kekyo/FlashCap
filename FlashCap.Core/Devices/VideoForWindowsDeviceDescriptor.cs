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

public sealed class VideoForWindowsDeviceDescriptor : CaptureDeviceDescriptor
{
    private readonly int deviceIndex;

    internal VideoForWindowsDeviceDescriptor(
        int deviceIndex, string name, string description,
        VideoCharacteristics[] characteristics,
        BufferPool defaultBufferPool) :
        base(name, description, characteristics, defaultBufferPool) =>
        this.deviceIndex = deviceIndex;

    public override object Identity =>
        this.deviceIndex;

    public override DeviceTypes DeviceType =>
        DeviceTypes.VideoForWindows;

    protected override Task<CaptureDevice> OnOpenWithFrameProcessorAsync(
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        FrameProcessor frameProcessor,
        CancellationToken ct) =>
        this.InternalOnOpenWithFrameProcessorAsync(
            new VideoForWindowsDevice(this.deviceIndex, this.Name),
            characteristics, transcodeFormat, frameProcessor, ct);
}
