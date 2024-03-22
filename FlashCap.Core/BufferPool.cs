////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace FlashCap;

public abstract class BufferPool
{
    protected BufferPool()
    {
    }

    public abstract byte[] Rent(int size, bool exactSize);
    public abstract void Return(byte[] buffer);
}

public sealed class DefaultBufferPool :
    BufferPool
{
    // Imported from:
    // https://github.com/kekyo/GitReader/blob/main/GitReader.Core/Internal/BufferPool.cs

    // Tried and tested, but simple strategies were the fastest.
    // Probably because each buffer table and lookup fragments on the CPU cache.

    private const int BufferHolderLength = 13;

    [DebuggerStepThrough]
    private sealed class BufferHolder
    {
        private readonly byte[]?[] buffers;

        public BufferHolder(int maxReservedBufferElements) =>
            this.buffers = new byte[maxReservedBufferElements][];

        public byte[] Rent(int size, bool exactSize)
        {
            for (var index = 0; index < this.buffers.Length; index++)
            {
                var buffer = this.buffers[index];
                if (buffer != null && (exactSize ? (buffer.Length == size) : (buffer.Length <= size)))
                {
                    if (Interlocked.CompareExchange(ref this.buffers[index], null, buffer) == buffer)
                    {
                        return buffer!;
                    }
                }
            }

            return new byte[size];
        }

        public void Return(byte[] buffer)
        {
            for (var index = 0; index < this.buffers.Length; index++)
            {
                if (this.buffers[index] == null)
                {
                    if (Interlocked.CompareExchange(ref this.buffers[index], buffer, null) == null)
                    {
                        break;
                    }
                }
            }

            // It was better to simply discard a buffer instance than the cost of extending the table.
        }
    }

    private readonly BufferHolder[] bufferHolders;

    public DefaultBufferPool() : this(32)
    {
    }

    public DefaultBufferPool(int maxReservedBufferElements) =>
        this.bufferHolders = Enumerable.Range(0, BufferHolderLength).
        Select(_ => new BufferHolder(maxReservedBufferElements)).
        ToArray();

    public override byte[] Rent(int size, bool exactSize)
    {
        // NOTE: Size is determined on a "less than" basis,
        // which may result in placement in different buckets and missed opportunities for reuse.
        // This implementation is ignored it.
        var bufferHolder = this.bufferHolders[size % this.bufferHolders.Length];
        return bufferHolder.Rent(size, exactSize);
    }

    public override void Return(byte[] buffer)
    {
        if (Interlocked.Exchange(ref buffer, null!) is { } b)
        {
            var bufferHolder = this.bufferHolders[b.Length % this.bufferHolders.Length];
            bufferHolder.Return(b);
        }
    }
}
