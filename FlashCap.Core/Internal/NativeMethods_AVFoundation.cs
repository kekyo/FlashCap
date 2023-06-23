using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace FlashCap.Internal;

internal static class NativeMethods_AVFoundation
{
    public static readonly Dictionary<PixelFormats, int> PixelFormatMap = new()
    {
        [PixelFormats.RGB16] = LibCoreVideo.PixelFormatType_24RGB,
        [PixelFormats.RGB32] = LibCoreVideo.PixelFormatType_30RGB,
        [PixelFormats.ARGB32] = LibCoreVideo.PixelFormatType_32ARGB,
        [PixelFormats.YUYV] = LibCoreVideo.PixelFormatType_422YpCbCr8,
    };

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

    public static class LibC
    {
        public const string Path = "/usr/lib/libc.dylib";
        
        [DllImport(Path, EntryPoint = "dispatch_get_global_queue")]
		public extern static IntPtr GetGlobalQueue(IntPtr identifier, IntPtr flags);

        public static class DispatchQualityOfService
        {
    		public const nint UserInteractive = 0x21;
    		public const nint UserInitiated = 0x19;
    		public const nint Default = 0x15;
    		public const nint Utility = 0x11;
    		public const nint Background = 0x09;
    		public const nint Unspecified = 0x00;
    	}
    }

    public static class LibObjC
    {
        public const string Path = "/usr/lib/libobjc.dylib";

        public const string InitSelector = "init";
        public const string AllocSelector = "alloc";
        public const string ReleaseSelector = "release";

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern void SendNoResult(IntPtr receiver, IntPtr selector);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern void SendNoResult(IntPtr receiver, IntPtr selector, IntPtr arg1);
        
        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern void SendNoResult(IntPtr receiver, IntPtr selector, LibCoreMedia.CMTime arg1);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public extern static void SendNoResult(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern IntPtr SendAndGetHandle(IntPtr receiver, IntPtr selector);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern IntPtr SendAndGetHandle(IntPtr receiver, IntPtr selector, IntPtr arg1);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern IntPtr SendAndGetHandle(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern IntPtr SendAndGetHandle(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2, long arg3);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern LibCoreMedia.CMTime SendAndGetCMTime(IntPtr receiver, IntPtr selector);

        [DllImport(Path, EntryPoint = "objc_msgSend_stret")]
        public static extern void SendAndGetCMTimeStret(out LibCoreMedia.CMTime result, IntPtr receiver, IntPtr selector);

        [DllImport(Path, EntryPoint = "objc_getClass")]
        public static extern IntPtr GetClass(string name);

        [DllImport(Path, EntryPoint = "object_getIvar")]
        public static extern IntPtr GetVariable(IntPtr obj, IntPtr ivar);

        [DllImport(Path, EntryPoint = "object_setIvar")]
        public static extern void SetVariable(IntPtr obj, IntPtr ivar, IntPtr value);

        [DllImport(Path, EntryPoint = "objc_allocateClassPair")]
        public static extern IntPtr AllocateClass(IntPtr superclass, string name, IntPtr extraBytes);

        [DllImport(Path, EntryPoint = "objc_registerClassPair")]
        public static extern void RegisterClass(IntPtr cls);

        [DllImport(Path, EntryPoint = "objc_getProtocol")]
        public static extern IntPtr GetProtocol(string name);

        [DllImport(Path, EntryPoint = "sel_registerName")]
        public static extern IntPtr GetSelector(string name);

        [DllImport(Path, EntryPoint = "class_addMethod")]
        [return: MarshalAs(UnmanagedType.U1)]
		public static extern bool AddMethod(IntPtr cls, IntPtr name, IntPtr imp, string types);

        [DllImport(Path, EntryPoint = "class_addIvar")]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern void AddVariable(IntPtr cls, string name, IntPtr size, byte alignment, string types);
        
        [DllImport(Path, EntryPoint = "class_addProtocol")]
        [return: MarshalAs(UnmanagedType.U1)]
		public static extern bool AddProtocol(IntPtr cls, IntPtr protocol);

        [DllImport(Path, EntryPoint = "class_getInstanceVariable")]
        public static extern IntPtr GetVariable(IntPtr cls, string name);

        // https://developer.apple.com/library/archive/documentation/Cocoa/Conceptual/ObjCRuntimeGuide/Articles/ocrtTypeEncodings.html#//apple_ref/doc/uid/TP40008048-CH100
        public static string GetSignature(MethodInfo method, bool blockLiteral)
        {
            var signature = new StringBuilder()
                .Append(GetSignature(method.ReturnType))
                .Append(blockLiteral ? "@?" : "@:");

            var parameters = blockLiteral
                ? method.GetParameters()
                : method.GetParameters().Skip(1);

            foreach (var parameter in parameters)
            {
                signature.Append(GetSignature(parameter.ParameterType));
            }

            return signature.ToString();
        }

        public static string GetSignature(Type type)
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
                var signatureString = GetSignature(target.Method, blockLiteral: true);
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

        public abstract class NSObject : NativeObject
        {
            protected NSObject(IntPtr handle, bool retain)
            {
                Handle = handle;

                if (retain)
                    LibCoreFoundation.CFRetain(handle);
            }

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

        public sealed class NSError : NSObject
        {
            public NSError(IntPtr handle, bool retain) :
                base(handle, retain)
            { }
        }
    }

    public static class LibCoreFoundation
    {
        public const string Path = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

        public static readonly IntPtr Handle = Dlfcn.OpenLibrary(Path, Dlfcn.Mode.None);
        public static readonly IntPtr kCFTypeArrayCallbacks = Dlfcn.GetSymbol(Handle, "kCFTypeArrayCallBacks");
        public static readonly IntPtr kCFCopyStringDictionaryKeyCallBacks = Dlfcn.GetSymbol(Handle, "kCFCopyStringDictionaryKeyCallBacks");
        public static readonly IntPtr kCFTypeDictionaryValueCallBacks = Dlfcn.GetSymbol(Handle, "kCFTypeDictionaryValueCallBacks");

        [DllImport(Path)]
        public static extern void CFRelease(IntPtr cf);

        [DllImport(Path)]
        public static extern void CFRetain(IntPtr cf);
        
        [DllImport(Path)]
		public static extern IntPtr CFArrayCreate(IntPtr allocator, IntPtr values, nint numValues, IntPtr callBacks);

        [DllImport(Path)]
        public static extern IntPtr CFArrayGetCount(IntPtr theArray);

        [DllImport(Path)]
        public static extern void CFArrayGetValues(IntPtr theArray, CFRange range, IntPtr values);

        [DllImport(Path)]
        public static extern IntPtr CFStringGetLength(IntPtr theString);

        [DllImport(Path)]
        public static extern unsafe char* CFStringGetCharactersPtr(IntPtr theString);

        [DllImport(Path)]
        public static extern unsafe void CFStringGetCharacters(IntPtr theString, CFRange range, char* buffer);

        [DllImport(Path)]
        public static extern unsafe IntPtr CFStringCreateWithCharacters(IntPtr allocator, char* str, nint count);

        [DllImport(Path)]
        public static extern unsafe IntPtr CFNumberCreate(IntPtr allocator, CFNumberType theType, void* valuePtr);

        [DllImport(Path)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern unsafe bool CFNumberGetValue(IntPtr number, CFNumberType theType, void* valuePtr);

        [DllImport(Path)]
        public static extern unsafe IntPtr CFDictionaryCreate(IntPtr allocator, void* keys, void* values, nint numValues, IntPtr keyCallBacks, IntPtr valueCallBacks);

        [StructLayout(LayoutKind.Sequential)]
        public struct CFRange
        {
            public nint Location;
            public nint Length;

            public CFRange(nint location, nint length)
            {
                Location = new IntPtr(location);
                Length = new IntPtr(length);
            }
        }

        public abstract class CFObject : NativeObject
        {
            protected CFObject(IntPtr handle) => Handle = handle;

            protected override void Dispose(bool disposing)
            {
                if (Handle == IntPtr.Zero)
                    return;

                CFRelease(Handle);

                Handle = IntPtr.Zero;
            }
        }

        // https://github.com/opensource-apple/CF/blob/master/CFArray.c
        public sealed class CFArray : CFObject
        {
            public CFArray(IntPtr handle) :
                base(handle)
            { }

            public static unsafe CFArray Create(IntPtr[] items)
            {
    			fixed (IntPtr* pointer = items)
                {
				    return new CFArray(CFArrayCreate(IntPtr.Zero, new IntPtr(pointer), items.Length, kCFTypeArrayCallbacks));
                }
            }
            
            public static unsafe T[] ToArray<T>(IntPtr handle, Func<IntPtr, T> constructor)
            {
                var count = CFArrayGetCount(handle).ToInt32();
                if (count == 0)
                    return Array.Empty<T>();

                var buffer = new IntPtr[count];

                unsafe
                {
                    fixed (void* ptr = buffer)
                        CFArrayGetValues(handle, new CFRange(location: 0, count), new IntPtr(ptr));
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
        public sealed class CFString : CFObject
        {
            public CFString(IntPtr handle) :
                base(handle)
            { }

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

            public static unsafe CFString Create(string value)
            {
                fixed (char* chars = value)
                {
                    return new(LibCoreFoundation.CFStringCreateWithCharacters(
                        allocator: IntPtr.Zero, chars, value.Length));
                }
            }
        }

        public enum CFNumberType
        {
            sInt32Type = 3
        }
    }

    public static class LibCoreMedia
    {
        public const string Path = "/System/Library/Frameworks/CoreMedia.framework/CoreMedia";

        [DllImport(Path)]
        public static extern CMTime CMTimeMake(long value, int timescale);

        [DllImport(Path)]
        public static extern double CMTimeGetSeconds(CMTime time);

        [DllImport(Path)]
        public static extern IntPtr CMSampleBufferGetImageBuffer(IntPtr sbuf);

        [DllImport(Path)]
        public static extern CMTime CMSampleBufferGetDecodeTimeStamp(IntPtr sbuf);

        [DllImport(Path)]
        public static extern CMMediaType CMFormatDescriptionGetMediaType(IntPtr desc);

        [DllImport(Path)]
        public static extern uint CMFormatDescriptionGetMediaSubType(IntPtr desc);

        [DllImport(Path)]
        public static extern CMVideoDimensions CMVideoFormatDescriptionGetDimensions(IntPtr videoDesc);

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

        public sealed class CMFormatDescription : LibCoreFoundation.CFObject
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

    public static class LibCoreVideo
    {
        public const string Path = "/System/Library/Frameworks/CoreMedia.framework/CoreMedia";

        public static readonly IntPtr Handle = Dlfcn.OpenLibrary(Path, Dlfcn.Mode.None);
        public static readonly IntPtr kCVPixelBufferPixelFormatTypeKey = Dlfcn.GetSymbol(Handle, "kCVPixelBufferPixelFormatTypeKey");

        [DllImport(Path)]
        public static extern nuint CVPixelBufferGetDataSize(IntPtr pixelBuffer);

        [DllImport(Path)]
        public static extern nuint CVPixelBufferGetPlaneCount(IntPtr pixelBuffer);

        [DllImport(Path)]
        public static extern IntPtr CVPixelBufferGetBaseAddress(IntPtr pixelBuffer);

        [DllImport(Path)]
        public static extern int CVPixelBufferLockBaseAddress(IntPtr pixelBuffer, PixelBufferLockFlags lockFlags);

        [DllImport(Path)]
        public static extern int CVPixelBufferUnlockBaseAddress(IntPtr pixelBuffer, PixelBufferLockFlags lockFlags);

        public enum PixelBufferLockFlags : long
        {
            None,
            ReadOnly = 1,
        }

        public static readonly int PixelFormatType_16BE555 = 0x00000010;
        public static readonly int PixelFormatType_16LE555 = GetFourCC("L555");
        public static readonly int PixelFormatType_16LE5551 = GetFourCC("5551");
        public static readonly int PixelFormatType_16BE565 = GetFourCC("B565");
        public static readonly int PixelFormatType_16LE565 = GetFourCC("L565");
        public static readonly int PixelFormatType_24RGB = 0x00000018;
        public static readonly int PixelFormatType_24BGR = GetFourCC("24BG");
        public static readonly int PixelFormatType_32ARGB = 0x00000020;
        public static readonly int PixelFormatType_32ABGR = GetFourCC("ABGR");
        public static readonly int PixelFormatType_32RGBA = GetFourCC("RGBA");
        public static readonly int PixelFormatType_64ARGB = GetFourCC("b64a");
        public static readonly int PixelFormatType_48RGB = GetFourCC("b48r");
        public static readonly int PixelFormatType_32AlphaGray = GetFourCC("b32a");
        public static readonly int PixelFormatType_16Gray = GetFourCC("b16g");
        public static readonly int PixelFormatType_30RGB = GetFourCC("R10k");
        public static readonly int PixelFormatType_422YpCbCr8 = GetFourCC("2vuy");
        public static readonly int PixelFormatType_4444YpCbCrA8 = GetFourCC("v408");
        public static readonly int PixelFormatType_4444YpCbCrA8R = GetFourCC("r408");
        public static readonly int PixelFormatType_4444AYpCbCr8 = GetFourCC("y408");
        public static readonly int PixelFormatType_4444AYpCbCr16 = GetFourCC("y416");
        public static readonly int PixelFormatType_444YpCbCr8 = GetFourCC("v308");
        public static readonly int PixelFormatType_422YpCbCr16 = GetFourCC("v216");
        public static readonly int PixelFormatType_422YpCbCr10 = GetFourCC("v210");
        public static readonly int PixelFormatType_444YpCbCr10 = GetFourCC("v410");
        public static readonly int PixelFormatType_422YpCbCr8_yuvs = GetFourCC("yuvs");
        public static readonly int PixelFormatType_422YpCbCr8FullRange = GetFourCC("yuvf");

        public static unsafe string GetFourCCName(int value)
        {
            var buffer = stackalloc char[4];

            buffer[0] = (char)(byte)(value >> 24);
            buffer[1] = (char)(byte)(value >> 16);
            buffer[2] = (char)(byte)(value >> 8);
            buffer[3] = (char)(byte)value;

            return new string(buffer);
        }

        private static int GetFourCC(string s) =>
            s[0] << 24 |
            s[1] << 16 |
            s[2] << 8 |
            s[3];
    }

    public static class LibAVFoundation
    {
        public const string Path = "/System/Library/Frameworks/AVFoundation.framework/AVFoundation";
        
        public static readonly IntPtr Handle = Dlfcn.OpenLibrary(Path, Dlfcn.Mode.None);

        public delegate void AVRequestAccessStatus(bool accessGranted);

        public sealed class AVFrameRateRange : LibObjC.NSObject
        {
            public AVFrameRateRange(IntPtr handle, bool retain) :
                base(handle, retain)
            { }

            public LibCoreMedia.CMTime MaxFrameDuration
            {
                get
                {
                    if (LibSystem.IsOnArm64)
                    {
                        LibObjC.SendAndGetCMTimeStret(out var result, Handle, LibObjC.GetSelector("maxFrameDuration"));

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
                        LibObjC.SendAndGetCMTimeStret(out var result, Handle, LibObjC.GetSelector("minFrameDuration"));

                        return result;
                    }

                    return LibObjC.SendAndGetCMTime(Handle, LibObjC.GetSelector("minFrameDuration"));
                }
            }

        }

        public sealed class AVCaptureDevice : LibObjC.NSObject
        {
            public AVCaptureDevice(IntPtr handle, bool retain) :
                base(handle, retain)
            { }

            public string UniqueID
            {
                get
                {
                    using var result = new LibCoreFoundation.CFString(
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
                    using var result = new LibCoreFoundation.CFString(
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
                    using var result = new LibCoreFoundation.CFString(
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

                    return LibCoreFoundation.CFArray.ToArray(handle, static handle => new AVCaptureDeviceFormat(handle, retain: true));
                }
            }

            public AVCaptureDeviceFormat ActiveFormat
            {
                get => new AVCaptureDeviceFormat(
                    LibObjC.SendAndGetHandle(
                        Handle,
                        LibObjC.GetSelector("activeVideoMinFrameDuration")),
                    retain: true);
                set =>
                    LibObjC.SendNoResult(
                        Handle,
                        LibObjC.GetSelector("setActiveVideoMinFrameDuration:"),
                        value.Handle);
            }

            public LibCoreMedia.CMTime ActiveVideoMinFrameDuration
            {
                get
                {
                    if (LibSystem.IsOnArm64)
                    {
                        LibObjC.SendAndGetCMTimeStret(out var result, Handle, LibObjC.GetSelector("activeVideoMinFrameDuration"));

                        return result;
                    }

                    return LibObjC.SendAndGetCMTime(Handle, LibObjC.GetSelector("activeVideoMinFrameDuration"));
                }
                set
                {
                    LibObjC.SendNoResult(
                        Handle,
                        LibObjC.GetSelector("setActiveVideoMinFrameDuration:"),
                        value);
                }
            }

            public LibCoreMedia.CMTime ActiveVideoMaxFrameDuration
            {
                get
                {
                    if (LibSystem.IsOnArm64)
                    {
                        LibObjC.SendAndGetCMTimeStret(out var result, Handle, LibObjC.GetSelector("activeVideoMaxFrameDuration"));

                        return result;
                    }

                    return LibObjC.SendAndGetCMTime(Handle, LibObjC.GetSelector("activeVideoMaxFrameDuration"));
                }
                set
                {
                    LibObjC.SendNoResult(
                        Handle,
                        LibObjC.GetSelector("setActiveVideoMaxFrameDuration:"),
                        value);
                }
            }

            public unsafe void LockForConfiguration()
            {
                IntPtr errorHandle;
                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("lockForConfiguration:"),
                    new IntPtr(&errorHandle));

                if (errorHandle != IntPtr.Zero)
                {
                    using var error = new LibObjC.NSError(errorHandle, retain: true);
                    throw new InvalidOperationException(/* error.FailureReason */);
                }
            }

            public unsafe void UnlockForConfiguration()
            {
                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("unlockForConfiguration:"));
            }

            public static AVCaptureDevice? DeviceWithUniqueID(string deviceUniqueID)
            {
                using var nativeDeviceUniqueID = LibCoreFoundation.CFString.Create(deviceUniqueID);
                var handle = LibObjC.SendAndGetHandle(
                    LibObjC.GetClass(nameof(AVCaptureDevice)),
                    LibObjC.GetSelector("deviceWithUniqueID:"),
                    nativeDeviceUniqueID.Handle);

                return handle == IntPtr.Zero ? null : new AVCaptureDevice(handle, retain: false);
            }

            public static AVAuthorizationStatus GetAuthorizationStatus(IntPtr mediaType)
            {
                return (AVAuthorizationStatus)(long)
                    LibObjC.SendAndGetHandle(
                        LibObjC.GetClass(nameof(AVCaptureDevice)),
                        LibObjC.GetSelector("authorizationStatusForMediaType:"),
                        mediaType);
            }

            private static LibObjC.BlockLiteralFactory? RequestAccessForMediaTypeBlockFactory;

            public static unsafe void RequestAccessForMediaType(IntPtr mediaType, AVRequestAccessStatus completion)
            {
                RequestAccessForMediaTypeBlockFactory ??= LibObjC.BlockLiteralFactory.CreateFactory(
                    delegate (IntPtr block, byte accessGranted)
                    {
                        LibObjC.BlockLiteral
                            .GetTarget<AVRequestAccessStatus>(block)
                            .Invoke(accessGranted != 0);
                    });

                using var blockLiteral = RequestAccessForMediaTypeBlockFactory.CreateLiteral(completion);

                LibObjC.SendNoResult(
                    LibObjC.GetClass(nameof(AVCaptureDevice)),
                    LibObjC.GetSelector("requestAccessForMediaType:completionHandler:"),
                    mediaType,
                    new IntPtr(&blockLiteral));
            }
        }

        public sealed class AVCaptureDeviceDiscoverySession : LibObjC.NSObject
        {
            public AVCaptureDeviceDiscoverySession(IntPtr handle, bool retain) :
                base(handle, retain)
            { }

            public AVCaptureDevice[] Devices =>
                LibCoreFoundation.CFArray.ToArray(
                    LibObjC.SendAndGetHandle(
                        LibObjC.GetClass(nameof(AVCaptureDeviceDiscoverySession)),
                        LibObjC.GetSelector("devices")),
                    static handle => new AVCaptureDevice(handle, retain: true));

            public static AVCaptureDeviceDiscoverySession DiscoverySessionWithVideoDevices()
            {
                var deviceTypes = new[]
                {
                    AVCaptureDeviceType.BuiltInWideAngleCamera,
                    AVCaptureDeviceType.BuiltInTelephotoCamera,
                    AVCaptureDeviceType.BuiltInDualCamera,
                    AVCaptureDeviceType.BuiltInTripleCamera,
                    AVCaptureDeviceType.BuiltInTrueDepthCamera
                }
                    .Where(static handle => handle != IntPtr.Zero)
                    .ToArray();

                using var deviceTypesArray = LibCoreFoundation.CFArray.Create(deviceTypes);

                var mediaType = AVMediaType.Video;
                var position = (long)AVCaptureDevicePosition.Unspecified;

                return new AVCaptureDeviceDiscoverySession(
                    LibObjC.SendAndGetHandle(
                        LibObjC.GetClass(nameof(AVCaptureDeviceDiscoverySession)),
                        LibObjC.GetSelector("discoverySessionWithDeviceTypes:mediaType:position:"),
                        deviceTypesArray.Handle,
                        mediaType,
                        position),
                    retain: false);
            }
        }

        public sealed class AVCaptureDeviceFormat : LibObjC.NSObject
        {
            public AVCaptureDeviceFormat(IntPtr handle, bool retain) :
                base(handle, retain)
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
                        LibObjC.GetSelector("videoSupportedFrameRateRanges")),
                    retain: true);
        }

        public enum AVCaptureDevicePosition : long
        {
    		Unspecified = 0,
    		Back = 1,
    		Front = 2,
    	}

        public static class AVCaptureDeviceType
        {
            public static readonly IntPtr BuiltInWideAngleCamera = GetConstant("AVCaptureDeviceTypeBuiltInWideAngleCamera");
            public static readonly IntPtr BuiltInTelephotoCamera = GetConstant("AVCaptureDeviceTypeBuiltInTelephotoCamera");
            public static readonly IntPtr BuiltInDualCamera = GetConstant("AVCaptureDeviceTypeBuiltInDualCamera");
            public static readonly IntPtr BuiltInTripleCamera = GetConstant("AVCaptureDeviceTypeBuiltInTripleCamera");
            public static readonly IntPtr BuiltInTrueDepthCamera = GetConstant("AVCaptureDeviceTypeBuiltInTrueDepthCamera");

            private static IntPtr GetConstant(string name) =>
                Dlfcn.GetSymbol(LibAVFoundation.Handle, name) is var handle && handle != IntPtr.Zero
                ? Marshal.ReadIntPtr(handle)
                : IntPtr.Zero;
        }
        
        public abstract class AVCaptureInput : LibObjC.NSObject
        {
            protected AVCaptureInput(IntPtr handle, bool retain) :
                base(handle, retain)
            { }
        }

        public abstract class AVCaptureOutput : LibObjC.NSObject
        {
            protected AVCaptureOutput(IntPtr handle, bool retain) :
                base(handle, retain)
            { }
        }

        public sealed class AVCaptureDeviceInput : AVCaptureInput
        {
            public AVCaptureDeviceInput(AVCaptureDevice device) :
                base(FromDevice(device), retain: false)
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
                    using var error = new LibObjC.NSError(errorHandle, retain: true);
                    throw new InvalidOperationException(/* error.FailureReason */);
                }

                return inputHandle;
            }
        }

        public sealed class AVCaptureVideoDataOutput : AVCaptureOutput
        {
            public AVCaptureVideoDataOutput() : base(
                LibObjC.SendAndGetHandle(
                    LibObjC.GetClass(nameof(AVCaptureVideoDataOutput)),
                    LibObjC.GetSelector(LibObjC.AllocSelector)),
                retain: false)
            {
                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("init"));
            }

            public unsafe int[] AvailableVideoCVPixelFormatTypes =>
                LibCoreFoundation.CFArray.ToArray(
                    LibObjC.SendAndGetHandle(
                        Handle,
                        LibObjC.GetSelector("availableVideoCVPixelFormatTypes")),
                    static handle =>
                    {
                        int value;
                        if (LibCoreFoundation.CFNumberGetValue(handle, LibCoreFoundation.CFNumberType.sInt32Type, &value))
                            return value;
                        throw new InvalidOperationException("The value contained by CFNumber cannot be read as 32-bit signed integer.");
                    });

            public unsafe void SetPixelFormatType(int format)
            {
                var number = LibCoreFoundation.CFNumberCreate(IntPtr.Zero, LibCoreFoundation.CFNumberType.sInt32Type, &format);
                var keys = stackalloc IntPtr[] { LibCoreVideo.kCVPixelBufferPixelFormatTypeKey };
                var values = stackalloc IntPtr[] { number };

                var dictionary = LibCoreFoundation.CFDictionaryCreate(
                    IntPtr.Zero,
                    &keys,
                    &values,
                    numValues: 1,
                    LibCoreFoundation.kCFCopyStringDictionaryKeyCallBacks,
                    LibCoreFoundation.kCFTypeDictionaryValueCallBacks);

                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("setVideoSettings:"),
                    dictionary);

                LibCoreFoundation.CFRelease(dictionary);
                LibCoreFoundation.CFRelease(number);
            }

            public void SetSampleBufferDelegate(AVCaptureVideoDataOutputSampleBuffer sampleBufferDelegate, IntPtr sampleBufferCallbackQueue) =>
                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("setSampleBufferDelegate:queue:"),
                    sampleBufferDelegate.Handle,
                    sampleBufferCallbackQueue);
        }

        public abstract class AVCaptureVideoDataOutputSampleBuffer : LibObjC.NSObject
        {
            private const string HandleVariableName = nameof(GCHandle);
            private static IntPtr HandleVariableDescriptor;
            
            static AVCaptureVideoDataOutputSampleBuffer()
            {
                var handle = LibObjC.AllocateClass(
                    LibObjC.GetClass("NSObject"),
                    nameof(AVCaptureVideoDataOutputSampleBuffer),
                    extraBytes: IntPtr.Zero);

                var didDropSampleBuffer = new DidDropSampleBufferDelegate(DidDropSampleBufferTrampoline);
                var didOutputSampleBuffer = new DidOutputSampleBufferDelegate(DidOutputSampleBufferTrampoline);
                
                LibObjC.AddMethod(
                    handle,
                    LibObjC.GetSelector("captureOutput:didDropSampleBuffer:fromConnection:"),
                    Marshal.GetFunctionPointerForDelegate(didDropSampleBuffer),
                    LibObjC.GetSignature(didDropSampleBuffer.Method, blockLiteral: false));

                LibObjC.AddMethod(
                    handle,
                    LibObjC.GetSelector("captureOutput:didOutputSampleBuffer:fromConnection:"),
                    Marshal.GetFunctionPointerForDelegate(didOutputSampleBuffer),
                    LibObjC.GetSignature(didOutputSampleBuffer.Method, blockLiteral: false));

                LibObjC.AddProtocol(
                    handle,
                    LibObjC.GetProtocol("AVCaptureVideoDataOutputSampleBufferDelegate"));

                LibObjC.AddVariable(
                    handle,
                    HandleVariableName,
                    size: new IntPtr(IntPtr.Size),
                    alignment: IntPtr.Size switch
                    {
                        4 => 2,
                        8 => 3,
                        _ => throw new NotSupportedException("The current arhitecture isn't supported.")
                    },
                    types: LibObjC.GetSignature(typeof(IntPtr)));

                LibObjC.RegisterClass(handle);

                HandleVariableDescriptor = LibObjC.GetVariable(handle, HandleVariableName);
            }

            protected AVCaptureVideoDataOutputSampleBuffer() : base(
                LibObjC.SendAndGetHandle(
                    LibObjC.GetClass(nameof(AVCaptureVideoDataOutputSampleBuffer)),
                    LibObjC.GetSelector(LibObjC.AllocSelector)),
                retain: false)
            {
                LibObjC.SetVariable(
                    Handle,
                    HandleVariableDescriptor,
                    Handle);
            }

            public abstract void DidDropSampleBuffer(IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection);

    		public abstract void DidOutputSampleBuffer(IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection);

            private delegate void DidDropSampleBufferDelegate(IntPtr self, IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection);
            private static void DidDropSampleBufferTrampoline(IntPtr self, IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection)
            {
                var handle = LibObjC.GetVariable(self, HandleVariableDescriptor);
                var obj = GCHandle.FromIntPtr(handle).Target as AVCaptureVideoDataOutputSampleBuffer;

                obj?.DidDropSampleBuffer(captureOutput, sampleBuffer, connection);
            }

            private delegate void DidOutputSampleBufferDelegate(IntPtr self, IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection);
            private static void DidOutputSampleBufferTrampoline(IntPtr self, IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection)
            {
                var handle = LibObjC.GetVariable(self, HandleVariableDescriptor);
                var obj = GCHandle.FromIntPtr(handle).Target as AVCaptureVideoDataOutputSampleBuffer;

                obj?.DidOutputSampleBuffer(captureOutput, sampleBuffer, connection);

            }

            private delegate void DeallocDelegate(IntPtr self);
            private static void DeallocTrampoline(IntPtr self)
            {
                var handle = LibObjC.GetVariable(self, HandleVariableDescriptor);

                GCHandle
                    .FromIntPtr(handle)
                    .Free();
            }
        }

        public sealed class AVCaptureSession : LibObjC.NSObject
        {
            public AVCaptureSession() : base(
                LibObjC.SendAndGetHandle(
                    LibObjC.GetClass(nameof(AVCaptureSession)),
                    LibObjC.GetSelector(LibObjC.AllocSelector)),
                retain: false)
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

        public enum AVAuthorizationStatus : long
        {
            NotDetermined,
            Restricted,
            Denied,
            Authorized
        }

        public static class AVMediaType
        {
            public static readonly IntPtr Video =
                Dlfcn.GetSymbol(LibAVFoundation.Handle, "AVMediaTypeVideo") is var handle && handle != IntPtr.Zero
                ? Marshal.ReadIntPtr(handle)
                : IntPtr.Zero;
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
}
