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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Internal.PipeWire;

internal sealed class PipeWireCamera
{
    internal readonly string Serial, NodeName, Name, Description;
    internal readonly PipeWireFormat[] Formats;
    internal readonly bool UsePortal;

    internal PipeWireCamera(string serial, string nodeName, string name, string description, PipeWireFormat[] formats, bool usePortal)
    {
        this.Serial = serial;
        this.NodeName = nodeName;
        this.Name = name;
        this.Description = description;
        this.Formats = formats;
        this.UsePortal = usePortal;
    }
}

// Owns one PipeWire connection and all its native objects. All graph access is serialized
// by pw_thread_loop's recursive lock; native callbacks already hold that lock.
internal sealed class PipeWireConnection : IDisposable
{
    private static readonly Lazy<bool> initialized = new(() =>
    {
        PipeWireNative.pw_init(IntPtr.Zero, IntPtr.Zero);
        return true;
    });

    private sealed class Node
    {
        internal readonly Listeners Listeners = new();
        internal readonly PipeWireCamera Camera;
        internal readonly List<PipeWireFormat> Formats = new();
        internal readonly IntPtr Proxy;
        internal Node(PipeWireCamera camera, IntPtr proxy) { this.Camera = camera; this.Proxy = proxy; }
    }

    private readonly Listeners listeners = new();
    private readonly bool usePortal;
    private Listeners? streamListeners;
    private readonly Dictionary<uint, Node> nodes = new();
    private readonly Dictionary<int, TaskCompletionSource<bool>> pending = new();
    private IntPtr loop, context, core, registry, stream;
    private bool loopStarted, disposed;
    private Exception? failure;
    private TaskCompletionSource<bool>? streamReady;
    private string? targetSerial;
    private Action<PipeWirePlane[]>? capture;
    private bool formatReady, streaming;
    private PipeWirePlane[] planes = new PipeWirePlane[1];
    internal event Action? Disconnected;

    private PipeWireConnection(bool usePortal) => this.usePortal = usePortal;

    internal static async Task<PipeWireConnection> OpenAsync(bool usePortal, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!NativeMethods.IsLinux()) throw new PlatformNotSupportedException("PipeWire camera capture requires Linux.");
        var descriptor = usePortal ? await CameraPortal.OpenRemoteAsync(ct).ConfigureAwait(false) : -1;
        var connection = new PipeWireConnection(usePortal);
        try
        {
            ct.ThrowIfCancellationRequested();
            connection.Initialize(ref descriptor);
            await connection.SynchronizeAsync(ct).ConfigureAwait(false);
            // Registry callbacks bind nodes and request their formats. A second barrier
            // covers those requests, which were issued after the initial barrier.
            await connection.SynchronizeAsync(ct).ConfigureAwait(false);
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
        finally
        {
            if (descriptor >= 0) PipeWireNative.close(descriptor);
        }
    }

    private sealed class Listeners : IDisposable
    {
        private readonly List<IntPtr> allocations = new();
        private readonly List<Delegate> callbacks = new();

        internal IntPtr Hook()
        {
            var pointer = PipeWireNative.AllocateZero(6 * IntPtr.Size);
            this.allocations.Add(pointer);
            return pointer;
        }

        internal IntPtr Callback<T>(T callback) where T : Delegate
        {
            this.callbacks.Add(callback);
            // The concrete delegate type must remain visible to the AOT compiler.
            return Marshal.GetFunctionPointerForDelegate(callback);
        }

        internal IntPtr Events(params IntPtr[] events)
        {
            var pointer = PipeWireNative.CreateEvents(0, events);
            this.allocations.Add(pointer);
            return pointer;
        }

        // The native owner must be destroyed before its hooks and delegates.
        public void Dispose()
        {
            foreach (var pointer in this.allocations) Marshal.FreeHGlobal(pointer);
            this.allocations.Clear();
            GC.KeepAlive(this.callbacks);
            this.callbacks.Clear();
        }
    }

    private void Initialize(ref int descriptor)
    {
        _ = initialized.Value;
        this.loop = PipeWireNative.Require(PipeWireNative.pw_thread_loop_new("FlashCap camera", IntPtr.Zero), "create loop");
        this.context = PipeWireNative.Require(PipeWireNative.pw_context_new(
            PipeWireNative.pw_thread_loop_get_loop(this.loop), IntPtr.Zero, UIntPtr.Zero), "create context");
        if (this.usePortal)
        {
            var ownedDescriptor = descriptor;
            descriptor = -1; // connect_fd consumes the descriptor even if it fails.
            this.core = PipeWireNative.Require(PipeWireNative.pw_context_connect_fd(
                this.context, ownedDescriptor, IntPtr.Zero, UIntPtr.Zero), "connect portal remote");
        }
        else
            this.core = PipeWireNative.Require(PipeWireNative.pw_context_connect(
                this.context, IntPtr.Zero, UIntPtr.Zero), "connect local remote");
        PipeWireNative.DoneCallback done = (_, id, sequence) =>
        {
            if (id == 0 && this.pending.TryGetValue(sequence, out var completion))
                completion.TrySetResult(true);
        };
        PipeWireNative.ErrorCallback error = (_, id, sequence, result, message) =>
            this.Fail(new IOException("FlashCap PipeWire: " + Marshal.PtrToStringAnsi(message) + $" ({result})."));
        PipeWireNative.AddListener(this.core, this.listeners.Hook(), this.listeners.Events(
            default, this.listeners.Callback(done), default, this.listeners.Callback(error),
            default, default, default, default));
        this.registry = PipeWireNative.GetRegistry(this.core);
        PipeWireNative.GlobalCallback global = (_, id, permissions, type, version, properties) =>
        {
            try { this.AddNode(id, type, version, properties); }
            catch (Exception exception) { this.Fail(exception); }
        };
        PipeWireNative.RemoveCallback remove = (_, id) =>
        {
            if (this.nodes.TryGetValue(id, out var node))
            {
                this.nodes.Remove(id);
                PipeWireNative.pw_proxy_destroy(node.Proxy);
                node.Listeners.Dispose();
                if (node.Camera.Serial == this.targetSerial)
                    this.Fail(new IOException("The PipeWire camera was removed."));
            }
        };
        PipeWireNative.AddListener(this.registry, this.listeners.Hook(),
            this.listeners.Events(this.listeners.Callback(global), this.listeners.Callback(remove)));
        PipeWireNative.Check(PipeWireNative.pw_thread_loop_start(this.loop), "start loop");
        this.loopStarted = true;
    }

    private void AddNode(uint id, IntPtr type, uint version, IntPtr properties)
    {
        if (Marshal.PtrToStringAnsi(type) != "PipeWire:Interface:Node") return;
        // A portal remote can announce the same global more than once. Keep the
        // existing binding until global_remove releases it, including its formats.
        if (this.nodes.ContainsKey(id)) return;
        var values = PipeWireNative.ReadDictionary(properties);
        if (!values.TryGetValue("media.class", out var mediaClass) || mediaClass != "Video/Source" ||
            !values.TryGetValue("object.serial", out var serial) ||
            !values.TryGetValue("node.name", out var nodeName))
            return;
        // A camera portal remote exposes only permitted cameras. Some camera providers
        // omit media.role, so do not require it; explicitly different roles are excluded.
        if (values.TryGetValue("media.role", out var role) && role != "Camera") return;
        var name = values.TryGetValue("node.nick", out var nick) ? nick :
            values.TryGetValue("node.description", out var description) ? description : nodeName;
        var camera = new PipeWireCamera(serial, nodeName, name, nodeName, ArrayEx.Empty<PipeWireFormat>(), this.usePortal);
        var node = new Node(camera, PipeWireNative.BindNode(this.registry, id, version));
        this.nodes.Add(id, node);
        PipeWireNative.ParamCallback parameter = (_, sequence, paramId, index, next, value) =>
        {
            if (paramId != PipeWireFormats.EnumFormat || value == IntPtr.Zero) return;
            try { node.Formats.AddRange(PipeWireFormats.Parse(PipeWireFormats.ReadPod(value))); }
            catch (Exception exception) { this.Fail(exception); }
        };
        PipeWireNative.AddListener(node.Proxy, node.Listeners.Hook(),
            node.Listeners.Events(default, node.Listeners.Callback(parameter)));
        PipeWireNative.EnumFormats(node.Proxy);
    }

    private void ThrowIfFailed()
    {
        if (this.disposed) throw new ObjectDisposedException(nameof(PipeWireConnection));
        if (this.failure is { } exception) throw new IOException("The PipeWire connection failed.", exception);
    }

    private void Fail(Exception exception)
    {
        if (this.failure is not null) return;
        this.failure = exception;
        this.capture = null;
        foreach (var completion in this.pending.Values) completion.TrySetException(exception);
        this.streamReady?.TrySetException(exception);
        this.Disconnected?.Invoke();
    }

    private async Task SynchronizeAsync(CancellationToken ct)
    {
        TaskCompletionSource<bool> completion;
        int sequence;
        PipeWireNative.pw_thread_loop_lock(this.loop);
        try
        {
            this.ThrowIfFailed();
            ct.ThrowIfCancellationRequested();
            sequence = PipeWireNative.Sync(this.core);
            completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.pending.Add(sequence, completion);
        }
        finally { PipeWireNative.pw_thread_loop_unlock(this.loop); }
        try
        {
            using (ct.Register(() => completion.TrySetCanceled()))
                await completion.Task.ConfigureAwait(false);
        }
        finally
        {
            PipeWireNative.pw_thread_loop_lock(this.loop);
            try { this.pending.Remove(sequence); }
            finally { PipeWireNative.pw_thread_loop_unlock(this.loop); }
        }
    }

    internal PipeWireCamera[] GetCameras()
    {
        PipeWireNative.pw_thread_loop_lock(this.loop);
        try
        {
            this.ThrowIfFailed();
            return this.nodes.Values.Select(node => new PipeWireCamera(
                node.Camera.Serial, node.Camera.NodeName, node.Camera.Name, node.Camera.Description,
                node.Formats.GroupBy(format => format.Characteristics).Select(group => group.First()).ToArray(), this.usePortal))
                .Where(camera => camera.Formats.Length != 0).ToArray();
        }
        finally { PipeWireNative.pw_thread_loop_unlock(this.loop); }
    }

    internal async Task StartAsync(PipeWireCamera camera, PipeWireFormat format,
        Action<PipeWirePlane[]> capture, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        PipeWireNative.pw_thread_loop_lock(this.loop);
        try
        {
            this.ThrowIfFailed();
            // Resolve target.object through the session manager. Camera visibility
            // on a new portal remote can lag behind its initial registry snapshot.
            this.streamReady = completion;
            this.targetSerial = camera.Serial;
            this.capture = capture;
            this.streamListeners = new Listeners();
            var properties = PipeWireNative.Require(PipeWireNative.pw_properties_new(IntPtr.Zero), "create stream properties");
            try
            {
                foreach (var property in new[]
                {
                    new KeyValuePair<string, string>("media.type", "Video"),
                    new KeyValuePair<string, string>("media.category", "Capture"),
                    new KeyValuePair<string, string>("media.role", "Camera"),
                    new KeyValuePair<string, string>("target.object", camera.Serial),
                })
                    PipeWireNative.Check(PipeWireNative.pw_properties_set(properties, property.Key, property.Value), "set stream properties");
                var ownedProperties = properties;
                properties = IntPtr.Zero;
                this.stream = PipeWireNative.Require(PipeWireNative.pw_stream_new(this.core, "FlashCap camera", ownedProperties), "create stream");
            }
            finally { if (properties != IntPtr.Zero) PipeWireNative.pw_properties_free(properties); }
            PipeWireNative.StreamStateCallback state = (_, previous, current, error) =>
            {
                if (current < 0) this.Fail(new IOException("FlashCap PipeWire stream: " + Marshal.PtrToStringAnsi(error)));
                this.streaming = current == 3;
                this.CompleteStart();
            };
            PipeWireNative.StreamParamCallback parameter = (_, id, value) =>
            {
                if (id != PipeWireFormats.Format || value == IntPtr.Zero) return;
                try
                {
                    var actual = PipeWireFormats.Parse(PipeWireFormats.ReadPod(value));
                    if (!actual.Any(candidate => candidate.MediaSubtype == format.MediaSubtype &&
                        candidate.SpaFormat == format.SpaFormat &&
                        candidate.Characteristics.PixelFormat == format.Characteristics.PixelFormat &&
                        candidate.Characteristics.Width == format.Characteristics.Width &&
                        candidate.Characteristics.Height == format.Characteristics.Height))
                        throw new IOException("PipeWire negotiated an unsupported camera format.");
                    UpdateParameters(this.stream, PipeWireFormats.CreateBufferParameters());
                    this.formatReady = true;
                    this.CompleteStart();
                }
                catch (Exception exception) { this.Fail(exception); }
            };
            PipeWireNative.ProcessCallback process = _ => this.Process();
            PipeWireNative.pw_stream_add_listener(this.stream, this.streamListeners.Hook(),
                this.streamListeners.Events(default, this.streamListeners.Callback(state), default, default,
                    this.streamListeners.Callback(parameter), default, default,
                    this.streamListeners.Callback(process), default), IntPtr.Zero);
            var pod = PipeWireFormats.CreateFormat(format);
            unsafe
            {
                fixed (byte* pointer = pod)
                    PipeWireNative.Check(PipeWireNative.pw_stream_connect(this.stream, 0, uint.MaxValue,
                        // AUTOCONNECT | MAP_BUFFERS | NO_CONVERT | DONT_RECONNECT
                        1 | 4 | 32 | 128, new[] { (IntPtr)pointer }, 1), "connect camera stream");
            }
        }
        catch
        {
            this.DestroyStream();
            throw;
        }
        finally { PipeWireNative.pw_thread_loop_unlock(this.loop); }
        try
        {
            using (ct.Register(() => completion.TrySetCanceled()))
                await completion.Task.ConfigureAwait(false);
        }
        catch
        {
            this.Stop();
            throw;
        }
    }

    private void CompleteStart()
    {
        if (this.streaming && this.formatReady) this.streamReady?.TrySetResult(true);
    }

    private static unsafe void UpdateParameters(IntPtr stream, byte[] pod)
    {
        fixed (byte* pointer = pod)
            PipeWireNative.Check(PipeWireNative.pw_stream_update_params(stream, new[] { (IntPtr)pointer }, 1), "set buffer parameters");
    }

    private void Process()
    {
        var buffer = PipeWireNative.pw_stream_dequeue_buffer(this.stream);
        if (buffer == IntPtr.Zero) return;
        try
        {
            var value = Marshal.PtrToStructure<PipeWireNative.Buffer>(Marshal.ReadIntPtr(buffer));
            if (value.DataCount == 0 || value.DataCount > 2)
                throw new InvalidDataException("Unsupported PipeWire buffer plane count.");
            if (this.planes.Length != value.DataCount) this.planes = new PipeWirePlane[value.DataCount];
            var size = Marshal.SizeOf<PipeWireNative.Data>();
            for (var index = 0; index < this.planes.Length; index++)
            {
                var data = Marshal.PtrToStructure<PipeWireNative.Data>(IntPtr.Add(value.Data, index * size));
                if (data.Type is not (1 or 2) || data.Pointer == IntPtr.Zero || data.Chunk == IntPtr.Zero)
                    throw new InvalidDataException("PipeWire did not provide a CPU-readable buffer.");
                var chunk = Marshal.PtrToStructure<PipeWireNative.Chunk>(data.Chunk);
                if ((chunk.Flags & 3) != 0 || chunk.Size == 0) return; // Corrupted or empty frame.
                if (data.MaximumSize == 0) throw new InvalidDataException("Empty PipeWire allocation.");
                var offset = chunk.Offset % data.MaximumSize;
                var length = Math.Min(chunk.Size, data.MaximumSize);
                if ((ulong)offset + length > data.MaximumSize || offset > int.MaxValue || length > int.MaxValue)
                    throw new InvalidDataException("Wrapped PipeWire video buffers are not supported.");
                this.planes[index] = new PipeWirePlane(IntPtr.Add(data.Pointer, (int)offset), (int)length, chunk.Stride);
            }
            // Capture copies the managed payload before this callback returns.
            this.capture?.Invoke(this.planes);
        }
        catch (Exception exception)
        {
            this.Fail(exception);
            Trace.WriteLine("FlashCap PipeWire capture failed: " + exception);
        }
        finally { PipeWireNative.pw_stream_queue_buffer(this.stream, buffer); }
    }

    internal void Stop()
    {
        PipeWireNative.pw_thread_loop_lock(this.loop);
        try { this.DestroyStream(); }
        finally { PipeWireNative.pw_thread_loop_unlock(this.loop); }
    }

    private void DestroyStream()
    {
        this.capture = null;
        if (this.stream != IntPtr.Zero)
        {
            PipeWireNative.pw_stream_destroy(this.stream);
            this.stream = IntPtr.Zero;
        }
        this.formatReady = false;
        this.streaming = false;
        this.streamReady = null;
        this.streamListeners?.Dispose();
        this.streamListeners = null;
    }

    public void Dispose()
    {
        if (this.disposed) return;
        // Stop without holding the lock, as required by pw_thread_loop_stop.
        if (this.loopStarted) PipeWireNative.pw_thread_loop_stop(this.loop);
        this.disposed = true;
        this.Fail(new ObjectDisposedException(nameof(PipeWireConnection)));
        this.DestroyStream();
        foreach (var node in this.nodes.Values)
        {
            PipeWireNative.pw_proxy_destroy(node.Proxy);
            node.Listeners.Dispose();
        }
        this.nodes.Clear();
        if (this.registry != IntPtr.Zero) PipeWireNative.pw_proxy_destroy(this.registry);
        if (this.core != IntPtr.Zero) PipeWireNative.pw_core_disconnect(this.core);
        if (this.context != IntPtr.Zero) PipeWireNative.pw_context_destroy(this.context);
        if (this.loop != IntPtr.Zero) PipeWireNative.pw_thread_loop_destroy(this.loop);
        this.listeners.Dispose();
        this.Disconnected = null;
    }
}
#endif
