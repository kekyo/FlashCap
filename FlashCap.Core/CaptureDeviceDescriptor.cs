////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Threading.Tasks;

namespace FlashCap;

public enum DeviceTypes
{
    VideoForWindows,
    DirectShow,
    V4L2,
}

public delegate void PixelBufferArrivedDelegate(
    PixelBufferScope bufferScope);

public delegate Task PixelBufferArrivedTaskDelegate(
    PixelBufferScope bufferScope);

public abstract class CaptureDeviceDescriptor
{
    protected CaptureDeviceDescriptor(
        string name, string description,
        VideoCharacteristics[] characteristics)
    {
        this.Name = name;
        this.Description = description;
        this.Characteristics = characteristics;
    }

    public abstract object Identity { get; }
    public abstract DeviceTypes DeviceType { get; }
    public string Name { get; }
    public string Description { get; }
    public VideoCharacteristics[] Characteristics { get; }

    protected abstract Task<CaptureDevice> OnOpenWithFrameProcessorAsync(
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        FrameProcessor frameProcessor);

    internal Task<CaptureDevice> InternalOpenWithFrameProcessorAsync(
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        FrameProcessor frameProcessor) =>
        this.OnOpenWithFrameProcessorAsync(characteristics, transcodeIfYUV, frameProcessor);

    public override string ToString() =>
        $"{this.Name}: {this.Description}, Characteristics={this.Characteristics.Length}";
}
