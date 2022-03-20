////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Threading;
using System.Threading.Tasks;

namespace FlashCap
{
    public static class CaptureDeviceExtension
    {
        public static PixelBuffer CaptureOneShot(
            this ICaptureDevice device)
        {
            using var waiter = new ManualResetEventSlim(false);
            var buffer = new PixelBuffer();
            
            void FrameArrived(object sender, FrameArrivedEventArgs e)
            {
                device.Capture(e, buffer);
                device.FrameArrived -= FrameArrived!;
                waiter.Set();
            }

            var isRunning = device.IsRunning;
            device.FrameArrived += FrameArrived!;
            try
            {
                if (!isRunning)
                {
                    device.Start();
                }
                waiter.Wait();
            }
            finally
            {
                if (!isRunning)
                {
                    device.Stop();
                }
                device.FrameArrived -= FrameArrived!;
            }

            return buffer;
        }

#if !(NET20 || NET35 || NET40)
        public static async Task<PixelBuffer> CaptureOneShotAsync(
            this ICaptureDevice device)
        {
            var tcs = new TaskCompletionSource<PixelBuffer>();
            var buffer = new PixelBuffer();
            
            void FrameArrived(object sender, FrameArrivedEventArgs e)
            {
                device.Capture(e, buffer);
                device.FrameArrived -= FrameArrived!;
                tcs.SetResult(buffer);
            }

            var isRunning = device.IsRunning;
            device.FrameArrived += FrameArrived!;
            try
            {
                if (!isRunning)
                {
                    device.Start();
                }

                return await tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                if (!isRunning)
                {
                    device.Stop();
                }
                device.FrameArrived -= FrameArrived!;
            }
        }
#endif
    }
}
