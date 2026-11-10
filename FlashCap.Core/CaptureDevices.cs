////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
// Copyright (c) Yoh Deadfall (@YohDeadfall)
// Copyright (c) Felipe Ferreira Quintella (@ffquintella)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Devices;
using FlashCap.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap;

/// <summary>
/// Enumerates the available capture devices using platform-specific backends:
/// <list type= "bullet">
/// <item><description><see cref="DirectShowDevices"/> (windows)</description></item>
/// <item><description><see cref="VideoForWindowsDevices"/> (windows)</description></item>
/// <item><description><see cref="MediaFoundationDevices"/> (Windows 7 or greater) - Available on all supported target frameworks</description></item>
/// <item><description><see cref="V4L2Devices"/> (linux)</description></item>
/// <item><description><see cref="AVFoundationDevices"/> (macOs)</description></item>
/// </list>
/// </summary>
/// <remarks>
/// On targets that support asynchronous enumeration, Linux also enumerates PipeWire
/// cameras through the desktop camera portal. Synchronous enumeration includes V4L2 only.
/// </remarks>
public class CaptureDevices
{
    protected readonly BufferPool DefaultBufferPool;

    public CaptureDevices() :
        this(new DefaultBufferPool())
    {
    }

    public CaptureDevices(BufferPool defaultBufferPool)
    {
        DefaultBufferPool = defaultBufferPool;
    }

    [RequiresUnreferencedCode("OnEnumerateDescriptors adds DirectShow which requires unreferenced code. Use platform-specific Devices directly.")]
    protected virtual IEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptors()
    {
        if (NativeMethods.IsWindows())
        {
            var descriptors = new DirectShowDevices(this.DefaultBufferPool).OnEnumerateDescriptors();
            descriptors = descriptors.Concat(new VideoForWindowsDevices(this.DefaultBufferPool).OnEnumerateDescriptors());
#if FLASHCAP_MEDIAFOUNDATION
            if (NativeMethods.IsWindowsVersionAtLeast(6, 1))
                descriptors = descriptors.Concat(new MediaFoundationDevices(this.DefaultBufferPool).OnEnumerateDescriptors());
#endif
            return descriptors;
        }
        if (NativeMethods.IsLinux())
        {
#if !(NET35 || NET40 || NET45 || NETSTANDARD1_3)
            return new LinuxDescriptors(this.DefaultBufferPool);
#else
            return new V4L2Devices(this.DefaultBufferPool).OnEnumerateDescriptors();
#endif
        }
        if (NativeMethods.IsMacOS())
        {
            return new AVFoundationDevices().OnEnumerateDescriptors();
        }
        return ArrayEx.Empty<CaptureDeviceDescriptor>();
    }

    [RequiresUnreferencedCode("OnEnumerateDescriptors adds DirectShow which requires unreferenced code. Use platform-specific Devices directly.")]
    internal IEnumerable<CaptureDeviceDescriptor> InternalEnumerateDescriptors() =>
        this.OnEnumerateDescriptors();

#if !(NET35 || NET40 || NET45 || NETSTANDARD1_3)
    /// <summary>
    /// Enumerates devices asynchronously. Synchronous backends complete each move synchronously.
    /// </summary>
    /// <param name="ct">Cancels enumeration.</param>
    /// <returns>A finite sequence of available device descriptors.</returns>
    [RequiresUnreferencedCode("Default enumeration includes DirectShow. Use platform-specific Devices directly.")]
    protected virtual async IAsyncEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptorsAsync(
        [EnumeratorCancellation] CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var descriptors = this.OnEnumerateDescriptors();
        // The default Linux sequence has an additional async-only backend. A
        // synchronous backend override still runs synchronously and in isolation.
        var asynchronous = descriptors as IAsyncEnumerable<CaptureDeviceDescriptor> ??
            new SynchronousAsyncEnumerable<CaptureDeviceDescriptor>(() => descriptors, ct);
        await foreach (var descriptor in asynchronous.WithCancellation(ct).ConfigureAwait(false))
            yield return descriptor;
    }

    [RequiresUnreferencedCode("Default enumeration includes DirectShow. Use platform-specific Devices directly.")]
    internal IAsyncEnumerable<CaptureDeviceDescriptor> InternalEnumerateDescriptorsAsync(CancellationToken ct) =>
        this.OnEnumerateDescriptorsAsync(ct);

    [SupportedOSPlatform("linux")]
    private sealed class LinuxDescriptors : IEnumerable<CaptureDeviceDescriptor>, IAsyncEnumerable<CaptureDeviceDescriptor>
    {
        private readonly V4L2Devices v4l2;
        private readonly PipeWireDevices pipeWire;

        internal LinuxDescriptors(BufferPool bufferPool)
        {
            this.v4l2 = new V4L2Devices(bufferPool);
            this.pipeWire = new PipeWireDevices(bufferPool);
        }

        public IEnumerator<CaptureDeviceDescriptor> GetEnumerator() =>
            this.v4l2.OnEnumerateDescriptors().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public async IAsyncEnumerator<CaptureDeviceDescriptor> GetAsyncEnumerator(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await foreach (var descriptor in this.v4l2.OnEnumerateDescriptorsAsync(ct).ConfigureAwait(false))
                yield return descriptor;
            await foreach (var descriptor in this.pipeWire.OnEnumerateDescriptorsAsync(ct).ConfigureAwait(false))
                yield return descriptor;
        }
    }
#endif
}
