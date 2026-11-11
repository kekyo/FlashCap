// FlashCap PipeWire integration fixture. This process owns a private server,
// a deterministic camera and a Camera portal on the test's private D-Bus.
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/socket.h>
#include <sys/un.h>
#include <unistd.h>
#include <gio/gio.h>
#include <gio/gunixfdlist.h>
#include <glib-unix.h>
#include <pipewire/pipewire.h>
#include <pipewire/impl-client.h>
#include <pipewire/impl-node.h>
#include <spa/param/video/format-utils.h>
#include <spa/param/props.h>

static struct pw_thread_loop *thread_loop;
static struct pw_context *context;
static struct pw_core *core;
static struct pw_registry *registry;
static struct pw_stream *camera, *extra_camera;
static struct spa_hook extra_stream_hook;
static uint32_t extra_output_port = SPA_ID_INVALID;
static bool extra_ready, expose_direct;
static gint next_portal_connection;
static struct spa_hook registry_hook, stream_hook, context_hook;
static struct spa_source *timer;
static struct pw_proxy *links[64];
static uint32_t input_ports[64];
static size_t link_count;
static uint32_t output_port = SPA_ID_INVALID;
static bool camera_ready;
static unsigned frame_number;
static unsigned client_number;
static GMainLoop *main_loop;
static const char *mode;
static GDBusNodeInfo *request_info;
static guint portal_owner;
static int brightness = 128;

static void fatal(const char *message)
{
    fprintf(stderr, "Fixture: %s (%s)\n", message, strerror(errno));
    exit(1);
}

static void process_frame(void *data)
{
    struct pw_stream *source = data;
    struct pw_buffer *buffer = pw_stream_dequeue_buffer(source);
    if (buffer == NULL) return;
    struct spa_data *plane = &buffer->buffer->datas[0];
    // Alternate tight and padded layouts, with a nonzero data offset, in the
    // same negotiated stream. Pixel values stay constant in both layouts.
    unsigned stride = (++frame_number & 1) ? 8 : 4;
    unsigned offset = 4;
    if (plane->data == NULL || plane->maxsize < offset + stride * 3)
        fatal("camera buffer is too small");
    memset(plane->data, 0xee, plane->maxsize);
    for (unsigned row = 0; row < 3; row++)
        for (unsigned column = 0; column < 4; column++)
            ((unsigned char *)plane->data)[offset + row * stride + column] = (source == extra_camera ? 101 : 1) + row * 4 + column;
    plane->chunk->offset = offset;
    plane->chunk->size = stride * 3;
    plane->chunk->stride = stride;
    plane->chunk->flags = 0;
    pw_stream_queue_buffer(source, buffer);
}

static void format_changed(void *data, uint32_t id, const struct spa_pod *param)
{
    struct pw_stream *source = data;
    if (id == SPA_PARAM_Props && param != NULL) {
        if (strcmp(mode, "controls-ignored") == 0) return;
        if (spa_pod_parse_object(param, SPA_TYPE_OBJECT_Props, NULL,
            SPA_PROP_brightness, SPA_POD_OPT_Int(&brightness)) < 0) fatal("parse camera controls");
        unsigned char storage[128];
        struct spa_pod_builder builder = SPA_POD_BUILDER_INIT(storage, sizeof(storage));
        const struct spa_pod *props = spa_pod_builder_add_object(&builder,
            SPA_TYPE_OBJECT_Props, SPA_PARAM_Props, SPA_PROP_brightness, SPA_POD_Int(brightness));
        if (pw_stream_update_params(source, &props, 1) < 0) fatal("update camera controls");
        return;
    }
    if (id != SPA_PARAM_Format || param == NULL) return;
    unsigned char storage[512];
    struct spa_pod_builder builder = SPA_POD_BUILDER_INIT(storage, sizeof(storage));
    const struct spa_pod *parameters[] = {
        spa_pod_builder_add_object(&builder, SPA_TYPE_OBJECT_ParamBuffers, SPA_PARAM_Buffers,
            SPA_PARAM_BUFFERS_buffers, SPA_POD_CHOICE_RANGE_Int(4, 2, 8),
            SPA_PARAM_BUFFERS_blocks, SPA_POD_Int(1),
            SPA_PARAM_BUFFERS_size, SPA_POD_Int(28),
            SPA_PARAM_BUFFERS_stride, SPA_POD_Int(8),
            SPA_PARAM_BUFFERS_dataType, SPA_POD_CHOICE_FLAGS_Int(1 << SPA_DATA_MemFd))
    };
    if (pw_stream_update_params(source, parameters, 1) < 0) fatal("update camera parameters");
}

static void state_changed(void *data, enum pw_stream_state old,
    enum pw_stream_state state, const char *error)
{
    (void)old;
    if (state == PW_STREAM_STATE_ERROR) fatal(error);
    if (state == PW_STREAM_STATE_PAUSED) {
        if (data == camera) camera_ready = true;
        else extra_ready = true;
        pw_thread_loop_signal(thread_loop, false);
    }
}

static void tick(void *data, uint64_t expirations)
{
    (void)data; (void)expirations;
    if (pw_stream_get_state(camera, NULL) == PW_STREAM_STATE_STREAMING)
        pw_stream_trigger_process(camera);
    if (extra_camera != NULL && pw_stream_get_state(extra_camera, NULL) == PW_STREAM_STATE_STREAMING)
        pw_stream_trigger_process(extra_camera);
}

static void global_added(void *data, uint32_t id, uint32_t permissions,
    const char *type, uint32_t version, const struct spa_dict *props)
{
    (void)data; (void)permissions; (void)version;
    if (strcmp(mode, "delayed") == 0 && strcmp(type, PW_TYPE_INTERFACE_Node) == 0 && props != NULL) {
        const char *media_class = spa_dict_lookup(props, PW_KEY_MEDIA_CLASS);
        const char *client_id = spa_dict_lookup(props, PW_KEY_CLIENT_ID);
        if (media_class != NULL && strcmp(media_class, "Stream/Input/Video") == 0 && client_id != NULL) {
            // A session manager grants access after discovering the capture stream.
            // The opening connection has already received its registry snapshot.
            struct pw_client *client = pw_registry_bind(registry, (uint32_t)strtoul(client_id, NULL, 10),
                PW_TYPE_INTERFACE_Client, PW_VERSION_CLIENT, 0);
            if (client == NULL) fatal("bind capture client");
            struct pw_permission permission = PW_PERMISSION_INIT(pw_stream_get_node_id(camera), PW_PERM_ALL);
            if (pw_client_update_permissions(client, 1, &permission) < 0) fatal("grant camera access");
            pw_proxy_destroy((struct pw_proxy *)client);
        }
    }
    if (strcmp(type, PW_TYPE_INTERFACE_Port) != 0 || props == NULL) return;
    const char *direction = spa_dict_lookup(props, PW_KEY_PORT_DIRECTION);
    const char *node = spa_dict_lookup(props, PW_KEY_NODE_ID);
    if (direction == NULL || node == NULL) return;
    if (strcmp(direction, "out") == 0) {
        if ((uint32_t)strtoul(node, NULL, 10) == pw_stream_get_node_id(camera)) output_port = id;
        else extra_output_port = id;
        return;
    }
    if (strcmp(direction, "in") != 0 || output_port == SPA_ID_INVALID) return;
    struct pw_stream *source = camera;
    uint32_t source_port = output_port;
    if (extra_camera != NULL) {
        struct pw_global *input_global = pw_context_find_global(context, (uint32_t)strtoul(node, NULL, 10));
        const struct pw_properties *input_props = pw_impl_node_get_properties(pw_global_get_object(input_global));
        const char *target = pw_properties_get(input_props, "target.object");
        struct pw_global *extra_global = pw_context_find_global(context, pw_stream_get_node_id(extra_camera));
        const char *serial = pw_properties_get(pw_global_get_properties(extra_global), PW_KEY_OBJECT_SERIAL);
        if (target != NULL && serial != NULL && strcmp(target, serial) == 0) {
            source = extra_camera;
            source_port = extra_output_port;
        }
    }
    if (link_count == 64) fatal("too many fixture links");
    struct pw_properties *properties = pw_properties_new(
        PW_KEY_LINK_INPUT_NODE, node, PW_KEY_LINK_PASSIVE, "false", NULL);
    pw_properties_setf(properties, PW_KEY_LINK_INPUT_PORT, "%u", id);
    pw_properties_setf(properties, PW_KEY_LINK_OUTPUT_PORT, "%u", source_port);
    pw_properties_setf(properties, PW_KEY_LINK_OUTPUT_NODE, "%u", pw_stream_get_node_id(source));
    links[link_count] = pw_core_create_object(core, "link-factory",
        PW_TYPE_INTERFACE_Link, PW_VERSION_LINK, &properties->dict, 0);
    input_ports[link_count++] = id;
    pw_properties_free(properties);
}

static void global_removed(void *data, uint32_t id)
{
    (void)data;
    for (size_t i = 0; i < link_count; i++) {
        if (input_ports[i] == id && links[i] != NULL) {
            pw_proxy_destroy(links[i]);
            links[i] = NULL;
        }
    }
}

static void resource_added(void *data, struct pw_resource *resource)
{
    (void)data;
    if (strcmp(pw_resource_get_type(resource, NULL), PW_TYPE_INTERFACE_Registry) != 0) return;
    // Send the camera once before the server's normal initial registry snapshot.
    // This reproduces duplicate globals observed with the desktop camera portal.
    struct pw_global *global = pw_context_find_global(context, pw_stream_get_node_id(camera));
    if (global == NULL) fatal("find camera global");
    pw_resource_call(resource, struct pw_registry_events, global, 0,
        pw_global_get_id(global), pw_global_get_permissions(global, pw_resource_get_client(resource)),
        pw_global_get_type(global), pw_global_get_version(global), &pw_global_get_properties(global)->dict);
}

static void client_destroyed(void *data)
{
    struct spa_hook *hook = data;
    spa_hook_remove(hook);
    free(hook);
}

static void check_access(void *data, struct pw_impl_client *client)
{
    (void)data;
    if (camera_ready && strstr(mode, "controls-readonly") != NULL) {
        struct pw_permission permission = PW_PERMISSION_INIT(
            pw_stream_get_node_id(camera), PW_PERM_R | PW_PERM_X);
        if (pw_impl_client_update_permissions(client, 1, &permission) < 0) fatal("make camera read-only");
        return;
    }
    if (camera_ready && (strncmp(mode, "partial", 7) == 0 || strcmp(mode, "readonly") == 0)) {
        bool portal = g_atomic_int_compare_and_exchange(&next_portal_connection, 1, 0);
        if (!portal) {
            struct pw_permission permission = PW_PERMISSION_INIT(
                pw_stream_get_node_id(extra_camera != NULL ? extra_camera : camera),
                strstr(mode, "readonly") != NULL ? PW_PERM_R : 0);
            if (pw_impl_client_update_permissions(client, 1, &permission) < 0) fatal("restrict direct camera");
        }
        return;
    }
    if (!camera_ready || (strcmp(mode, "duplicate") != 0 && strcmp(mode, "delayed") != 0)) return;
    // Enumeration and opening use separate remotes. Delay the camera's
    // visibility on each opening connection, without a timing-dependent sleep.
    if (strcmp(mode, "delayed") == 0 && (++client_number % 2) == 0) {
        struct pw_permission permission = PW_PERMISSION_INIT(pw_stream_get_node_id(camera), 0);
        if (pw_impl_client_update_permissions(client, 1, &permission) < 0) fatal("hide camera");
    }
    if (strcmp(mode, "delayed") == 0) return;
    struct spa_hook *hook = calloc(1, sizeof(*hook));
    if (hook == NULL) fatal("allocate client listener");
    static const struct pw_impl_client_events events = {
        PW_VERSION_IMPL_CLIENT_EVENTS, .destroy = client_destroyed, .resource_added = resource_added
    };
    pw_impl_client_add_listener(client, hook, &events, hook);
}

static struct pw_stream *create_camera(const char *name, const char *description, struct spa_hook *hook)
{
    struct pw_stream *source = pw_stream_new(core, description, pw_properties_new(
        PW_KEY_NODE_NAME, name, PW_KEY_NODE_DESCRIPTION, description,
        PW_KEY_MEDIA_TYPE, "Video", PW_KEY_MEDIA_CLASS, strcmp(mode, "direct-empty") == 0 ? "Audio/Source" : "Video/Source", PW_KEY_MEDIA_ROLE, "Camera", NULL));
    static const struct pw_stream_events events = {
        PW_VERSION_STREAM_EVENTS, .state_changed = state_changed,
        .param_changed = format_changed, .process = process_frame
    };
    pw_stream_add_listener(source, hook, &events, source);
    unsigned char storage[1024];
    struct spa_pod_builder builder = SPA_POD_BUILDER_INIT(storage, sizeof(storage));
    const struct spa_pod *parameters[] = {
        spa_format_video_raw_build(&builder, SPA_PARAM_EnumFormat, &(struct spa_video_info_raw) {
            .format = SPA_VIDEO_FORMAT_NV12, .size = { 4, 2 }, .framerate = { 30, 1 }
        }),
        spa_pod_builder_add_object(&builder, SPA_TYPE_OBJECT_PropInfo, SPA_PARAM_PropInfo,
            SPA_PROP_INFO_id, SPA_POD_Id(SPA_PROP_brightness),
            SPA_PROP_INFO_type, SPA_POD_CHOICE_STEP_Int(128, 0, 255, 1)),
        spa_pod_builder_add_object(&builder, SPA_TYPE_OBJECT_Props, SPA_PARAM_Props,
            SPA_PROP_brightness, SPA_POD_Int(brightness))
    };
    if (pw_stream_connect(source, PW_DIRECTION_OUTPUT, PW_ID_ANY,
        PW_STREAM_FLAG_DRIVER | PW_STREAM_FLAG_MAP_BUFFERS,
        parameters, strcmp(mode, "controls-unavailable") == 0 ? 1 : SPA_N_ELEMENTS(parameters)) < 0) fatal("connect camera");
    return source;
}

static void start_camera(const char *config)
{
    pw_init(NULL, NULL);
    thread_loop = pw_thread_loop_new("FlashCap fixture", NULL);
    struct pw_loop *loop = pw_thread_loop_get_loop(thread_loop);
    context = pw_context_new(loop, pw_properties_new(
        PW_KEY_CONFIG_NAME, config, PW_KEY_CORE_DAEMON, "true", PW_KEY_CORE_NAME, "flashcap-test", NULL), 0);
    if (context == NULL) fatal("create server");
    static const struct pw_context_events context_events = {
        PW_VERSION_CONTEXT_EVENTS, .check_access = check_access
    };
    pw_context_add_listener(context, &context_hook, &context_events, NULL);
    core = pw_context_connect(context, pw_properties_new(PW_KEY_REMOTE_NAME, "flashcap-test", NULL), 0);
    if (core == NULL) fatal("connect server");
    registry = pw_core_get_registry(core, PW_VERSION_REGISTRY, 0);
    static const struct pw_registry_events registry_events = {
        PW_VERSION_REGISTRY_EVENTS, .global = global_added, .global_remove = global_removed
    };
    pw_registry_add_listener(registry, &registry_hook, &registry_events, NULL);
    camera = create_camera("flashcap.test.camera", "FlashCap fixture camera", &stream_hook);
    if (strncmp(mode, "partial", 7) == 0)
        extra_camera = create_camera("flashcap.test.extra", "FlashCap fixture camera extra", &extra_stream_hook);
    timer = pw_loop_add_timer(loop, tick, NULL);
    struct timespec interval = { .tv_nsec = 33333333 };
    pw_loop_update_timer(loop, timer, &interval, &interval, false);
    pw_thread_loop_lock(thread_loop);
    if (pw_thread_loop_start(thread_loop) < 0) fatal("start loop");
    while (!camera_ready || (extra_camera != NULL && !extra_ready)) pw_thread_loop_wait(thread_loop);
    pw_thread_loop_unlock(thread_loop);
}

static void request_call(GDBusConnection *bus, const gchar *sender, const gchar *path,
    const gchar *interface, const gchar *method, GVariant *parameters,
    GDBusMethodInvocation *invocation, gpointer data)
{
    (void)bus; (void)sender; (void)path; (void)interface;
    (void)method; (void)parameters; (void)data;
    g_dbus_method_invocation_return_value(invocation, NULL);
    puts("CLOSED"); fflush(stdout);
}
static const GDBusInterfaceVTable request_vtable = { .method_call = request_call };

static void camera_call(GDBusConnection *bus, const gchar *sender, const gchar *path,
    const gchar *interface, const gchar *method, GVariant *parameters,
    GDBusMethodInvocation *invocation, gpointer data)
{
    (void)path; (void)interface; (void)data;
    if (strcmp(method, "AccessCamera") == 0) {
        if (strcmp(mode, "direct-empty") == 0) fatal("unnecessary portal request");
        GVariant *options;
        const char *token;
        g_variant_get(parameters, "(@a{sv})", &options);
        if (!g_variant_lookup(options, "handle_token", "&s", &token)) fatal("missing handle_token");
        char *escaped_sender = g_strdup(sender + 1);
        for (char *p = escaped_sender; *p; p++) if (*p == '.') *p = '_';
        char *handle = g_strdup_printf("/org/freedesktop/portal/desktop/request/%s/%s", escaped_sender, token);
        g_dbus_connection_register_object(bus, handle, request_info->interfaces[0],
            &request_vtable, NULL, NULL, NULL);
        if (strcmp(mode, "wait") != 0 && strcmp(mode, "vanish") != 0) {
            unsigned response = strstr(mode, "deny") != NULL ? 2 : strcmp(mode, "cancel") == 0 ? 1 : 0;
            // Deliberately emit before returning the handle to exercise the subscription race.
            g_dbus_connection_emit_signal(bus, sender, handle, "org.freedesktop.portal.Request",
                "Response", g_variant_new("(u@a{sv})", response,
                    g_variant_new_array(G_VARIANT_TYPE("{sv}"), NULL, 0)), NULL);
        }
        g_dbus_method_invocation_return_value(invocation, g_variant_new("(o)", handle));
        if (strcmp(mode, "vanish") == 0) {
            g_bus_unown_name(portal_owner);
            portal_owner = 0;
        }
        puts("REQUESTED"); fflush(stdout);
        g_free(handle); g_free(escaped_sender); g_variant_unref(options);
    } else if (strcmp(method, "OpenPipeWireRemote") == 0) {
        g_atomic_int_set(&next_portal_connection, 1);
        int descriptor = socket(AF_UNIX, SOCK_STREAM | SOCK_CLOEXEC, 0);
        struct sockaddr_un address = { .sun_family = AF_UNIX };
        snprintf(address.sun_path, sizeof(address.sun_path), "%s/flashcap-test", getenv("XDG_RUNTIME_DIR"));
        if (connect(descriptor, (struct sockaddr *)&address, sizeof(address)) < 0) fatal("connect portal socket");
        GUnixFDList *fds = g_unix_fd_list_new();
        // A filler descriptor makes the returned handle differ from both zero and the FD.
        int filler = dup(STDIN_FILENO);
        g_unix_fd_list_append(fds, filler, NULL);
        close(filler);
        int handle = g_unix_fd_list_append(fds, descriptor, NULL);
        close(descriptor);
        g_dbus_method_invocation_return_value_with_unix_fd_list(invocation, g_variant_new("(h)", handle), fds);
        g_object_unref(fds);
    }
}
static const GDBusInterfaceVTable camera_vtable = { .method_call = camera_call };

static void acquired(GDBusConnection *bus, const gchar *name, gpointer data)
{
    (void)bus; (void)name; (void)data;
    puts("READY"); fflush(stdout);
}

static gboolean quit(gpointer data)
{
    (void)data;
    g_main_loop_quit(main_loop);
    return G_SOURCE_REMOVE;
}

static gboolean input_ready(gint descriptor, GIOCondition condition, gpointer data)
{
    (void)condition;
    char value;
    (void)read(descriptor, &value, 1);
    return quit(data);
}

/** Runs an isolated, synthetic camera and portal for the managed integration tests. */
int main(int argc, char **argv)
{
    if (argc != 3) return 2;
    mode = argv[2];
    expose_direct = strncmp(mode, "direct", 6) == 0 || strncmp(mode, "partial", 7) == 0 || strcmp(mode, "readonly") == 0;
    start_camera(argv[1]);
    char direct_socket[sizeof(((struct sockaddr_un *)0)->sun_path)];
    snprintf(direct_socket, sizeof(direct_socket), "%s/pipewire-0", getenv("XDG_RUNTIME_DIR"));
    // A deliberately failed test may have exited before removing its alias.
    unlink(direct_socket);
    if (expose_direct && symlink("flashcap-test", direct_socket) < 0)
        fatal("expose direct socket");
    main_loop = g_main_loop_new(NULL, FALSE);
    GError *error = NULL;
    GDBusConnection *bus = g_bus_get_sync(G_BUS_TYPE_SESSION, NULL, &error);
    if (bus == NULL) fatal(error->message);
    GDBusNodeInfo *info = g_dbus_node_info_new_for_xml(
        "<node><interface name='org.freedesktop.portal.Camera'>"
        "<method name='AccessCamera'><arg type='a{sv}' direction='in'/><arg type='o' direction='out'/></method>"
        "<method name='OpenPipeWireRemote'><arg type='a{sv}' direction='in'/><arg type='h' direction='out'/></method>"
        "</interface></node>", NULL);
    request_info = g_dbus_node_info_new_for_xml(
        "<node><interface name='org.freedesktop.portal.Request'><method name='Close'/>"
        "<signal name='Response'><arg type='u'/><arg type='a{sv}'/></signal></interface></node>", NULL);
    g_dbus_connection_register_object(bus, "/org/freedesktop/portal/desktop", info->interfaces[0],
        &camera_vtable, NULL, NULL, &error);
    if (error != NULL) fatal(error->message);
    if (strcmp(mode, "absent") == 0 || strcmp(mode, "direct") == 0) {
        puts("READY"); fflush(stdout);
    } else {
        portal_owner = g_bus_own_name_on_connection(bus, "org.freedesktop.portal.Desktop",
            G_BUS_NAME_OWNER_FLAGS_NONE, acquired, NULL, NULL, NULL);
    }
    g_unix_signal_add(SIGTERM, quit, NULL);
    g_unix_fd_add(STDIN_FILENO, G_IO_IN | G_IO_HUP, input_ready, NULL);
    g_main_loop_run(main_loop);
    if (expose_direct) unlink(direct_socket);
    if (portal_owner != 0) g_bus_unown_name(portal_owner);
    g_object_unref(bus);
    g_dbus_node_info_unref(info);
    g_dbus_node_info_unref(request_info);
    g_main_loop_unref(main_loop);
    pw_thread_loop_stop(thread_loop);
    pw_loop_destroy_source(pw_thread_loop_get_loop(thread_loop), timer);
    for (size_t i = 0; i < link_count; i++) if (links[i] != NULL) pw_proxy_destroy(links[i]);
    if (extra_camera != NULL) pw_stream_destroy(extra_camera);
    pw_stream_destroy(camera);
    pw_proxy_destroy((struct pw_proxy *)registry);
    pw_core_disconnect(core);
    pw_context_destroy(context);
    pw_thread_loop_destroy(thread_loop);
    return 0;
}
