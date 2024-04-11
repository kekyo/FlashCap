////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace FlashCap;

public abstract class BufferPool
{
    protected BufferPool()
    {
    }

    public abstract byte[] Rent(int minimumSize);
    public abstract void Return(byte[] buffer);
}

public sealed class DefaultBufferPool :
    BufferPool
{
    private sealed class BufferElement
    {
        private readonly int size;
        private readonly WeakReference wr;

        public BufferElement(byte[] buffer)
        {
            this.size = buffer.Length;
            this.wr = new WeakReference(buffer);
        }

        public bool IsAvailable =>
            this.wr.IsAlive;

        public bool IsAvailableAndFit(int minimumSize) =>
            this.wr.IsAlive && (minimumSize <= this.size);

        public byte[]? ExtractBuffer() =>
            (byte[]?)this.wr.Target;
    }

    private readonly BufferElement?[] bufferElements;

    public DefaultBufferPool() : this(16)
    {
    }

    public DefaultBufferPool(int maxReservedBufferElements) =>
        this.bufferElements = new BufferElement?[maxReservedBufferElements];

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public int UnsafeAvailableCount =>
        this.bufferElements.Count(bufferHolder => bufferHolder?.IsAvailable ?? false);

    public override byte[] Rent(int minimumSize)
    {
        for (var index = 0; index < this.bufferElements.Length; index++)
        {
            var bufferElement = this.bufferElements[index];

            // First phase:
            // * Determined: size and exactSize
            // * NOT determined: Availability
            if (bufferElement?.IsAvailableAndFit(minimumSize) ?? false)
            {
                if (object.ReferenceEquals(
                    Interlocked.CompareExchange(
                        ref this.bufferElements[index],
                        null,
                        bufferElement),
                        bufferElement) &&
                    // Second phase
                    // * Determined: size, exactSize and availability
                    bufferElement.ExtractBuffer() is { } buffer)
                {
#if DEBUG
                    Debug.WriteLine($"DefaultBufferPool: Rend: Size={buffer.Length}/{minimumSize}, Index={index}");
#endif
                    return buffer;
                }
            }
            else if (!(bufferElement?.IsAvailable ?? true))
            {
                // Remove corrected element (and forgot).
                Interlocked.CompareExchange(
                    ref this.bufferElements[index],
                    null,
                    bufferElement);
            }
        }

#if DEBUG
        Debug.WriteLine($"DefaultBufferPool: Created: Size={minimumSize}");
#endif
        return new byte[minimumSize];
    }

    public override void Return(byte[] buffer)
    {
        var newBufferElement = new BufferElement(buffer);

        for (var index = 0; index < this.bufferElements.Length; index++)
        {
            var bufferElement = this.bufferElements[index];
            if (bufferElement == null || !bufferElement.IsAvailable)
            {
                if (object.ReferenceEquals(
                    Interlocked.CompareExchange(
                        ref this.bufferElements[index],
                        newBufferElement,
                        bufferElement),
                    bufferElement))
                {
#if DEBUG
                    Debug.WriteLine($"DefaultBufferPool: Returned: Size={buffer.Length}, Index={index}");
#endif
                    return;
                }
            }
        }

        // It was better to simply discard a buffer instance than the cost of extending the table.
#if DEBUG
        Debug.WriteLine($"DefaultBufferPool: Discarded: Size={buffer.Length}");
#endif
    }
}
