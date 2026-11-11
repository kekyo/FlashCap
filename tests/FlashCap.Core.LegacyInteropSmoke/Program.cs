////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FlashCap.Internal;

namespace FlashCap.Core.LegacyInteropSmoke;

internal static unsafe class Program
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int OnReadSampleDelegate(
        IntPtr self,
        int status,
        uint streamIndex,
        uint streamFlags,
        long timestamp,
        IntPtr sample);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int OnFlushDelegate(IntPtr self, uint streamIndex);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int OnEventDelegate(IntPtr self, uint streamIndex, IntPtr mediaEvent);

    private static int Main()
    {
        var callback = new SourceReaderCallback();
        var callbackPointer = IntPtr.Zero;
        try
        {
            if (!VerifyAsyncLock()) return 6;
            if (!typeof(IMFSourceReaderCallbackInterop).IsVisible)
            {
                Console.Error.WriteLine("The callback interface is not visible to COM.");
                return 5;
            }

            callbackPointer = NativeMethods_MediaFoundation.GetComInterfaceForObject(callback);
            if (callbackPointer == IntPtr.Zero)
            {
                return 1;
            }

            var vtable = *(IntPtr**)callbackPointer;
            var onReadSample = (OnReadSampleDelegate)Marshal.GetDelegateForFunctionPointer(
                vtable[3], typeof(OnReadSampleDelegate));
            var onFlush = (OnFlushDelegate)Marshal.GetDelegateForFunctionPointer(
                vtable[4], typeof(OnFlushDelegate));
            var onEvent = (OnEventDelegate)Marshal.GetDelegateForFunctionPointer(
                vtable[5], typeof(OnEventDelegate));

            if (onReadSample(callbackPointer, -7, 11, 13, 17, new IntPtr(19)) != 0 ||
                onFlush(callbackPointer, 23) != 0 ||
                onEvent(callbackPointer, 29, new IntPtr(31)) != 0)
            {
                return 2;
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
                return 3;
            }

            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 4;
        }
        finally
        {
            NativeMethods_MediaFoundation.ReleaseComInterface(callbackPointer);
        }
    }

    private static bool VerifyAsyncLock()
    {
        var mutex = new AsyncLock();
        using var cancellation = new CancellationTokenSource();
        var owner = mutex.LockAsync(CancellationToken.None).GetAwaiter().GetResult();
        var canceled = mutex.LockAsync(cancellation.Token);
        var next = mutex.LockAsync(CancellationToken.None);
        if (next.IsCompleted) return false;
        cancellation.Cancel();
        owner.Dispose();
        // The net35 TPL backport's blocking Task.Wait uses Windows-only APIs.
        // Observe completion through a kernel event so these checks also run on Mono.
        using (var completed = new ManualResetEvent(false))
        {
            canceled.ContinueWith(_ => completed.Set());
            if (!completed.WaitOne(TimeSpan.FromSeconds(30))) return false;
        }
        if (!canceled.IsCanceled) return false;
        using (var completed = new ManualResetEvent(false))
        {
            next.ContinueWith(_ => completed.Set());
            if (!completed.WaitOne(TimeSpan.FromSeconds(30))) return false;
        }
        using (next.GetAwaiter().GetResult()) { }
        using var final = mutex.LockAsync(CancellationToken.None).GetAwaiter().GetResult();
        return true;
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class SourceReaderCallback :
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
