////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap
{
    public sealed class ObservableCaptureDevice : IObservable<PixelBufferScope>
    {
        private readonly CaptureDevice captureDevice;
        private readonly ObserverProxy proxy;

        internal ObservableCaptureDevice(CaptureDevice captureDevice, ObserverProxy proxy)
        {
            this.captureDevice = captureDevice;
            this.proxy = proxy;
        }

        public VideoCharacteristics Characteristics =>
            this.captureDevice.Characteristics;
        public bool IsRunning =>
            this.captureDevice.IsRunning;

        public void Start() =>
            this.captureDevice.Start();
        public void Stop() =>
            this.captureDevice.Stop();

        public IDisposable Subscribe(IObserver<PixelBufferScope> observer)
        {
            this.proxy.Subscribe(observer);
            return this.proxy;
        }

        internal sealed class ObserverProxy : IDisposable
        {
            private volatile IObserver<PixelBufferScope>? observer;

            public void Subscribe(IObserver<PixelBufferScope> observer)
            {
                if (Interlocked.CompareExchange(ref this.observer, observer, null) != null)
                {
                    throw new InvalidOperationException();
                }
            }

            public void Dispose() =>
                Interlocked.Exchange(ref this.observer, null);

            public void OnPixelBufferArrived(PixelBufferScope bufferScope)
            {
                if (this.observer is { } observer)
                {
                    observer.OnNext(bufferScope);
                }
            }
        }
    }
}
