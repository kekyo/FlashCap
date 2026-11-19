////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;
using FlashCap;
using FlashCap.Devices;
using FlashCap.Internal;
using FlashCap.Internal.MediaFoundation;

namespace FlashCap.Core.AotSmoke;

internal static partial class Program
{
    private static readonly Guid CallbackInterfaceId =
        new("DEEC8D99-FA1D-4D82-84C2-2C8969944867");

    private static int Main(string[] args)
    {
        if (args.Contains("--pipewire") && OperatingSystem.IsLinux())
            return CapturePipeWireAsync().GetAwaiter().GetResult();
        if (OperatingSystem.IsMacOS())
        {
            var devices = new AVFoundationDevices();
            _ = devices.EnumerateDescriptors().ToArray();
            _ = devices.GetDescriptors();
            return 0;
        }

        if (OperatingSystem.IsLinux())
        {
            var devices = new V4L2Devices();
            _ = devices.EnumerateDescriptors().ToArray();
            _ = devices.GetDescriptors();
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            try
            {
                new PipeWireDevices().GetDescriptorsAsync(cancellation.Token).GetAwaiter().GetResult();
                return 7;
            }
            catch (OperationCanceledException) { }
        }

        try
        {
#pragma warning disable CA1416
            _ = new MediaFoundationDevices().GetDescriptors();
#pragma warning restore CA1416
            Console.Error.WriteLine("Media Foundation enumeration unexpectedly ran on Linux.");
            return 1;
        }
        catch (PlatformNotSupportedException)
        {
        }

        var callback = new SourceReaderCallback();
        IntPtr callbackPointer = IntPtr.Zero;
        IntPtr callbackInterface = IntPtr.Zero;
        try
        {
            // The smoke test intentionally exercises the platform-independent generated COM path on Linux.
#pragma warning disable CA1416
            callbackPointer = NativeMethods_MediaFoundation.GetComInterfaceForObject(
                callback);
#pragma warning restore CA1416
            if (callbackPointer == IntPtr.Zero)
            {
                Console.Error.WriteLine("The generated COM interface pointer was null.");
                return 2;
            }

            unsafe
            {
                var unknownVtable = *(void***)callbackPointer;
                var queryInterface =
                    (delegate* unmanaged[Stdcall]<IntPtr, Guid*, IntPtr*, int>)unknownVtable[0];
                var callbackInterfaceId = CallbackInterfaceId;
                if (queryInterface(
                        callbackPointer,
                        &callbackInterfaceId,
                        &callbackInterface) < 0 ||
                    callbackInterface == IntPtr.Zero)
                {
                    return 3;
                }

                var vtable = *(void***)callbackInterface;
                var onReadSample =
                    (delegate* unmanaged[Stdcall]<IntPtr, int, uint, uint, long, IntPtr, int>)vtable[3];
                var onFlush = (delegate* unmanaged[Stdcall]<IntPtr, uint, int>)vtable[4];
                var onEvent = (delegate* unmanaged[Stdcall]<IntPtr, uint, IntPtr, int>)vtable[5];
                if (onReadSample(callbackInterface, -7, 11, 13, 17, new IntPtr(19)) != 0)
                {
                    return 3;
                }
                if (onFlush(callbackInterface, 23) != 0)
                {
                    return 3;
                }
                if (onEvent(callbackInterface, 29, new IntPtr(31)) != 0)
                {
                    return 3;
                }
            }
            if (callback.Status != -7 ||
                callback.ReadStreamIndex != 11 ||
                callback.StreamFlags != 13 ||
                callback.Timestamp != 17 ||
                callback.Sample != new IntPtr(19) ||
                callback.FlushStreamIndex != 23 ||
                callback.EventStreamIndex != 29 ||
                callback.MediaEvent != new IntPtr(31))
            {
                return 4;
            }

            return VerifyCameraControls() ? 0 : 6;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 5;
        }
        finally
        {
            if (callbackInterface != IntPtr.Zero)
            {
#pragma warning disable CA1416
                NativeMethods_MediaFoundation.ReleaseComInterface(callbackInterface);
#pragma warning restore CA1416
            }
            if (callbackPointer != IntPtr.Zero)
            {
                // The smoke test intentionally exercises the platform-independent raw IUnknown release path on Linux.
#pragma warning disable CA1416
                NativeMethods_MediaFoundation.ReleaseComInterface(callbackPointer);
#pragma warning restore CA1416
            }
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("linux")]
    private static async Task<int> CapturePipeWireAsync()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var devices = await new PipeWireDevices().GetDescriptorsAsync(timeout.Token);
        if (devices.Length == 0) return 8;
        foreach (var camera in devices)
        {
            var received = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
            var device = await camera.OpenAsync(camera.Characteristics.Single(), TranscodeFormats.DoNotTranscode,
                scope => received.TrySetResult(scope.Buffer.CopyImage()), timeout.Token);
            byte[] frame;
            try
            {
                await device.StartAsync(timeout.Token);
                var controls = await device.GetControlsAsync(timeout.Token);
                foreach (var control in controls)
                {
                    var value = await device.GetControlValueAsync(control.Type, timeout.Token);
                    if (value is null) return 8;
                    // Read-only fixtures reject this call; it must still exercise the
                    // native set_param delegate and keep the capture connection alive.
                    await device.SetControlValueAsync(control.Type, value.Value, value.IsAuto, timeout.Token);
                }
                frame = await received.Task.WaitAsync(timeout.Token);
                if (!device.IsRunning) return 8;
            }
            finally { await device.DisposeAsync(); }
            var first = camera.Name.EndsWith(" extra", StringComparison.Ordinal) ? 101 : 1;
            if (frame.Length != 66 || !frame.Skip(54).SequenceEqual(Enumerable.Range(first, 12).Select(value => (byte)value)))
                return 8;
        }
        Console.WriteLine("PipeWire AOT capture passed.");
        return 0;
    }

    // Exercise the native control ABI in the published AOT executable without camera hardware.
    // This catches accidental introduction of runtime-generated COM wrappers into this path.
#pragma warning disable CA1416
    private static unsafe bool VerifyCameraControls()
    {
        void** vtable = stackalloc void*[6];
        vtable[0] = (void*)(delegate* unmanaged[Stdcall]<Control*, Guid*, void**, int>)&QueryControl;
        vtable[2] = (void*)(delegate* unmanaged[Stdcall]<Control*, uint>)&ReleaseControl;
        vtable[3] = (void*)(delegate* unmanaged[Stdcall]<Control*, int, int*, int*, int*, int*, int*, int>)&GetControlRange;
        vtable[4] = (void*)(delegate* unmanaged[Stdcall]<Control*, int, int, int, int>)&SetControl;
        vtable[5] = (void*)(delegate* unmanaged[Stdcall]<Control*, int, int*, int*, int>)&GetControl;
        var native = new Control { Vtable = vtable, Value = 32, Flags = 2 };
        using (var controls = new CameraControls((NativeMethods_MediaFoundation.IUnknown*)&native))
        {
            var ranges = controls.GetControls();
            if (ranges.Length != 1 || ranges[0].Type != CaptureDeviceControlTypes.Brightness ||
                !ranges[0].SupportsAuto || controls.GetValue(CaptureDeviceControlTypes.Brightness)?.Value != 32 ||
                !controls.SetValue(CaptureDeviceControlTypes.Brightness, 64, false) ||
                !controls.SetValue(CaptureDeviceControlTypes.Brightness, int.MaxValue, true) ||
                controls.GetValue(CaptureDeviceControlTypes.Brightness)?.IsAuto != true || native.Value != 64)
            {
                return false;
            }
        }
        return native.References == 0;
    }
#pragma warning restore CA1416

    private unsafe struct Control
    {
        internal void** Vtable;
        internal int References;
        internal int Value;
        internal int Flags;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static unsafe int QueryControl(Control* self, Guid* iid, void** value)
    {
        *value = null;
        if (*iid != new Guid("C6E13360-30AC-11D0-A18C-00A0C9118956"))
        {
            return unchecked((int)0x80004002);
        }
        self->References++;
        *value = self;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static unsafe uint ReleaseControl(Control* self) => (uint)--self->References;

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static unsafe int GetControlRange(Control* self, int property, int* min, int* max, int* step, int* defaultValue, int* caps)
    {
        *min = 0;
        *max = 255;
        *step = 1;
        *defaultValue = 128;
        *caps = 3;
        return property == 0 ? 0 : unchecked((int)0x80070490);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static unsafe int SetControl(Control* self, int property, int value, int flags)
    {
        self->Value = value;
        self->Flags = flags;
        return property == 0 ? 0 : unchecked((int)0x80070490);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static unsafe int GetControl(Control* self, int property, int* value, int* flags)
    {
        *value = self->Value;
        *flags = self->Flags;
        return property == 0 ? 0 : unchecked((int)0x80070490);
    }

    [GeneratedComClass]
    private sealed partial class SourceReaderCallback :
        IMFSourceReaderCallbackInterop
    {
        internal int Status;
        internal uint ReadStreamIndex;
        internal uint StreamFlags;
        internal long Timestamp;
        internal IntPtr Sample;
        internal uint FlushStreamIndex;
        internal uint EventStreamIndex;
        internal IntPtr MediaEvent;

        public int OnReadSample(
            int status,
            uint streamIndex,
            uint streamFlags,
            long timestamp,
            IntPtr sample)
        {
            this.Status = status;
            this.ReadStreamIndex = streamIndex;
            this.StreamFlags = streamFlags;
            this.Timestamp = timestamp;
            this.Sample = sample;
            return 0;
        }

        public int OnFlush(uint streamIndex)
        {
            this.FlushStreamIndex = streamIndex;
            return 0;
        }

        public int OnEvent(uint streamIndex, IntPtr mediaEvent)
        {
            this.EventStreamIndex = streamIndex;
            this.MediaEvent = mediaEvent;
            return 0;
        }
    }
}
