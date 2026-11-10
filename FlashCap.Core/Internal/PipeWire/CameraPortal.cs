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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCap.Internal.PipeWire;

internal static class CameraPortal
{
    private const string BusName = "org.freedesktop.portal.Desktop";
    private const string ObjectPath = "/org/freedesktop/portal/desktop";
    private const string CameraInterface = "org.freedesktop.portal.Camera";
    private const string RequestInterface = "org.freedesktop.portal.Request";
    private const int AuthenticationClient = 1;
    private const int MessageBusConnection = 8;

    internal static Task<int> OpenRemoteAsync(CancellationToken ct) =>
        // A private GLib context dispatches portal callbacks without depending on a GUI's main loop.
        // iteration(true) sleeps in GLib's event wait; this worker does not poll.
        Task.Factory.StartNew(() => OpenRemote(ct), ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);

    private static int OpenRemote(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var context = IntPtr.Zero;
        var cancellable = IntPtr.Zero;
        var connection = IntPtr.Zero;
        var subscription = 0u;
        var ownerSubscription = 0u;
        var registration = default(CancellationTokenRegistration);
        var accessType = IntPtr.Zero;
        var responseType = IntPtr.Zero;
        var remoteType = IntPtr.Zero;
        var pushedContext = false;
        ReadyCallback? ready = null;
        SignalCallback? response = null;
        SignalCallback? ownerChanged = null;
        try
        {
            context = g_main_context_new();
            cancellable = g_cancellable_new();
            accessType = g_variant_type_new("(o)");
            responseType = g_variant_type_new("(ua{sv})");
            remoteType = g_variant_type_new("(h)");
            g_main_context_push_thread_default(context);
            pushedContext = true;
            registration = ct.Register(() =>
            {
                g_cancellable_cancel(cancellable);
                g_main_context_wakeup(context);
            });
            var address = g_dbus_address_get_for_bus_sync(2, cancellable, out var error);
            ThrowError(error, ct);
            try
            {
                // A private connection avoids changing shared connections or their exit-on-close policy.
                connection = g_dbus_connection_new_for_address_sync(address,
                    AuthenticationClient | MessageBusConnection, IntPtr.Zero, cancellable, out error);
                ThrowError(error, ct);
            }
            finally { g_free(address); }
            g_dbus_connection_set_exit_on_close(connection, 0);
            var sender = Marshal.PtrToStringAnsi(g_dbus_connection_get_unique_name(connection))!;
            var token = "flashcap_" + Guid.NewGuid().ToString("N");
            var requestPath = ObjectPath + "/request/" + sender.Substring(1).Replace('.', '_') + "/" + token;
            uint? responseCode = null;
            Exception? failure = null;
            var returned = false;
            ownerChanged = (bus, source, path, iface, signal, parameters, userData) =>
            {
                var owner = g_variant_get_child_value(parameters, (UIntPtr)2);
                try
                {
                    if (Marshal.PtrToStringAnsi(g_variant_get_string(owner, IntPtr.Zero)) == "")
                        failure = new IOException("The camera portal closed before responding.");
                }
                finally { g_variant_unref(owner); }
            };
            ownerSubscription = g_dbus_connection_signal_subscribe(connection, "org.freedesktop.DBus",
                "org.freedesktop.DBus", "NameOwnerChanged", "/org/freedesktop/DBus",
                BusName, 0, ownerChanged, IntPtr.Zero, IntPtr.Zero);
            response = (bus, source, path, iface, signal, parameters, userData) =>
            {
                try
                {
                    if (g_variant_is_of_type(parameters, responseType) == 0)
                        throw new IOException("The camera portal returned an invalid response.");
                    var child = g_variant_get_child_value(parameters, UIntPtr.Zero);
                    try { responseCode = g_variant_get_uint32(child); }
                    finally { g_variant_unref(child); }
                }
                catch (Exception exception) { failure = exception; }
            };
            ready = (source, result, userData) =>
            {
                try
                {
                    var reply = g_dbus_connection_call_finish(source, result, out var callError);
                    ThrowError(callError, ct);
                    try
                    {
                        var child = g_variant_get_child_value(reply, UIntPtr.Zero);
                        try
                        {
                            var actualPath = Marshal.PtrToStringAnsi(g_variant_get_string(child, IntPtr.Zero));
                            if (actualPath != requestPath)
                                throw new IOException("The camera portal returned an unexpected request handle.");
                        }
                        finally { g_variant_unref(child); }
                    }
                    finally { g_variant_unref(reply); }
                }
                catch (Exception exception) { failure = exception; }
                finally { returned = true; }
            };
            // Subscribe before calling AccessCamera, including when a remembered grant responds immediately.
            subscription = g_dbus_connection_signal_subscribe(connection, BusName, RequestInterface,
                "Response", requestPath, null, 0, response, IntPtr.Zero, IntPtr.Zero);
            var options = Parse("({'handle_token': <'" + token + "'>},)");
            try
            {
                g_dbus_connection_call(connection, BusName, ObjectPath, CameraInterface, "AccessCamera",
                    options, accessType, 0, -1, cancellable, ready, IntPtr.Zero);
            }
            finally { g_variant_unref(options); }
            // Drain the method callback even on cancellation so its delegate cannot outlive this context.
            while (!returned || (failure is null && !responseCode.HasValue && !ct.IsCancellationRequested))
            {
                g_main_context_iteration(context, 1);
                if (g_dbus_connection_is_closed(connection) != 0)
                    failure = new IOException("The session bus closed before the camera portal responded.");
            }
            if (ct.IsCancellationRequested)
            {
                g_dbus_connection_call(connection, BusName, requestPath, RequestInterface, "Close",
                    IntPtr.Zero, IntPtr.Zero, 1, -1, IntPtr.Zero, null, IntPtr.Zero); // NO_AUTO_START
                _ = g_dbus_connection_flush_sync(connection, IntPtr.Zero, out var flushError);
                if (flushError != IntPtr.Zero) g_error_free(flushError);
                ct.ThrowIfCancellationRequested();
            }
            if (failure is not null) throw failure;
            if (responseCode == 1) throw new OperationCanceledException("Camera access was cancelled by the user.");
            if (responseCode != 0) throw new UnauthorizedAccessException("Camera access was denied by the portal.");

            options = Parse("(@a{sv} {},)");
            try
            {
                var reply = g_dbus_connection_call_with_unix_fd_list_sync(connection, BusName, ObjectPath,
                    CameraInterface, "OpenPipeWireRemote", options, remoteType, 0, -1, IntPtr.Zero,
                    out var fdList, cancellable, out error);
                ThrowError(error, ct);
                try
                {
                    var child = g_variant_get_child_value(reply, UIntPtr.Zero);
                    try
                    {
                        // The D-Bus handle is an index, not the received Unix file descriptor.
                        var fd = g_unix_fd_list_get(fdList, g_variant_get_handle(child), out error);
                        ThrowError(error, ct);
                        if (fd < 0) throw new IOException("The camera portal did not return a PipeWire descriptor.");
                        return fd;
                    }
                    finally { g_variant_unref(child); }
                }
                finally
                {
                    g_variant_unref(reply);
                    if (fdList != IntPtr.Zero) g_object_unref(fdList);
                }
            }
            finally { g_variant_unref(options); }
        }
        finally
        {
            registration.Dispose();
            if (connection != IntPtr.Zero)
            {
                if (subscription != 0) g_dbus_connection_signal_unsubscribe(connection, subscription);
                if (ownerSubscription != 0) g_dbus_connection_signal_unsubscribe(connection, ownerSubscription);
                _ = g_dbus_connection_close_sync(connection, IntPtr.Zero, out var error);
                if (error != IntPtr.Zero) g_error_free(error);
                g_object_unref(connection);
            }
            if (pushedContext) g_main_context_pop_thread_default(context);
            if (context != IntPtr.Zero) g_main_context_unref(context);
            if (cancellable != IntPtr.Zero) g_object_unref(cancellable);
            if (accessType != IntPtr.Zero) g_variant_type_free(accessType);
            if (responseType != IntPtr.Zero) g_variant_type_free(responseType);
            if (remoteType != IntPtr.Zero) g_variant_type_free(remoteType);
            GC.KeepAlive(ready);
            GC.KeepAlive(response);
            GC.KeepAlive(ownerChanged);
        }
    }

    private static IntPtr Parse(string text)
    {
        var value = g_variant_parse(IntPtr.Zero, text, IntPtr.Zero, IntPtr.Zero, out var error);
        ThrowError(error, default);
        return value;
    }

    private static void ThrowError(IntPtr error, CancellationToken ct)
    {
        if (error == IntPtr.Zero) return;
        string? message;
        try { message = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(error, 8)); }
        finally { g_error_free(error); }
        ct.ThrowIfCancellationRequested();
        throw new IOException("FlashCap camera portal: " + message);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ReadyCallback(IntPtr source, IntPtr result, IntPtr data);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SignalCallback(IntPtr connection, IntPtr sender, IntPtr path,
        IntPtr iface, IntPtr signal, IntPtr parameters, IntPtr data);

    private const string Glib = "libglib-2.0.so.0";
    private const string Gio = "libgio-2.0.so.0";
    private const string GObject = "libgobject-2.0.so.0";
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr g_main_context_new();
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern void g_main_context_unref(IntPtr context);
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern void g_main_context_push_thread_default(IntPtr context);
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern void g_main_context_pop_thread_default(IntPtr context);
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern int g_main_context_iteration(IntPtr context, int mayBlock);
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern void g_main_context_wakeup(IntPtr context);
    [DllImport(Gio, CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr g_cancellable_new();
    [DllImport(Gio, CallingConvention = CallingConvention.Cdecl)] private static extern void g_cancellable_cancel(IntPtr cancellable);
    [DllImport(GObject, CallingConvention = CallingConvention.Cdecl)] private static extern void g_object_unref(IntPtr value);
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern void g_free(IntPtr value);
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern void g_error_free(IntPtr error);
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr g_variant_parse(IntPtr type, string text, IntPtr limit, IntPtr end, out IntPtr error);
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern void g_variant_unref(IntPtr value);
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr g_variant_type_new(string type);
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern void g_variant_type_free(IntPtr type);
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern int g_variant_is_of_type(IntPtr value, IntPtr type);
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr g_variant_get_child_value(IntPtr value, UIntPtr index);
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern uint g_variant_get_uint32(IntPtr value);
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern int g_variant_get_handle(IntPtr value);
    [DllImport(Glib, CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr g_variant_get_string(IntPtr value, IntPtr length);
    [DllImport(Gio, CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr g_dbus_address_get_for_bus_sync(int bus, IntPtr cancellable, out IntPtr error);
    [DllImport(Gio, CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr g_dbus_connection_new_for_address_sync(IntPtr address, int flags, IntPtr observer, IntPtr cancellable, out IntPtr error);
    [DllImport(Gio, CallingConvention = CallingConvention.Cdecl)] private static extern void g_dbus_connection_set_exit_on_close(IntPtr connection, int exit);
    [DllImport(Gio, CallingConvention = CallingConvention.Cdecl)] private static extern int g_dbus_connection_close_sync(IntPtr connection, IntPtr cancellable, out IntPtr error);
    [DllImport(Gio, CallingConvention = CallingConvention.Cdecl)] private static extern int g_dbus_connection_is_closed(IntPtr connection);
    [DllImport(Gio, CallingConvention = CallingConvention.Cdecl)] private static extern int g_dbus_connection_flush_sync(IntPtr connection, IntPtr cancellable, out IntPtr error);
    [DllImport(Gio, CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr g_dbus_connection_get_unique_name(IntPtr connection);
    [DllImport(Gio, CallingConvention = CallingConvention.Cdecl)] private static extern uint g_dbus_connection_signal_subscribe(IntPtr connection, string sender, string iface, string member, string path, string? arg0, uint flags, SignalCallback callback, IntPtr data, IntPtr destroy);
    [DllImport(Gio, CallingConvention = CallingConvention.Cdecl)] private static extern void g_dbus_connection_signal_unsubscribe(IntPtr connection, uint subscription);
    [DllImport(Gio, CallingConvention = CallingConvention.Cdecl)] private static extern void g_dbus_connection_call(IntPtr connection, string bus, string path, string iface, string method, IntPtr parameters, IntPtr replyType, int flags, int timeout, IntPtr cancellable, ReadyCallback? callback, IntPtr data);
    [DllImport(Gio, CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr g_dbus_connection_call_finish(IntPtr connection, IntPtr result, out IntPtr error);
    [DllImport(Gio, CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr g_dbus_connection_call_with_unix_fd_list_sync(IntPtr connection, string bus, string path, string iface, string method, IntPtr parameters, IntPtr replyType, int flags, int timeout, IntPtr inFds, out IntPtr outFds, IntPtr cancellable, out IntPtr error);
    [DllImport(Gio, CallingConvention = CallingConvention.Cdecl)] private static extern int g_unix_fd_list_get(IntPtr list, int index, out IntPtr error);
}
#endif
