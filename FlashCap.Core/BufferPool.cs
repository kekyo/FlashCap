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

    private const int bufferHolderLength = 13;

    [DebuggerStepThrough]
    private sealed class BufferHolder
    {
        private readonly byte[]?[] buffers;

        public BufferHolder(int maxReservedBufferElements) =>
            this.buffers = new byte[maxReservedBufferElements][];

        public byte[]? Rent(int size, bool exactSize)
        {
            for (var index = 0; index < this.buffers.Length; index++)
            {
                var buffer = this.buffers[index];
                if (buffer != null && (exactSize ? (buffer.Length == size) : (buffer.Length >= size)))
                {
                    if (Interlocked.CompareExchange(ref this.buffers[index], null, buffer) == buffer)
                    {
                        return buffer;
                    }
                }
            }

            return null;
        }

        public bool Return(byte[] buffer)
        {
            for (var index = 0; index < this.buffers.Length; index++)
            {
                if (this.buffers[index] == null)
                {
                    if (Interlocked.CompareExchange(ref this.buffers[index], buffer, null) == null)
                    {
                        return true;
                    }
                }
            }

            // It was better to simply discard a buffer instance than the cost of extending the table.
            return false;
        }
    }

    private readonly BufferHolder[] bufferHolders;
    private int saved;

    public DefaultBufferPool() : this(16)
    {
    }

    public DefaultBufferPool(int maxReservedBufferElements) =>
        this.bufferHolders = Enumerable.Range(0, bufferHolderLength).
        Select(_ => new BufferHolder(maxReservedBufferElements)).
        ToArray();

    public override byte[] Rent(int size, bool exactSize)
    {
        // NOTE: Size is determined on a "less than" basis when not exact size,
        // which may result in placement in different buckets and missed opportunities for reuse.
        // This implementation is ignored penalties.
        int saved;
        var bufferHolder = this.bufferHolders[size % this.bufferHolders.Length];
        if (bufferHolder.Rent(size, exactSize) is { } b)
        {
            saved = Interlocked.Decrement(ref this.saved);
        }
        else
        {
            b = new byte[size];
            saved = this.saved;
        }

#if DEBUG
        Debug.WriteLine($"DefaultBufferPool: Rend: Size={b.Length}/{size}, ExactSize={exactSize}, Saved={saved}");
#endif
        return b;
    }

    public override void Return(byte[] buffer)
    {
        int saved;
        var bufferHolder = this.bufferHolders[buffer.Length % this.bufferHolders.Length];
        if (bufferHolder.Return(buffer))
        {
            saved = Interlocked.Increment(ref this.saved);
        }
        else
        {
            saved = this.saved;
        }

#if DEBUG
        Debug.WriteLine($"DefaultBufferPool: Returned: Size={buffer.Length}, Saved={saved}");
#endif
    }
}
