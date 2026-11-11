////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////


#if !(NET35 || NET40 || NET45 || NETSTANDARD1_3)
using FlashCap.Internal.PipeWire;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Devices;

/// <summary>Enumerates accessible Linux cameras through PipeWire.</summary>
/// <remarks>
/// Enumeration may display a permission dialog and requires asynchronous enumeration.
/// Synchronous enumeration always throws <see cref="NotSupportedException"/>.
/// Requires libpipewire-0.3. Portal connections also require GLib/GIO and a desktop camera portal.
/// Captures CPU-readable RGB24, RGB32, ARGB32, YUYV, UYVY, NV12 and JPEG formats;
/// DMA-BUF formats are not advertised.
/// </remarks>
/// <seealso href="https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.Camera.html"/>
[SupportedOSPlatform("linux")]
public sealed class PipeWireDevices : CaptureDevices
{
    /// <summary>Creates a camera enumerator using the default buffer pool.</summary>
    public PipeWireDevices() : this(new DefaultBufferPool()) { }

    /// <summary>Creates a camera enumerator using the supplied buffer pool.</summary>
    /// <param name="defaultBufferPool">The pool used by opened capture devices.</param>
    public PipeWireDevices(BufferPool defaultBufferPool) : base(defaultBufferPool) { }

    /// <summary>Rejects synchronous enumeration, including calls through <see cref="CaptureDevices"/>.</summary>
    /// <returns>This method never returns.</returns>
    /// <exception cref="NotSupportedException">Synchronous enumeration is not supported.</exception>
    [UnconditionalSuppressMessage("Trimming", "IL2046", Justification = "This backend does not enumerate DirectShow devices.")]
    protected override IEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptors() =>
        throw new NotSupportedException("PipeWire camera enumeration requires EnumerateDescriptorsAsync.");

    /// <summary>Requests camera access and enumerates the available cameras.</summary>
    /// <param name="ct">Cancels the permission request or enumeration.</param>
    /// <returns>A finite sequence of accessible cameras, or an empty sequence if the backend is unavailable.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2046", Justification = "This backend does not enumerate DirectShow devices.")]
    protected override async IAsyncEnumerable<CaptureDeviceDescriptor> OnEnumerateDescriptorsAsync(
        [EnumeratorCancellation] CancellationToken ct)
    {
        var cameras = await GetCamerasAsync(false, ct).ConfigureAwait(false);
        if (cameras.Length == 0)
            cameras = await GetCamerasAsync(true, ct).ConfigureAwait(false);
        foreach (var camera in cameras)
        {
            ct.ThrowIfCancellationRequested();
            yield return new PipeWireDeviceDescriptor(camera, this.DefaultBufferPool);
        }
    }

    private static async Task<PipeWireCamera[]> GetCamerasAsync(bool usePortal, CancellationToken ct)
    {
        PipeWireCamera[] cameras;
        try
        {
            using var connection = await PipeWireConnection.OpenAsync(usePortal, ct).ConfigureAwait(false);
            cameras = connection.GetCameras();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or
            DllNotFoundException or EntryPointNotFoundException or BadImageFormatException or OperationCanceledException)
        {
            // A portal dismissal is not cancellation of the caller's enumeration.
            ct.ThrowIfCancellationRequested();
            Trace.WriteLine($"FlashCap: PipeWire {(usePortal ? "portal" : "direct")} enumeration unavailable: {exception}");
            cameras = ArrayEx.Empty<PipeWireCamera>();
        }
        ct.ThrowIfCancellationRequested();
        return cameras;
    }
}
#endif
