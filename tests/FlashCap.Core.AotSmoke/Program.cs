////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices.Marshalling;
using FlashCap;
using FlashCap.Devices;
using FlashCap.Internal;

namespace FlashCap.Core.AotSmoke;

internal static partial class Program
{
    private static readonly Guid CallbackInterfaceId =
        new("DEEC8D99-FA1D-4D82-84C2-2C8969944867");

    private static int Main()
    {
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

            return 0;
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
