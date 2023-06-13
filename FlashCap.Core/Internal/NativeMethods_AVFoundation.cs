using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace FlashCap.Internal;

public static class NativeMethods_AVFoundation
{
    private static class Dlfcn
    {
        [DllImport(LibSystem.Path, EntryPoint = "dlopen")]
        public static extern IntPtr OpenLibrary(string path, Mode mode);

        [DllImport(LibSystem.Path, EntryPoint = "dlsym")]
        public static extern IntPtr GetSymbol(IntPtr handle, string symbol);

        [Flags]
        public enum Mode : int
        {
            None = 0x0,
        }
    }

    private static class LibSystem
    {
        public const string Path = "/usr/lib/libSystem.dylib";
        public static readonly IntPtr Handle = Dlfcn.OpenLibrary(Path, Dlfcn.Mode.None);
    }

    private static class LibObjC
    {
        private const string Path = "/usr/lib/libobjc.dylib";

        public const string InitSelector = "init";
        public const string AllocSelector = "alloc";
        public const string ReleaseSelector = "release";

        public static void Release(IntPtr handle)
        {
            SendNoResult(handle, GetSelector(ReleaseSelector));
        }

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public extern static void SendNoResult(IntPtr receiver, IntPtr selector);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public extern static void SendNoResult(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public extern static IntPtr SendAndGetHandle(IntPtr receiver, IntPtr selector);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public extern static IntPtr SendAndGetHandle(IntPtr receiver, IntPtr selector, IntPtr arg1);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public extern static IntPtr SendAndGetHandle(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public extern static IntPtr SendAndGetHandle(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2, long arg3);

        [DllImport(Path, EntryPoint = "objc_getClass")]
        public static extern IntPtr GetClass(string name);

        [DllImport(Path, EntryPoint = "sel_registerName")]
        public extern static IntPtr GetSelector(string name);

        [Flags]
        public enum BlockFlags
        {
            BLOCK_REFCOUNT_MASK = 0xffff,
            BLOCK_NEEDS_FREE = 1 << 24,
            BLOCK_HAS_COPY_DISPOSE = 1 << 25,
            BLOCK_HAS_CTOR = 1 << 26,
            BLOCK_IS_GC = 1 << 27,
            BLOCK_IS_GLOBAL = 1 << 28,
            BLOCK_HAS_STRET = 1 << 29,
            BLOCK_HAS_SIGNATURE = 1 << 30,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BlockDescriptor
        {
            public IntPtr Reserved;
            public IntPtr Size;
            public IntPtr Copy;
            public IntPtr Dispose;
            public IntPtr Signature;

            public static IntPtr Create(Delegate target, Action<IntPtr, IntPtr> copy, Action<IntPtr> dispose)
            {
                var signatureString = GetSignature(target.Method);
                var signatureBytes = Encoding.UTF8.GetBytes(signatureString);
                var descriptorSize = Marshal.SizeOf<BlockDescriptor>();

                var memory = Marshal.AllocHGlobal(descriptorSize + signatureBytes.Length);
                var memoryToSignature = memory + descriptorSize;

                Marshal.Copy(signatureBytes, startIndex: 0, memoryToSignature, length: 0);

                unsafe
                {
                    var descriptor = (BlockDescriptor*)memory;

                    descriptor->Size = new IntPtr(Marshal.SizeOf<BlockLiteral>());
                    descriptor->Copy = Marshal.GetFunctionPointerForDelegate(copy);
                    descriptor->Dispose = Marshal.GetFunctionPointerForDelegate(dispose);
                    descriptor->Signature = memoryToSignature;
                }

                return memory;
            }

            private static string GetSignature(MethodInfo method)
            {
                var signature = new StringBuilder()
                    .Append(GetSignature(method.ReturnType))
                    .Append("@?");

                foreach (var parameter in method.GetParameters())
                {
                    signature.Append(GetSignature(parameter.ParameterType));
                }

                return signature.ToString();
            }

            private static string GetSignature(Type type)
            {
                type = Nullable.GetUnderlyingType(type) ?? type;
                return type.FullName switch
                {
                    "System.Void" => "v",
                    "System.String" => "@",
                    "System.IntPtr" => "^v",
                    "System.SByte" => "c",
                    "System.Byte" => "C",
                    "System.Char" => "s",
                    "System.Int16" => "s",
                    "System.UInt16" => "S",
                    "System.Int32" => "i",
                    "System.UInt32" => "I",
                    "System.Int64" => "q",
                    "System.UInt64" => "Q",
                    "System.Single" => "f",
                    "System.Double" => "d",
                    "System.Boolean" => IntPtr.Size == 64 ? "B" : "c",
                    _ => typeof(NSObject).IsAssignableFrom(type)
                        ? "@" : throw new NotSupportedException($"Type '{type}' is not supported for interop.")
                };
            }
        }

        // https://clang.llvm.org/docs/Block-ABI-Apple.html
        public unsafe struct BlockLiteral : IDisposable
        {
            public IntPtr Isa;
            public BlockFlags Flags;
            public int Reserved;
            public IntPtr Invoke;
            public IntPtr Descriptor;
            public IntPtr ContextHandle;

            public void Dispose()
            {
                if (ContextHandle != IntPtr.Zero)
                {
                    GCHandle
                        .FromIntPtr(ContextHandle)
                        .Free();

                    ContextHandle = IntPtr.Zero;
                }
            }

            public static TDelegate GetTarget<TDelegate>(IntPtr block)
                where TDelegate : class
            {
                var self = (BlockLiteral*)block;
                var target = GCHandle.FromIntPtr(self->ContextHandle).Target;

                return (TDelegate)target!;
            }
        }

        public sealed class BlockLiteralFactory
        {
            private static readonly IntPtr NSConcreteStackBlock = Dlfcn.GetSymbol(LibSystem.Handle, "_NSConcreteStackBlock");

            private static readonly Action<IntPtr, IntPtr> CopyHandler = delegate(IntPtr dst, IntPtr src)
            {
                unsafe
                {
                    var dstLiteral = (BlockLiteral*)dst;
                    var srcLiteral = (BlockLiteral*)src;
                    var context = GCHandle.FromIntPtr(srcLiteral->ContextHandle);

                    dstLiteral->ContextHandle = (IntPtr)GCHandle.Alloc(context);
                    dstLiteral->Descriptor = srcLiteral->Descriptor;
                }
            };
            
            private static readonly Action<IntPtr> DisposeHandler = delegate(IntPtr self)
            {
                unsafe
                {
                    var literal = (BlockLiteral*)self;

                    GCHandle
                        .FromIntPtr(literal->ContextHandle)
                        .Free();

                    literal->ContextHandle = IntPtr.Zero;
                    literal->Descriptor = IntPtr.Zero;
                }
            };

            private readonly IntPtr _trampoline;
            private readonly IntPtr _descriptor;

            private BlockLiteralFactory(IntPtr trampoline, IntPtr descriptor)
            {
                _trampoline = trampoline;
                _descriptor = descriptor;
            }
            
            public static BlockLiteralFactory CreateFactory<T>(T trampoline)
                where T : Delegate
            {
                var descriptor = DescriptorCache<T>.Instance;
                if (descriptor == IntPtr.Zero)
                {
                    descriptor = BlockDescriptor.Create(trampoline, CopyHandler, DisposeHandler);

                    var descriptorCached = Interlocked.CompareExchange(ref DescriptorCache<T>.Instance, descriptor, default);
                    if (descriptorCached != default)
                    {
                        Marshal.FreeHGlobal(descriptor);

                        descriptor = descriptorCached;
                    }
                }

                return new BlockLiteralFactory(Marshal.GetFunctionPointerForDelegate(trampoline), descriptor);
            }

            public BlockLiteral CreateLiteral(object? context)
            {
                return new BlockLiteral
                {
                    Isa = NSConcreteStackBlock,
                    Flags = BlockFlags.BLOCK_HAS_COPY_DISPOSE | BlockFlags.BLOCK_HAS_SIGNATURE,
                    Invoke = _trampoline,
                    Descriptor = _descriptor,
                    ContextHandle = (IntPtr)GCHandle.Alloc(context),
                };
            }

            private static class DescriptorCache<T>
                where T : Delegate
            {
                public static IntPtr Instance;
            }
        }
    }

    private static class LibCoreFoundation
    {
        private const string Path = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

        [DllImport(Path)]
        public extern static void CFRelease(IntPtr cf);

        [DllImport(Path)]
        public extern static IntPtr CFArrayGetCount(IntPtr theArray);

        [DllImport(Path)]
        public extern static void CFArrayGetValues(IntPtr theArray, CFRange range, IntPtr values);

        [DllImport(Path)]
        public extern static IntPtr CFStringGetLength(IntPtr theString);

        [DllImport(Path)]
        public extern static unsafe char* CFStringGetCharactersPtr(IntPtr theString);

        [DllImport(Path)]
        public extern static unsafe void CFStringGetCharacters(IntPtr theString, CFRange range, char* buffer);

        [DllImport(Path)]
        public extern static unsafe IntPtr CFStringCreateWithCharacters(IntPtr allocator, char* str, nint count);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CFRange
    {
        public nint Location;
        public nint Length;

        public CFRange(nint location, nint length)
        {
            Location = new IntPtr(location);
            Length = new IntPtr(length);
        }
    }

    // https://github.com/opensource-apple/CF/blob/master/CFArray.c
    internal static class CFArray
    {
        public static unsafe T[] ToArray<T>(IntPtr handle, Func<IntPtr, T> constructor)
        {
            var count = LibCoreFoundation.CFArrayGetCount(handle).ToInt32();
            if (count == 0)
                return Array.Empty<T>();

            var buffer = new IntPtr[count];

            unsafe
            {
                fixed (void* ptr = buffer)
                    LibCoreFoundation.CFArrayGetValues(handle, new CFRange(location: 0, count), new IntPtr(ptr));
            }

            var array = new T[count];
            for (var i = 0; i < count; i++)
            {
                array[i] = constructor(buffer[i]);
            }

            return array;
        }
    }

    // https://github.com/opensource-apple/CF/blob/master/CFString.c
    internal readonly struct CFString : IDisposable
    {
        public readonly IntPtr Handle;

        public CFString(IntPtr handle) => Handle = handle;

        public void Dispose() => LibCoreFoundation.CFRelease(Handle);

        public override unsafe string? ToString()
        {
            if (Handle == IntPtr.Zero)
                return null;

            int length = LibCoreFoundation.CFStringGetLength(Handle).ToInt32();
            if (length == 0)
                return string.Empty;

            var chars = LibCoreFoundation.CFStringGetCharactersPtr(Handle);
            var memoryAllocated = false;

            if (chars is null)
            {
                var heapAllocated = length > 128;
                if (heapAllocated)
                    chars = (char*)Marshal.AllocHGlobal(length * 2);
                else
                {
                    var buffer = stackalloc char[length];
                    chars = buffer;
                }

                LibCoreFoundation.CFStringGetCharacters(Handle, new CFRange(0, length), chars);
            }

            var result = new string(chars, 0, length);

            if (memoryAllocated)
                Marshal.FreeHGlobal(new IntPtr(chars));

            return result;
        }

        public static unsafe CFString Create(string? value)
        {
            if (value is null)
                return default;

            fixed (char* chars = value)
            {
                return new(LibCoreFoundation.CFStringCreateWithCharacters(
                    allocator: IntPtr.Zero, chars, value.Length));
            }
        }
    }

    internal abstract class NSObject : IDisposable
    {
        protected NSObject(IntPtr handle)
        {
            Handle = handle;
        }

        ~NSObject()
        {
            Dispose(false);
        }

        public IntPtr Handle { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Handle == IntPtr.Zero)
                return;

            LibObjC.Release(Handle);
            Handle = IntPtr.Zero;
        }
    }

    internal delegate void AVRequestAccessStatus(bool accessGranted);

    internal sealed class AVCaptureDevice : NSObject
    {
        private AVCaptureDevice(IntPtr handle) : base(handle)
        {
        }

        public string UniqueID
        {
            get
            {
                using var result = new CFString(
                    LibObjC.SendAndGetHandle(
                        Handle,
                        LibObjC.GetSelector("uniqueID:")));

                return result.ToString() ?? throw new InvalidOperationException();
            }
        }

        public string ModelID
        {
            get
            {
                using var result = new CFString(
                    LibObjC.SendAndGetHandle(
                        Handle,
                        LibObjC.GetSelector("modelID:")));

                return result.ToString() ?? throw new InvalidOperationException();
            }
        }

        public string LocalizedName
        {
            get
            {
                using var result = new CFString(
                    LibObjC.SendAndGetHandle(
                        Handle,
                        LibObjC.GetSelector("localizedName:")));

                return result.ToString() ?? throw new InvalidOperationException();
            }

        }

        public AVCaptureDeviceFormat[] Formats
        {
            get
            {
                var handle = LibObjC.SendAndGetHandle(
                    Handle,
                    LibObjC.GetSelector("formats"));

                return CFArray.ToArray(handle, static handle => new AVCaptureDeviceFormat(handle));
            }
        }

        public static AVCaptureDevice? DeviceWithUniqueID(string deviceUniqueID)
        {
            using var nativeDeviceUniqueID = CFString.Create(deviceUniqueID);
            var handle = LibObjC.SendAndGetHandle(
                LibObjC.GetClass(nameof(AVCaptureDevice)),
                LibObjC.GetSelector("deviceWithUniqueID:"),
                nativeDeviceUniqueID.Handle);

            return handle == IntPtr.Zero ? null : new AVCaptureDevice(handle);
        }

        public static AVCaptureDevice? GetDefaultDevice(string mediaType)
        {
            using var nativeMediaType = CFString.Create(mediaType);
            var handle = LibObjC.SendAndGetHandle(
                LibObjC.GetClass(nameof(AVCaptureDevice)),
                LibObjC.GetSelector("defaultDeviceWithMediaType:"),
                nativeMediaType.Handle);

            return handle == IntPtr.Zero ? null : new AVCaptureDevice(handle);
        }

        public static AVAuthorizationStatus GetAuthorizationStatus(string mediaType)
        {
            using var nativeMediaType = CFString.Create(mediaType);
            return (AVAuthorizationStatus)(long)
                LibObjC.SendAndGetHandle(
                    LibObjC.GetClass(nameof(AVCaptureDevice)),
                    LibObjC.GetSelector("authorizationStatusForMediaType:"),
                    nativeMediaType.Handle);
        }

        private static LibObjC.BlockLiteralFactory? RequestAccessForMediaTypeBlockFactory;

        public static unsafe void RequestAccessForMediaType(string mediaType, AVRequestAccessStatus completion)
        {
            RequestAccessForMediaTypeBlockFactory ??= LibObjC.BlockLiteralFactory.CreateFactory(
                delegate (IntPtr block, byte accessGranted)
                {
                    LibObjC.BlockLiteral
                        .GetTarget<AVRequestAccessStatus>(block)
                        .Invoke(accessGranted != 0);
                });

            using var blockLiteral = RequestAccessForMediaTypeBlockFactory.CreateLiteral(completion);
            using var nativeMediaType = CFString.Create(mediaType);

            LibObjC.SendNoResult(
                LibObjC.GetClass(nameof(AVCaptureDevice)),
                LibObjC.GetSelector("requestAccessForMediaType:completionHandler:"),
                nativeMediaType.Handle,
                new IntPtr(&blockLiteral));
        }
    }

    internal sealed class AVCaptureDeviceFormat : NSObject
    {
        public AVCaptureDeviceFormat(IntPtr handle) : base(handle)
        {
        }
    }

    internal sealed class AVCaptureDeviceInput : NSObject
    {
        public AVCaptureDeviceInput(IntPtr handle) : base(handle)
        {
        }
    }

    internal sealed class AVCaptureSession : NSObject
    {
        public AVCaptureSession() : base(
            LibObjC.SendAndGetHandle(
                LibObjC.GetClass(nameof(AVCaptureSession)),
                LibObjC.GetSelector(LibObjC.AllocSelector)))
        {
            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("init"));
        }
    }

    internal enum AVAuthorizationStatus : long
    {
        NotDetermined,
        Restricted,
        Denied,
        Authorized
    }
}
