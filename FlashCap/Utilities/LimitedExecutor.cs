////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Utilities
{
    public delegate void LimitedAction();
#if !(NET20 || NET35 || NET40)
    public delegate Task LimitedAsyncAction();
#endif

    public sealed class LimitedExecutor
    {
        private readonly int maxConcurrentCount;
        private int inCount;

        public LimitedExecutor(int maxConcurrentCount = 1) =>
            this.maxConcurrentCount = maxConcurrentCount;

        public void Execute(LimitedAction justNow)
        {
            if (Interlocked.Increment(ref this.inCount) <= this.maxConcurrentCount)
            {
                try
                {
                    justNow();
                }
                finally
                {
                    Interlocked.Decrement(ref this.inCount);
                }
            }
            else
            {
                Interlocked.Decrement(ref this.inCount);
            }
        }

        public void ExecuteAndOffload(
            LimitedAction justNow, LimitedAction offloadedContinuation)
        {
            if (Interlocked.Increment(ref this.inCount) <= this.maxConcurrentCount)
            {
                try
                {
                    justNow();
                    ThreadPool.QueueUserWorkItem(oc =>
                    {
                        var offloadedContinuation = (LimitedAction)oc!;
                        try
                        {
                            offloadedContinuation();
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref this.inCount);
                        }
                    }, offloadedContinuation);
                }
                catch
                {
                    Interlocked.Decrement(ref this.inCount);
                    throw;
                }
            }
            else
            {
                Interlocked.Decrement(ref this.inCount);
            }
        }

#if !(NET20 ||NET35 || NET40)
        public void ExecuteAndOffload(
            LimitedAction justNow, LimitedAsyncAction offloadedContinuation)
        {
            if (Interlocked.Increment(ref this.inCount) <= this.maxConcurrentCount)
            {
                try
                {
                    justNow();
                    ThreadPool.QueueUserWorkItem(async oc =>
                    {
                        var offloadedContinuation = (LimitedAsyncAction)oc!;
                        try
                        {
                            await offloadedContinuation().ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref this.inCount);
                        }
                    }, offloadedContinuation);
                }
                catch
                {
                    Interlocked.Decrement(ref this.inCount);
                    throw;
                }
            }
            else
            {
                Interlocked.Decrement(ref this.inCount);
            }
        }
#endif
    }
}
