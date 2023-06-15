using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace FlashCap.Internal;

internal static class NativeMethods_AVFoundation
{
    public static class Dlfcn
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

    public static class LibSystem
    {
        public const string Path = "/usr/lib/libSystem.dylib";

        public static readonly IntPtr Handle = Dlfcn.OpenLibrary(Path, Dlfcn.Mode.None);

        public static readonly bool IsOnArm64;

        unsafe static LibSystem()
        {
            IsOnArm64 =
                IntPtr.Size == 8 &&
                NXGetLocalArchInfo()->GetName()?.StartsWith("arm64", StringComparison.OrdinalIgnoreCase) is true;
        }

        [DllImport(Path)]
        private static unsafe extern NXArchInfo* NXGetLocalArchInfo();

        private enum NXByteOrder
        {
            Unknown,
            LittleEndian,
            BigEndian,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NXArchInfo
        {
            public IntPtr Name;
            public int CpuType;
            public int CpuSubType;
            public NXByteOrder ByteOrder;
            public IntPtr Description;

            public string? GetName() => GetString(Name);

            public string? GetDescription() => GetString(Description);

            private static unsafe string? GetString(IntPtr pointer)
            {
                if (pointer == IntPtr.Zero)
                    return null;

                var start = (byte*)pointer;
                var end = (byte*)pointer;

                for (; *end != 0; end += 1) ;

                return Encoding.UTF8.GetString(start, new IntPtr(end - start).ToInt32());
            }
        }
    }

    public static class LibObjC
    {
        public const string Path = "/usr/lib/libobjc.dylib";

        public const string InitSelector = "init";
        public const string AllocSelector = "alloc";
        public const string ReleaseSelector = "release";

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public extern static void SendNoResult(IntPtr receiver, IntPtr selector);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public extern static void SendNoResult(IntPtr receiver, IntPtr selector, IntPtr arg1);

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

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public extern static LibCoreMedia.CMTime SendAndGetCMTime(IntPtr receiver, IntPtr selector);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public extern static void SendStret(out LibCoreMedia.CMTime result, IntPtr receiver, IntPtr selector);

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

            public static IntPtr Create(Delegate target, BlockLiteralCopy copy, BlockLiteralDispose dispose)
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

        public unsafe delegate void BlockLiteralCopy(BlockLiteral* dst, BlockLiteral* src);
        public unsafe delegate void BlockLiteralDispose(BlockLiteral* self);

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

            public static unsafe void Dispose(BlockLiteral* self) =>
                self->Dispose();

            public static unsafe void Copy(BlockLiteral* dst, BlockLiteral* src)
            {
                var context = GCHandle.FromIntPtr(src->ContextHandle);

                dst->ContextHandle = (IntPtr)GCHandle.Alloc(context);
                dst->Descriptor = src->Descriptor;
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
            private static readonly IntPtr NSConcreteStackBlock;
            private static readonly BlockLiteralCopy CopyHandler;
            private static readonly BlockLiteralDispose DisposeHandler;

            unsafe static BlockLiteralFactory()
            {
                NSConcreteStackBlock = Dlfcn.GetSymbol(LibSystem.Handle, "_NSConcreteStackBlock");

                CopyHandler = BlockLiteral.Copy;
                DisposeHandler = BlockLiteral.Dispose;
            }

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

    public static class LibCoreFoundation
    {
        public const string Path = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

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

    public static class LibCoreMedia
    {
        public const string Path = "/System/Library/Frameworks/CoreMedia.framework/CoreMedia";

        [DllImport(Path)]
        private extern static CMMediaType CMFormatDescriptionGetMediaType(IntPtr desc);

        [DllImport(Path)]
        private extern static uint CMFormatDescriptionGetMediaSubType(IntPtr desc);

        [DllImport(Path)]
        private extern static CMVideoDimensions CMVideoFormatDescriptionGetDimensions(IntPtr videoDesc);

        public enum CMMediaType : uint
        {
            Video = 1986618469, // 'vide'
        }

        public enum CMPixelFormat : uint
        {
            AlphaRedGreenBlue32bits = 32,
            BlueGreenRedAlpha32bits = 1111970369,
            RedGreenBlue24bits = 24,
            BigEndian555_16bits = 16,
            BigEndian565_16bits = 1110783541,
            LittleEndian555_16bits = 1278555445,
            LittleEndian565_16bits = 1278555701,
            LittleEndian5551_16bits = 892679473,
            YpCbCr422_8bits = 846624121,
            YpCbCr422yuvs_8bits = 2037741171,
            YpCbCr444_8bits = 1983066168,
            YpCbCrA4444_8bits = 1983131704,
            YpCbCr422_16bits = 1983000886,
            YpCbCr422_10bits = 1983000880,
            YpCbCr444_10bits = 1983131952,
            IndexedGrayWhiteIsZero_8bits = 40,
        }

        [StructLayout(LayoutKind.Sequential)]
        public partial struct CMTime
        {
            [Flags]
            public enum Flags : uint
            {
                Valid = 1,
                HasBeenRounded = 2,
                PositiveInfinity = 4,
                NegativeInfinity = 8,
                Indefinite = 16,
            }

            public long Value;
            public int TimeScale;
            public Flags TimeFlags;
            public long TimeEpoch;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CMVideoDimensions
        {
            public int Width;
            public int Height;
        }

        public sealed class CMFormatDescription : CFObject
        {
            public CMFormatDescription(IntPtr handle) :
                base(handle)
            { }

            public CMMediaType MediaType => CMFormatDescriptionGetMediaType(Handle);

            public CMVideoDimensions Dimensions => MediaType == CMMediaType.Video
                ? CMVideoFormatDescriptionGetDimensions(Handle)
                : default;
        }
    }

    internal static class LibAVFoundation
    {
        public const string Path = "/System/Library/Frameworks/AVFoundation.framework/AVFoundation";
        
        public static readonly IntPtr Handle = Dlfcn.OpenLibrary(Path, Dlfcn.Mode.None);
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

    internal abstract class NativeObject : IDisposable
    {
        ~NativeObject()
        {
            Dispose(false);
        }

        public IntPtr Handle { get; protected set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }

    internal abstract class CFObject : NativeObject
    {
        protected CFObject(IntPtr handle) => Handle = handle;

        protected override void Dispose(bool disposing)
        {
            if (Handle == IntPtr.Zero)
                return;

            LibCoreFoundation.CFRelease(Handle);

            Handle = IntPtr.Zero;
        }
    }

    internal abstract class NSObject : NativeObject
    {
        protected NSObject(IntPtr handle) => Handle = handle;

        protected override void Dispose(bool disposing)
        {
            if (Handle == IntPtr.Zero)
                return;

            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector(LibObjC.ReleaseSelector));

            Handle = IntPtr.Zero;
        }
    }

    internal sealed class NSError : NSObject
    {
        public NSError(IntPtr handle) :
            base(handle)
        { }
    }

    internal delegate void AVRequestAccessStatus(bool accessGranted);

    internal sealed class AVFrameRateRange : NSObject
    {
        public AVFrameRateRange(IntPtr handle) :
            base(handle)
        { }

        public LibCoreMedia.CMTime MaxFrameDuration
        {
            get
            {
                if (LibSystem.IsOnArm64)
                {
                    LibObjC.SendStret(out var result, Handle, LibObjC.GetSelector("maxFrameDuration"));

                    return result;
                }

                return LibObjC.SendAndGetCMTime(Handle, LibObjC.GetSelector("maxFrameDuration"));
            }
        }

        public LibCoreMedia.CMTime MinFrameDuration
        {
            get
            {
                if (LibSystem.IsOnArm64)
                {
                    LibObjC.SendStret(out var result, Handle, LibObjC.GetSelector("minFrameDuration"));

                    return result;
                }

                return LibObjC.SendAndGetCMTime(Handle, LibObjC.GetSelector("minFrameDuration"));
            }
        }

    }

    internal sealed class AVCaptureDevice : NSObject
    {
        private AVCaptureDevice(IntPtr handle) :
            base(handle)
        { }

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
        public AVCaptureDeviceFormat(IntPtr handle) :
            base(handle)
        { }

        public LibCoreMedia.CMFormatDescription FormatDescription =>
            new LibCoreMedia.CMFormatDescription(
                LibObjC.SendAndGetHandle(
                    Handle,
                    LibObjC.GetSelector("formatDescription")));

        public AVFrameRateRange VideoSupportedFrameRateRanges =>
            new AVFrameRateRange(
                LibObjC.SendAndGetHandle(
                    Handle,
                    LibObjC.GetSelector("videoSupportedFrameRateRanges")));
    }

    internal abstract class AVCaptureInput : NSObject
    {
        protected AVCaptureInput(IntPtr handle) :
            base(handle)
        { }
    }

    internal abstract class AVCaptureOutput : NSObject
    {
        protected AVCaptureOutput(IntPtr handle) :
            base(handle)
        { }
    }

    internal sealed class AVCaptureDeviceInput : AVCaptureInput
    {
        public AVCaptureDeviceInput(AVCaptureDevice device) :
            base(FromDevice(device))
        { }

        private static unsafe IntPtr FromDevice(AVCaptureDevice device)
        {
            var errorHandle = default(IntPtr);
            var inputHandle = LibObjC.SendAndGetHandle(
                LibObjC.GetClass(nameof(AVCaptureDeviceInput)),
                LibObjC.GetSelector("deviceInputWithDevice:error:"),
                device.Handle,
                new IntPtr(&errorHandle));

            if (errorHandle != IntPtr.Zero)
            {
                using var error = new NSError(errorHandle);
                throw new InvalidOperationException(/* error.FailureReason */);
            }

            return inputHandle;
        }
    }

    internal sealed class AVCaptureDeviceOutput : AVCaptureOutput
    {
        public AVCaptureDeviceOutput() : base(
            LibObjC.SendAndGetHandle(
                LibObjC.GetClass(nameof(AVCaptureDeviceOutput)),
                LibObjC.GetSelector(LibObjC.AllocSelector)))
        {
            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("init"));
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

        public void AddInput(AVCaptureInput input) =>
            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("addInput:"),
                input.Handle);

        public void AddOutput(AVCaptureOutput output) =>
            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("addOutput:"),
                output.Handle);

		public void StartRunning() =>
            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("startRunning"));

		public void StopRunning() =>
            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("stopRunning"));
    }

    internal enum AVAuthorizationStatus : long
    {
        NotDetermined,
        Restricted,
        Denied,
        Authorized
    }
}
