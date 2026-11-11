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
using System.IO;
using System.Runtime.InteropServices;

namespace FlashCap.Internal.PipeWire;

// Public libpipewire/SPA ABI. Inline interface methods dispatch through spa_interface.cb;
// they are not exported symbols and cannot be called with DllImport directly.
internal static class PipeWireNative
{
    private const string Library = "libpipewire-0.3.so.0";

    [StructLayout(LayoutKind.Sequential)]
    private struct Interface
    {
        internal IntPtr Type;
        internal uint Version;
        internal IntPtr Methods;
        internal IntPtr Data;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Dictionary
    {
        internal uint Flags, Count;
        internal IntPtr Items;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Buffer
    {
        internal uint MetadataCount, DataCount;
        internal IntPtr Metadata, Data;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Data
    {
        internal uint Type, Flags;
        internal long FileDescriptor;
        internal uint MapOffset, MaximumSize;
        internal IntPtr Pointer, Chunk;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Chunk
    {
        internal uint Offset, Size;
        internal int Stride, Flags;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DoneCallback(IntPtr data, uint id, int sequence);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ErrorCallback(IntPtr data, uint id, int sequence, int result, IntPtr message);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void GlobalCallback(IntPtr data, uint id, uint permissions, IntPtr type, uint version, IntPtr properties);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void RemoveCallback(IntPtr data, uint id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ParamCallback(IntPtr data, int sequence, uint id, uint index, uint next, IntPtr parameter);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void PermissionsCallback(IntPtr data, uint index, uint count, IntPtr permissions);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void StreamStateCallback(IntPtr data, int previous, int state, IntPtr error);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void StreamParamCallback(IntPtr data, uint id, IntPtr parameter);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ProcessCallback(IntPtr data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int AddListenerMethod(IntPtr data, IntPtr hook, IntPtr events, IntPtr userData);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr GetRegistryMethod(IntPtr data, uint version, UIntPtr userSize);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int SyncMethod(IntPtr data, uint id, int sequence);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr BindMethod(IntPtr data, uint id, [MarshalAs(UnmanagedType.LPStr)] string type, uint version, UIntPtr userSize);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int EnumParamsMethod(IntPtr data, int sequence, uint id, uint start, uint count, IntPtr filter);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int SetParamMethod(IntPtr data, uint id, uint flags, IntPtr parameter);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetPermissionsMethod(IntPtr data, uint index, uint count);

    // PW_PERM_R | PW_PERM_X: discover a node and read its formats.
    internal const uint ReadExecute = 0x100 | 0x40;

    private static IntPtr Method(Interface iface, int slot) =>
        Marshal.ReadIntPtr(iface.Methods, checked(IntPtr.Size * (slot + 1)));

    internal static void AddListener(IntPtr instance, IntPtr hook, IntPtr events)
    {
        var iface = Marshal.PtrToStructure<Interface>(instance);
        Check(Marshal.GetDelegateForFunctionPointer<AddListenerMethod>(Method(iface, 0))
            (iface.Data, hook, events, IntPtr.Zero), "subscribe");
    }

    internal static IntPtr GetRegistry(IntPtr core)
    {
        var iface = Marshal.PtrToStructure<Interface>(core);
        return Require(Marshal.GetDelegateForFunctionPointer<GetRegistryMethod>(Method(iface, 5))
            (iface.Data, 3, UIntPtr.Zero), "get registry");
    }

    internal static int Sync(IntPtr core)
    {
        var iface = Marshal.PtrToStructure<Interface>(core);
        var sequence = Marshal.GetDelegateForFunctionPointer<SyncMethod>(Method(iface, 2))(iface.Data, 0, 0);
        Check(sequence, "synchronize");
        return sequence;
    }

    internal static IntPtr BindNode(IntPtr registry, uint id, uint version)
    {
        var iface = Marshal.PtrToStructure<Interface>(registry);
        return Require(Marshal.GetDelegateForFunctionPointer<BindMethod>(Method(iface, 1))
            (iface.Data, id, "PipeWire:Interface:Node", Math.Min(version, 3), UIntPtr.Zero), "bind camera node");
    }

    internal static void EnumFormats(IntPtr node)
        => EnumParams(node, PipeWireFormats.EnumFormat);

    internal static int EnumParams(IntPtr node, uint id)
    {
        var iface = Marshal.PtrToStructure<Interface>(node);
        var sequence = Marshal.GetDelegateForFunctionPointer<EnumParamsMethod>(Method(iface, 2))
            (iface.Data, 0, id, 0, uint.MaxValue, IntPtr.Zero);
        Check(sequence, "enumerate parameters");
        return sequence;
    }

    internal static unsafe void SetProps(IntPtr node, byte[] pod)
    {
        var iface = Marshal.PtrToStructure<Interface>(node);
        fixed (byte* pointer = pod)
            Check(Marshal.GetDelegateForFunctionPointer<SetParamMethod>(Method(iface, 3))
                (iface.Data, PipeWireControls.Props, 0, (IntPtr)pointer), "set camera control");
    }

    internal static void GetPermissions(IntPtr client)
    {
        var iface = Marshal.PtrToStructure<Interface>(client);
        Check(Marshal.GetDelegateForFunctionPointer<GetPermissionsMethod>(Method(iface, 3))
            (iface.Data, 0, uint.MaxValue), "query client permissions");
    }

    internal static Dictionary<string, string> ReadDictionary(IntPtr pointer)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (pointer == IntPtr.Zero) return result;
        var dictionary = Marshal.PtrToStructure<Dictionary>(pointer);
        for (var index = 0u; index < dictionary.Count; index++)
        {
            var item = IntPtr.Add(dictionary.Items, checked((int)index * IntPtr.Size * 2));
            var key = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(item));
            var value = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(item, IntPtr.Size));
            if (key is not null && value is not null) result[key] = value;
        }
        return result;
    }

    internal static IntPtr AllocateZero(int length)
    {
        var pointer = Marshal.AllocHGlobal(length);
        Marshal.Copy(new byte[length], 0, pointer, length);
        return pointer;
    }

    internal static IntPtr CreateEvents(uint version, params IntPtr[] callbacks)
    {
        var pointer = AllocateZero(checked((callbacks.Length + 1) * IntPtr.Size));
        Marshal.WriteInt32(pointer, (int)version);
        for (var index = 0; index < callbacks.Length; index++)
            Marshal.WriteIntPtr(pointer, (index + 1) * IntPtr.Size, callbacks[index]);
        return pointer;
    }

    internal static void Check(int result, string operation)
    {
        if (result < 0) throw new IOException($"FlashCap PipeWire could not {operation}: {result}.");
    }

    internal static IntPtr Require(IntPtr pointer, string operation)
    {
        if (pointer == IntPtr.Zero) throw new IOException($"FlashCap PipeWire could not {operation}.");
        return pointer;
    }

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern void pw_init(IntPtr argc, IntPtr argv);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr pw_thread_loop_new(string name, IntPtr properties);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern void pw_thread_loop_destroy(IntPtr loop);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr pw_thread_loop_get_loop(IntPtr loop);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int pw_thread_loop_start(IntPtr loop);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern void pw_thread_loop_stop(IntPtr loop);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern void pw_thread_loop_lock(IntPtr loop);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern void pw_thread_loop_unlock(IntPtr loop);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr pw_context_new(IntPtr loop, IntPtr properties, UIntPtr userSize);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern void pw_context_destroy(IntPtr context);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr pw_context_connect(IntPtr context, IntPtr properties, UIntPtr userSize);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr pw_context_connect_fd(IntPtr context, int fd, IntPtr properties, UIntPtr userSize);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int pw_core_disconnect(IntPtr core);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr pw_core_get_client(IntPtr core);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern void pw_proxy_destroy(IntPtr proxy);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern uint pw_proxy_get_id(IntPtr proxy);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr pw_properties_new(IntPtr firstKey);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern void pw_properties_free(IntPtr properties);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int pw_properties_set(IntPtr properties, string key, string value);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr pw_stream_new(IntPtr core, string name, IntPtr properties);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern void pw_stream_destroy(IntPtr stream);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern void pw_stream_add_listener(IntPtr stream, IntPtr hook, IntPtr events, IntPtr data);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int pw_stream_connect(IntPtr stream, int direction, uint targetId, uint flags, IntPtr[] parameters, uint count);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int pw_stream_update_params(IntPtr stream, IntPtr[] parameters, uint count);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr pw_stream_dequeue_buffer(IntPtr stream);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int pw_stream_queue_buffer(IntPtr stream, IntPtr buffer);
    [DllImport("libc", CallingConvention = CallingConvention.Cdecl)] internal static extern int close(int fd);
}
#endif
