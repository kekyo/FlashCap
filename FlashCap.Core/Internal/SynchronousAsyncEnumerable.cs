////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

#if !(NET35 || NET40 || NET45 || NETSTANDARD1_3)
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Internal;

// This adapter preserves synchronous execution and deferred enumeration without scheduling a worker.
internal sealed class SynchronousAsyncEnumerable<T> : IAsyncEnumerable<T>
{
    private readonly Func<IEnumerable<T>> factory;
    private readonly CancellationToken cancellationToken;

    internal SynchronousAsyncEnumerable(Func<IEnumerable<T>> factory, CancellationToken cancellationToken)
    {
        this.factory = factory;
        this.cancellationToken = cancellationToken;
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new Enumerator(this.factory, this.cancellationToken, cancellationToken);

    private sealed class Enumerator : IAsyncEnumerator<T>
    {
        private readonly Func<IEnumerable<T>> factory;
        private readonly CancellationToken methodCancellation;
        private readonly CancellationToken enumerationCancellation;
        private IEnumerator<T>? iterator;
        private bool finished;

        internal Enumerator(Func<IEnumerable<T>> factory,
            CancellationToken methodCancellation, CancellationToken enumerationCancellation)
        {
            this.factory = factory;
            this.methodCancellation = methodCancellation;
            this.enumerationCancellation = enumerationCancellation;
        }

        public T Current => this.iterator!.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            this.methodCancellation.ThrowIfCancellationRequested();
            this.enumerationCancellation.ThrowIfCancellationRequested();
            if (this.finished)
                return new ValueTask<bool>(false);
            this.iterator ??= this.factory().GetEnumerator();
            if (this.iterator.MoveNext())
                return new ValueTask<bool>(true);
            this.finished = true;
            this.iterator.Dispose();
            this.iterator = null;
            return new ValueTask<bool>(false);
        }

        public ValueTask DisposeAsync()
        {
            this.finished = true;
            this.iterator?.Dispose();
            this.iterator = null;
            return default;
        }
    }
}
#endif
