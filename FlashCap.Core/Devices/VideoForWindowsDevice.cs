////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;

namespace FlashCap.Devices;

public sealed class VideoForWindowsDevice : CaptureDevice
{
    private readonly TimestampCounter counter = new();
    private int deviceIndex;
    private bool transcodeIfYUV;
    private FrameProcessor frameProcessor;
    private long frameIndex;

    private IndependentSingleApartmentContext? workingContext = new();
    private IntPtr handle;
    private GCHandle thisPin;
    private NativeMethods_VideoForWindows.CAPVIDEOCALLBACK? callback;
    private IntPtr pBih;

#pragma warning disable CS8618
    internal VideoForWindowsDevice()
#pragma warning restore CS8618
    {
    }

    protected override unsafe Task OnInitializeAsync(
        object identity,
        VideoCharacteristics characteristics,
        bool transcodeIfYUV,
        FrameProcessor frameProcessor,
        CancellationToken ct)
    {
        this.deviceIndex = (int)identity;
        this.Characteristics = characteristics;
        this.transcodeIfYUV = transcodeIfYUV;
        this.frameProcessor = frameProcessor;

        if (!NativeMethods.GetCompressionAndBitCount(
            characteristics.PixelFormat, out var compression, out var bitCount))
        {
            throw new ArgumentException(
                $"FlashCap: Couldn't set video format [1]: DeviceIndex={deviceIndex}");
        }

        return this.workingContext!.InvokeAsync(() =>
        {
            this.handle = NativeMethods_VideoForWindows.CreateVideoSourceWindow(deviceIndex);

            NativeMethods_VideoForWindows.capDriverConnect(this.handle, this.deviceIndex);

            ///////////////////////////////////////

            NativeMethods_VideoForWindows.capSetPreviewScale(this.handle, false);
            NativeMethods_VideoForWindows.capSetPreviewFPS(this.handle, 15);
            NativeMethods_VideoForWindows.capSetOverlay(this.handle, true);

            ///////////////////////////////////////

            // At first set 5fps, because can't set both fps and video format atomicity.
            NativeMethods_VideoForWindows.capCaptureGetSetup(handle, out var cp);
            cp.dwRequestMicroSecPerFrame = 1_000_000 / 5;   // 5fps
            if (!NativeMethods_VideoForWindows.capCaptureSetSetup(handle, cp))
            {
                throw new ArgumentException(
                    $"FlashCap: Couldn't set video frame rate [1]: DeviceIndex={deviceIndex}");
            }
            NativeMethods_VideoForWindows.capCaptureGetSetup(handle, out cp);

            var pih = NativeMethods.AllocateMemory((IntPtr)sizeof(NativeMethods.BITMAPINFOHEADER));
            try
            {
                var pBih = (NativeMethods.BITMAPINFOHEADER*)pih.ToPointer();

                pBih->biSize = sizeof(NativeMethods.BITMAPINFOHEADER);
                pBih->biCompression = compression;
                pBih->biPlanes = 1;
                pBih->biBitCount = bitCount;
                pBih->biWidth = characteristics.Width;
                pBih->biHeight = characteristics.Height;
                pBih->biSizeImage = pBih->CalculateImageSize();

                // Try to set video format.
                if (!NativeMethods_VideoForWindows.capSetVideoFormat(handle, pih))
                {
                    throw new ArgumentException(
                        $"FlashCap: Couldn't set video format [2]: DeviceIndex={deviceIndex}");
                }

                // Try to set fps, but VFW API may cause ignoring it silently...
                cp.dwRequestMicroSecPerFrame =
                    (int)(1_000_000 / characteristics.FramesPerSecond);
                if (!NativeMethods_VideoForWindows.capCaptureSetSetup(handle, cp))
                {
                    throw new ArgumentException(
                        $"FlashCap: Couldn't set video frame rate [2]: DeviceIndex={deviceIndex}");
                }
                NativeMethods_VideoForWindows.capCaptureGetSetup(handle, out cp);
            }
            finally
            {
                NativeMethods.FreeMemory(pih);
            }

            // Get final video format.
            NativeMethods_VideoForWindows.capGetVideoFormat(handle, out this.pBih);

            ///////////////////////////////////////

            // https://stackoverflow.com/questions/4097235/is-it-necessary-to-gchandle-alloc-each-callback-in-a-class
            this.thisPin = GCHandle.Alloc(this, GCHandleType.Normal);
            this.callback = this.CallbackEntry;

            NativeMethods_VideoForWindows.capSetCallbackFrame(this.handle, this.callback);
        }, ct);
    }

    ~VideoForWindowsDevice()
    {
        if (this.handle != IntPtr.Zero)
        {
            this.workingContext?.Send(_ =>
            {
                NativeMethods_VideoForWindows.capSetCallbackFrame(this.handle, null);
                NativeMethods_VideoForWindows.capDriverDisconnect(this.handle, this.deviceIndex);
                NativeMethods_VideoForWindows.DestroyWindow(this.handle);
            }, null);

            this.handle = IntPtr.Zero;
            this.thisPin.Free();
            this.callback = null;
            NativeMethods.FreeMemory(this.pBih);
            this.pBih = IntPtr.Zero;

            this.workingContext?.Dispose();
            this.workingContext = null;
        }
    }

    protected override async Task OnDisposeAsync()
    {
        if (this.handle != IntPtr.Zero)
        {
            try
            {
                await this.frameProcessor.DisposeAsync().
                    ConfigureAwait(false);

                await this.OnStopAsync(default).
                    ConfigureAwait(false);

                await this.workingContext!.InvokeAsync(() =>
                {
                    NativeMethods_VideoForWindows.capSetCallbackFrame(this.handle, null);
                    NativeMethods_VideoForWindows.capDriverDisconnect(this.handle, this.deviceIndex);
                    NativeMethods_VideoForWindows.DestroyWindow(this.handle);
                }, default);
            }
            finally
            {
                this.handle = IntPtr.Zero;
                this.thisPin.Free();
                this.callback = null;
                NativeMethods.FreeMemory(this.pBih);
                this.pBih = IntPtr.Zero;

                this.workingContext!.Dispose();
                this.workingContext = null;
            }
        }
    }

    private void CallbackEntry(
        IntPtr hWnd, in NativeMethods_VideoForWindows.VIDEOHDR hdr)
    {
        // HACK: Avoid stupid camera devices...
        if (hdr.dwBytesUsed >= 64)
        {
            try
            {
                this.frameProcessor.OnFrameArrived(
                    this,
                    hdr.lpData, hdr.dwBytesUsed,
                    // HACK: `hdr.dwTimeCaptured` always zero on my environment...
                    this.counter.ElapsedMicroseconds,
                    this.frameIndex++);
            }
            // DANGER: Stop leaking exception around outside of unmanaged area...
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }

    protected override Task OnStartAsync(CancellationToken ct)
    {
        if (!this.IsRunning)
        {
            return this.workingContext!.InvokeAsync(() =>
            {
                this.frameIndex = 0;
                this.counter.Restart();
                NativeMethods_VideoForWindows.capShowPreview(this.handle, true);
                this.IsRunning = true;
            }, default);
        }
        else
        {
            return TaskCompat.CompletedTask;
        }
    }

    protected override Task OnStopAsync(CancellationToken ct)
    {
        if (this.IsRunning)
        {
            return this.workingContext!.InvokeAsync(() =>
            {
                this.IsRunning = false;
                NativeMethods_VideoForWindows.capShowPreview(this.handle, false);
            }, default);
        }
        else
        {
            return TaskCompat.CompletedTask;
        }
    }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    protected override void OnCapture(
        IntPtr pData, int size, long timestampMicroseconds, long frameIndex,
        PixelBuffer buffer) =>
        buffer.CopyIn(this.pBih, pData, size, timestampMicroseconds, frameIndex, this.transcodeIfYUV);
}
