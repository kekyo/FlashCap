////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Devices;

#if NET6_0_OR_GREATER // Remove this when dropping net5.0. Its required because RequiresUnreferencedCode was not allowed on class level in net5.0
[RequiresUnreferencedCode("Direct show depends on COM runtime generation and might require unreferenced code.")]
#endif
[SupportedOSPlatform("windows")]
public sealed class DirectShowDeviceDescriptor : CaptureDeviceDescriptor
{
    private readonly string devicePath;

    internal DirectShowDeviceDescriptor(
        string devicePath, string name, string description,
        VideoCharacteristics[] characteristics,
        BufferPool defaultBufferPool) :
        base(name, description, characteristics, defaultBufferPool) =>
        this.devicePath = devicePath;

    public override object Identity =>
        this.devicePath;

    public override DeviceTypes DeviceType =>
        DeviceTypes.DirectShow;

    protected override Task<CaptureDevice> OnOpenWithFrameProcessorAsync(
        VideoCharacteristics characteristics,
        TranscodeFormats transcodeFormat,
        FrameProcessor frameProcessor,
        CancellationToken ct) =>
        this.InternalOnOpenWithFrameProcessorAsync(
            new DirectShowDevice(this.devicePath, this.Name),
            characteristics, transcodeFormat, frameProcessor, ct);
}
