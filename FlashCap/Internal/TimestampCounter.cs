////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;

namespace FlashCap.Internal
{
    internal sealed class TimestampCounter
    {
        private static readonly double frequencyMicrosecond =
            Stopwatch.Frequency / 1_000_000.0;
        private readonly Stopwatch stopwatch = new();

        public double ElapsedMicroseconds
        {
            get
            {
                // https://stackoverflow.com/questions/6664538/is-stopwatch-elapsedticks-threadsafe
                long tick;
                lock (this.stopwatch)
                {
                    tick = this.stopwatch.ElapsedTicks;
                }
                return tick / frequencyMicrosecond;
            }
        }

        public void Restart()
        {
            lock (this.stopwatch)
            {
                this.stopwatch.Reset();
                this.stopwatch.Start();
            }
        }
    }
}
