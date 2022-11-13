////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

// Imported from:
// https://github.com/kekyo/SynchContextSample/blob/master/SynchContextSample/MessageQueueSynchronizationContext.cs

using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace FlashCap.Internal;

internal sealed class IndependentSingleApartmentContext :
    SynchronizationContext, IDisposable
{
    #region Interops for Win32
    private static readonly int WM_QUIT = 0x0012;

    private struct MSG
    {
        public IntPtr hWnd;
        public int msg;
        public IntPtr wParam;
        public IntPtr lParam;
        public IntPtr result;
    }

    [DllImport("user32", SetLastError = true)]
    private static extern bool PostThreadMessage(int threadId, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32", SetLastError = true)]
    private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax);

    [DllImport("user32", SetLastError = true)]
    private static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport("user32", SetLastError = true)]
    private static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

    [DllImport("user32", SetLastError = true)]
    private static extern int RegisterWindowMessage(string lpString);

    [DllImport("kernel32")]
    private static extern int GetCurrentThreadId();
    #endregion

    private sealed class Message : IDisposable
    {
        private ManualResetEventSlim? done;

        private ExceptionDispatchInfo? edi;

        public readonly SendOrPostCallback Callback;
        public readonly object? State;

        public Message(
            SendOrPostCallback callback, object? state, bool waitable)
        {
            this.Callback = callback;
            this.State = state;
            this.done = waitable ? new ManualResetEventSlim(false) : null;
        }

        public void Dispose()
        {
            if (this.done != null)
            {
                this.done?.Dispose();
                this.done = null;
                this.edi = null;
            }
        }

        public void SetDone() =>
            this.done?.Set();

        public void SetException(Exception ex)
        {
            if (this.done is { } done)
            {
                this.edi = ExceptionDispatchInfo.Capture(ex);
                done.Set();
            }
        }

        public void Wait()
        {
            this.done!.Wait();
            this.edi?.Throw();
        }
    }

    private static readonly int WM_SC =
        RegisterWindowMessage("IndependentSingleApartmentContext_" + Guid.NewGuid().ToString("N"));

    private Thread? thread;
    private ManualResetEventSlim? ready = new(false);
    private int targetThreadId;
    private int recursiveCount;

    public IndependentSingleApartmentContext()
    {
        Debug.Assert(NativeMethods.CurrentPlatform == NativeMethods.Platforms.Windows);

        this.thread = new Thread(this.ThreadEntry);
        this.thread.IsBackground = true;
        this.thread.SetApartmentState(ApartmentState.STA);   // Improved compatibility
        this.thread.Start();

        this.ready!.Wait();
        this.ready.Dispose();
        this.ready = null;
    }

    public void Dispose()
    {
        if (this.thread != null)
        {
            PostThreadMessage(this.targetThreadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
            this.thread.Join();
            this.thread = null;
        }
    }

    public override SynchronizationContext CreateCopy() =>
        this;

    private bool SendCore(SendOrPostCallback continuation, object? state)
    {
        // If current thread id is target thread id:
        var currentThreadId = GetCurrentThreadId();
        if (currentThreadId == this.targetThreadId)
        {
            if (Interlocked.Increment(ref this.recursiveCount) < 50)
            {
                try
                {
                    continuation(state);
                }
                finally
                {
                    Interlocked.Decrement(ref this.recursiveCount);
                }
                return true;
            }
            else
            {
                Interlocked.Decrement(ref this.recursiveCount);
            }
        }

        return false;
    }

    public override void Send(SendOrPostCallback continuation, object? state)
    {
        if (!this.SendCore(continuation, state))
        {
            using var message = new Message(continuation, state, true);
            var handle = GCHandle.ToIntPtr(GCHandle.Alloc(message));

            PostThreadMessage(this.targetThreadId, WM_SC, IntPtr.Zero, handle);

            message.Wait();
        }
    }

    public override void Post(SendOrPostCallback continuation, object? state)
    {
        if (!this.SendCore(continuation, state))
        {
            var message = new Message(continuation, state, false);
            var handle = GCHandle.ToIntPtr(GCHandle.Alloc(message));

            PostThreadMessage(this.targetThreadId, WM_SC, IntPtr.Zero, handle);
        }
    }

    private void ThreadEntry()
    {
        this.targetThreadId = GetCurrentThreadId();
        SetSynchronizationContext(this);

        this.ready!.Set();

        while (true)
        {
            MSG msg;
            var result = GetMessage(out msg, IntPtr.Zero, 0, 0);
            if (result == 0)
            {
                break;
            }

            if (result == -1)
            {
                var hr = Marshal.GetHRForLastWin32Error();
                Debug.WriteLine($"FlashCap: Unknown error for win32 message: {hr}");
                break;
            }

            if (msg.msg == WM_SC)
            {
                var handle = GCHandle.FromIntPtr(msg.lParam);

                var message = (Message)handle.Target!;
                handle.Free();

                try
                {
                    message.Callback(message.State);
                    message.SetDone();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    message.SetException(ex);
                }

                continue;
            }

            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }
    }
}
