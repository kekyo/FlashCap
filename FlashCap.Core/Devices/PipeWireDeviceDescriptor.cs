////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////


#if !(NET35 || NET40 || NET45 || NETSTANDARD1_3)
using FlashCap.Internal.PipeWire;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Devices;

/// <summary>Describes a camera accessible through the desktop camera portal.</summary>
[SupportedOSPlatform("linux")]
public sealed class PipeWireDeviceDescriptor : CaptureDeviceDescriptor
{
    private readonly PipeWireCamera camera;

    internal PipeWireDeviceDescriptor(PipeWireCamera camera, BufferPool bufferPool) :
        base(camera.Name, camera.Description, camera.Formats.Select(format => format.Characteristics).ToArray(), bufferPool) =>
        this.camera = camera;

    /// <summary>Gets the PipeWire object serial and node name identifying this camera.</summary>
    public override object Identity => "pipewire:" + this.camera.Serial + ":" + this.camera.NodeName;

    /// <summary>Gets the PipeWire backend type.</summary>
    public override DeviceTypes DeviceType => DeviceTypes.PipeWire;

    /// <summary>Opens this camera with the selected format and frame processor.</summary>
    /// <param name="characteristics">A format advertised by this descriptor.</param>
    /// <param name="transcodeFormat">The desired color conversion.</param>
    /// <param name="frameProcessor">Receives captured frames.</param>
    /// <param name="ct">Cancels opening the camera.</param>
    /// <returns>The initialized capture device.</returns>
    protected override Task<CaptureDevice> OnOpenWithFrameProcessorAsync(
        VideoCharacteristics characteristics, TranscodeFormats transcodeFormat,
        FrameProcessor frameProcessor, CancellationToken ct)
    {
        var format = this.camera.Formats.FirstOrDefault(candidate => candidate.Characteristics.Equals(characteristics));
        if (format is null) throw new ArgumentException("The camera does not advertise this format.", nameof(characteristics));
        return this.InternalOnOpenWithFrameProcessorAsync(
            new PipeWireCaptureDevice(this.Identity, this.camera, format),
            characteristics, transcodeFormat, frameProcessor, ct);
    }
}
#endif
