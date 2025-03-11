using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace FlashCap.Internal;

internal static class NativeMethods_AVFoundation
{
    public static readonly Dictionary<PixelFormats, int> PixelFormatMap = new()
    {
        [PixelFormats.RGB24] = LibCoreVideo.PixelFormatType_24RGB,
        [PixelFormats.RGB32] = 32,
        [PixelFormats.UYVY] = LibCoreVideo.PixelFormatType_422YpCbCr8_yuvs,
        [PixelFormats.ARGB32] = LibCoreVideo.PixelFormatType_32BGRA,
        //[PixelFormats.ARGB32] = LibCoreVideo.PixelFormatType_32ARGB,
        [PixelFormats.YUYV] = LibCoreVideo.PixelFormatType_422YpCbCr8,
    };

    public static class Dlfcn
    {
        // Carrega o framework AVFoundation via dlopen.
        [DllImport("libdl.dylib", CharSet = CharSet.Ansi)]
        public static extern IntPtr dlopen(string path, int mode);
        
        [DllImport("libdl.dylib")]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);
        
        [DllImport(LibSystem.Path, EntryPoint = "dlopen")]
        public static extern IntPtr OpenLibrary(string path, Mode mode);

        [DllImport(LibSystem.Path, EntryPoint = "dlsym")]
        public static extern IntPtr GetSymbol(IntPtr handle, string symbol);

        public static IntPtr GetSymbolIndirect(IntPtr handle, string symbol) =>
            GetSymbol(LibAVFoundation.Handle, symbol) is var indirect && indirect != IntPtr.Zero
                ? Marshal.ReadIntPtr(indirect)
                : IntPtr.Zero;

        [Flags]
        public enum Mode : int
        {
            None = 0x0,
            Now = 2,
        }
    }

    public static class LibSystem
    {
        public const string Path = "/usr/lib/libSystem.dylib";

        public static readonly IntPtr Handle = Dlfcn.OpenLibrary(Path, Dlfcn.Mode.None);
        
        [DllImport(Path, EntryPoint = "dispatch_queue_create")]
        public static extern IntPtr dispatch_queue_create(string label, IntPtr attr);

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

        [DllImport(Path, EntryPoint = "dispatch_queue_create")]
        public extern static IntPtr DispatchQueueCreate(string label, IntPtr attr);

        [DllImport(Path, EntryPoint = "dispatch_release")]
        public extern static IntPtr DispatchRelease(IntPtr o);

        [DllImport(Path, EntryPoint = "dispatch_retain")]
        public extern static IntPtr DispatchRetain(IntPtr o);
    }

    public static class LibObjC
    {
        public const string Path = "/usr/lib/libobjc.A.dylib";

        public const string InitSelector = "init";
        public const string AllocSelector = "alloc";
        public const string ReleaseSelector = "release";

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern void SendNoResult(IntPtr receiver, IntPtr selector, bool arg1);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern void SendNoResult(IntPtr receiver, IntPtr selector);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern void SendNoResult(IntPtr receiver, IntPtr selector, IntPtr arg1);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern void SendNoResult(IntPtr receiver, IntPtr selector, LibCoreMedia.CMTime arg1);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public extern static void SendNoResult(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern bool SendAndGetBool(IntPtr receiver, IntPtr selector);
        
        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern bool SendAndGetBool(IntPtr receiver, IntPtr selector, IntPtr arg1);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern IntPtr SendAndGetHandle(IntPtr receiver, IntPtr selector);

        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern IntPtr SendAndGetHandle(IntPtr receiver, IntPtr selector, IntPtr arg1);
        
        [DllImport(Path, EntryPoint = "objc_msgSend")]
        public static extern IntPtr SendAndGetHandle(IntPtr receiver, IntPtr selector, int arg1);

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
        
        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_allocateClassPair")]
        public static extern IntPtr objc_allocateClassPair(IntPtr superClass, string name, IntPtr extraBytes);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "class_addMethod")]
        public static extern bool class_addMethod(IntPtr cls, IntPtr sel, IntPtr imp, string types);
        
        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_registerClassPair")]
        public static extern void objc_registerClassPair(IntPtr cls);
        
        [DllImport("/usr/lib/libSystem.dylib", EntryPoint = "dispatch_queue_create")]
        public static extern IntPtr dispatch_queue_create(string label, IntPtr attr);
        
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

            public static IntPtr Create(string signature, Delegate target, BlockLiteralCopy copy, BlockLiteralDispose dispose)
            {
                var signatureBytes = Encoding.UTF8.GetBytes(signature);
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

                dst->ContextHandle = (IntPtr)GCHandle.Alloc(context.Target);
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

            public static BlockLiteralFactory CreateFactory<T>(string signature, T trampoline)
                where T : Delegate
            {
                var descriptor = DescriptorCache<T>.Instance;
                if (descriptor == IntPtr.Zero)
                {
                    descriptor = BlockDescriptor.Create(signature, trampoline, CopyHandler, DisposeHandler);

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
        
        public static IntPtr CreateNSNumber(int value)
        {
            IntPtr nsNumberClass = LibObjC.GetClass("NSNumber");
            IntPtr sel = LibObjC.GetSelector("numberWithInt:");
            return LibObjC.SendAndGetHandle(nsNumberClass, sel, value);
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

            public string Error { get; set; } = string.Empty;
        }
    }

    public static class LibCoreFoundation
    {
        public const string Path = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

        public static readonly IntPtr Handle = Dlfcn.OpenLibrary(Path, Dlfcn.Mode.Now);
        public static readonly IntPtr kCFTypeArrayCallbacks = Dlfcn.GetSymbol(Handle, "kCFTypeArrayCallBacks");
        public static readonly IntPtr kCFCopyStringDictionaryKeyCallBacks = Dlfcn.GetSymbolIndirect(Handle, "kCFCopyStringDictionaryKeyCallBacks");
        public static readonly IntPtr kCFTypeDictionaryValueCallBacks = Dlfcn.GetSymbolIndirect(Handle, "kCFTypeDictionaryValueCallBacks");

        [DllImport(Path)]
        public static extern void CFRelease(IntPtr cf);
        
        [DllImport(Path)]
        public static extern IntPtr CFGetTypeID(IntPtr cf);

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
        public static extern unsafe IntPtr CFDictionaryCreate(IntPtr allocator, IntPtr[] keys, IntPtr[] values, nint numValues, IntPtr keyCallBacks, IntPtr valueCallBacks);

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
        
        public static class BasicClassesHelper
        {
            public static IntPtr NSString => LibObjC.GetClass("NSString");
            public static IntPtr NSNumber => LibObjC.GetClass("NSNumber");
            public static IntPtr NSArray => LibObjC.GetClass("NSArray");
            public static IntPtr NSDictionary => LibObjC.GetClass("NSDictionary");
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

        public sealed class DispatchQueue : NativeObject
        {
            public DispatchQueue(string label)
            {
                //Handle = LibC.DispatchQueueCreate(label, IntPtr.Zero) is var handle && handle != IntPtr.Zero
                //    ? handle : throw new InvalidOperationException("Cannot create a dispatch queue.");

                Handle = LibSystem.dispatch_queue_create(label, IntPtr.Zero);
                CFRetain(Handle);
            }


            protected override void Dispose(bool disposing) =>
                LibC.DispatchRelease(Handle);
        }
    }
    
    public enum CMBlockBufferError : int {
        None						= 0,
        StructureAllocationFailed	= -12700,
        BlockAllocationFailed		= -12701,
        BadCustomBlockSource		= -12702,
        BadOffsetParameter			= -12703,
        BadLengthParameter			= -12704,
        BadPointerParameter			= -12705,
        EmptyBlockBuffer			= -12706,
        UnallocatedBlock			= -12707,
        InsufficientSpace			= -12708,
    }

    public static class LibCoreMedia
    {
        

        
        public const string Path = "/System/Library/Frameworks/CoreMedia.framework/CoreMedia";

        [DllImport(Path)]
        public static extern IntPtr CMSampleBufferGetAttachments(IntPtr sampleBuffer, int makeWritable);

        [DllImport(Path)]
        public static extern IntPtr CMGetAttachment(IntPtr target, IntPtr key, out CMAttachmentMode attachmentMode);

        [DllImport(Path, EntryPoint = "CMFormatDescriptionGetMediaType", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint CmFormatDescriptionGetMediaTypeIntCode(IntPtr formatDescription);

        //[DllImport(Path, EntryPoint = "CMFormatDescriptionGetMediaType", CallingConvention = CallingConvention.Cdecl)]
        //public static extern FourCharCode CmFormatDescriptionGetMediaTypeFourCode(IntPtr formatDescription);
        
        //[DllImport(Path)]
        //public static extern CMVideoDimensions CMVideoFormatDescriptionGetDimensions(IntPtr videoDesc);

        // Add this enum
        public enum CMAttachmentMode
        {
            ShouldNotPropagate = 0,
            ShouldPropagate = 1
        }
        

        // Add this constant
        public static readonly FourCharCode kCMMediaType_Video = new FourCharCode('v', 'i', 'd', 'e');
        
        
        [DllImport(Path)]
        public static extern CMTime CMTimeMake(long value, int timescale);

        [DllImport(Path)]
        public static extern double CMTimeGetSeconds(CMTime time);

        [DllImport(Path)]
        public static extern IntPtr CMSampleBufferGetImageBuffer(IntPtr sbuf);
        
        [DllImport(Path)]
        public static extern IntPtr CMSampleBufferGetDataBuffer(IntPtr sbuf);
        
        [DllImport(Path)]
        public static extern bool CMSampleBufferIsValid(IntPtr sbuf);
        
        [DllImport(Path)]
        public static extern IntPtr CMSampleBufferGetFormatDescription(IntPtr sbuf);

        [DllImport(Path)]
        public static extern CMTime CMSampleBufferGetDecodeTimeStamp(IntPtr sbuf);
        
        [DllImport(Path)]
        public static extern CMBlockBufferError CMBlockBufferGetDataPointer(IntPtr theBuffer, nuint offset, out IntPtr lengthAtOffset, out IntPtr totalLength, out IntPtr dataPointer);

        [DllImport(Path)]
        public static extern nuint CMBlockBufferGetDataLength(IntPtr theBuffer);
        
        [DllImport(Path, CallingConvention = CallingConvention.Cdecl)]
        public static extern CMMediaType CMFormatDescriptionGetMediaType(IntPtr desc);
        //public static extern FourCharCode CMFormatDescriptionGetMediaType(IntPtr desc);
        
        [DllImport(Path)]
        public static extern IntPtr CMSampleBufferGetSampleAttachmentsArray(IntPtr sampleBuffer, bool createIfNecessary);
        

        [DllImport(Path)]
        public static extern uint CMFormatDescriptionGetMediaSubType(IntPtr desc);

        [DllImport(Path)]
        public static extern CMVideoDimensions CMVideoFormatDescriptionGetDimensions(IntPtr videoDesc);

        public enum CMMediaType : uint
        {
            Video = 1986618469, // 'video'
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
        //public const string Path = "/System/Library/Frameworks/CoreMedia.framework/CoreMedia";
        
        public const string Path = "/System/Library/Frameworks/CoreVideo.framework/CoreVideo";
        
        //IntPtr coreVideoHandle = dlopen("/System/Library/Frameworks/CoreVideo.framework/CoreVideo", RTLD_NOW);
        private const int RTLD_NOW = 2;
        public static readonly IntPtr Handle = Dlfcn.dlopen(Path, RTLD_NOW);
        
        public static readonly IntPtr kCVPixelBufferPixelFormatTypeKey = Dlfcn.GetSymbolIndirect(Handle, "kCVPixelBufferPixelFormatTypeKey");
        //public static readonly IntPtr kCVPixelBufferPixelFormatTypeKey = Dlfcn.GetSymbol(Handle, "kCVPixelBufferPixelFormatTypeKey");
        
        public static readonly IntPtr kCVPixelBufferMetalCompatibilityKey = Dlfcn.GetSymbolIndirect(Handle, "kCVPixelBufferMetalCompatibilityKey");

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

        //public static readonly int PixelFormatType_16BE555 = 0x00000010;
        public static readonly int PixelFormatType_16LE555 = GetFourCC("L555");
        public static readonly int PixelFormatType_16LE5551 = GetFourCC("5551");
        public static readonly int PixelFormatType_16BE565 = GetFourCC("B565");
        public static readonly int PixelFormatType_16LE565 = GetFourCC("L565");
        //public static readonly int PixelFormatType_24RGB = 0x00000018;
        public static readonly int PixelFormatType_24RGB = GetFourCC("24RG");
        public static readonly int PixelFormatType_24BGR = GetFourCC("24BG");
        //public static readonly int PixelFormatType_32ARGB = 0x00000020;
        public static readonly int PixelFormatType_32ARGB = GetFourCC("ARGB");
        public static readonly int PixelFormatType_32ABGR = GetFourCC("ABGR");
        public static readonly int PixelFormatType_32RGBA = GetFourCC("RGBA");
        public static readonly int PixelFormatType_32BGRA = GetFourCC("BGRA");
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


        private static int GetFourCC(string s)
        {
            var fcc = new FourCharCode(s);
            return fcc.GetIntVal();
        }
        
        /*private static int GetFourCC(string s) =>
            s[0] << 24 |
            s[1] << 16 |
            s[2] << 8 |
            s[3];*/
    }

    public static class LibAVFoundation
    {
        public const string Path = "/System/Library/Frameworks/AVFoundation.framework/AVFoundation";

        public static readonly IntPtr Handle = Dlfcn.OpenLibrary(Path, Dlfcn.Mode.Now);

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
                        return LibObjC.SendAndGetCMTime(Handle, LibObjC.GetSelector("maxFrameDuration"));
                    }

                    LibObjC.SendAndGetCMTimeStret(out var result, Handle, LibObjC.GetSelector("maxFrameDuration"));

                    return result;
                }
            }

            public LibCoreMedia.CMTime MinFrameDuration
            {
                get
                {
                    if (LibSystem.IsOnArm64)
                    {

                        return LibObjC.SendAndGetCMTime(Handle, LibObjC.GetSelector("minFrameDuration"));

                    }


                    LibObjC.SendAndGetCMTimeStret(out var result, Handle, LibObjC.GetSelector("minFrameDuration"));

                    return result;
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
                            LibObjC.GetSelector("uniqueID")));

                    var str = result.ToString() ?? throw new InvalidOperationException();

                    return str;
                }
            }

            public string ModelID
            {
                get
                {
                    using var result = new LibCoreFoundation.CFString(
                        LibObjC.SendAndGetHandle(
                            Handle,
                            LibObjC.GetSelector("modelID")));

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
                            LibObjC.GetSelector("localizedName")));

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
                        LibObjC.GetSelector("activeFormat")),
                    retain: true);
                set =>
                    LibObjC.SendNoResult(
                        Handle,
                        LibObjC.GetSelector("setActiveFormat:"),
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
                    Console.WriteLine(error.Error);
                    throw new InvalidOperationException(/* error.FailureReason */);
                }
            }

            public unsafe void UnlockForConfiguration()
            {
                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("unlockForConfiguration"));
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
            private delegate void RequestAccessForMediaTypeTrampoline(IntPtr block, byte accessGranted);

            public static unsafe void RequestAccessForMediaType(IntPtr mediaType, AVRequestAccessStatus completion)
            {
                RequestAccessForMediaTypeBlockFactory ??= LibObjC.BlockLiteralFactory.CreateFactory<RequestAccessForMediaTypeTrampoline>(
                    signature: "v@?^vC",
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
                        Handle,
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
                    AVCaptureDeviceType.BuiltInTrueDepthCamera,
                    AVCaptureDeviceType.ExternalCamera,
                    AVCaptureDeviceType.ExternalContinuityCamera
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
                    retain: true);
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

            public AVFrameRateRange[] VideoSupportedFrameRateRanges =>
                LibCoreFoundation.CFArray.ToArray(
                    LibObjC.SendAndGetHandle(
                        Handle,
                        LibObjC.GetSelector("videoSupportedFrameRateRanges")),
                    static handle => new AVFrameRateRange(handle, retain: true));
        }

        public enum AVCaptureDevicePosition : long
        {
            Unspecified = 0,
            Back = 1,
            Front = 2,
        }

        public static class AVCaptureDeviceType
        {
            public static readonly IntPtr BuiltInWideAngleCamera = Dlfcn.GetSymbolIndirect(LibAVFoundation.Handle, "AVCaptureDeviceTypeBuiltInWideAngleCamera");
            public static readonly IntPtr BuiltInTelephotoCamera = Dlfcn.GetSymbolIndirect(LibAVFoundation.Handle, "AVCaptureDeviceTypeBuiltInTelephotoCamera");
            public static readonly IntPtr BuiltInDualCamera = Dlfcn.GetSymbolIndirect(LibAVFoundation.Handle, "AVCaptureDeviceTypeBuiltInDualCamera");
            public static readonly IntPtr BuiltInTripleCamera = Dlfcn.GetSymbolIndirect(LibAVFoundation.Handle, "AVCaptureDeviceTypeBuiltInTripleCamera");
            public static readonly IntPtr BuiltInTrueDepthCamera = Dlfcn.GetSymbolIndirect(LibAVFoundation.Handle, "AVCaptureDeviceTypeBuiltInTrueDepthCamera");
            public static readonly IntPtr ExternalCamera = Dlfcn.GetSymbolIndirect(LibAVFoundation.Handle, "AVCaptureDeviceTypeExternal");
            public static readonly IntPtr ExternalContinuityCamera = Dlfcn.GetSymbolIndirect(LibAVFoundation.Handle, "AVCaptureDeviceTypeContinuityCamera");
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
                base(FromDevice(device), retain: true)
            { }

            private static unsafe IntPtr FromDevice(AVCaptureDevice device)
            {
                var errorHandle = default(IntPtr);
                var inputHandle = LibObjC.SendAndGetHandle(
                    LibObjC.GetClass("AVCaptureDeviceInput"),
                    LibObjC.GetSelector("deviceInputWithDevice:error:"),
                    device.Handle,
                    new IntPtr(&errorHandle));

                if (errorHandle != IntPtr.Zero)
                {
                    using var error = new LibObjC.NSError(errorHandle, retain: false);
                    Console.WriteLine(error.Error);
                    throw new InvalidOperationException(/* error.FailureReason */);
                }

                return inputHandle;
            }
        }

        public sealed class AVCaptureVideoDataOutput : AVCaptureOutput
        {
            public AVCaptureVideoDataOutput() : base(IntPtr.Zero, retain: false)
            {
                Init();
            }

            private void Init()
            {

                var videoAlloc = LibObjC.SendAndGetHandle(
                    LibObjC.GetClass("AVCaptureVideoDataOutput"),
                    LibObjC.GetSelector(LibObjC.AllocSelector));
                
                
                Handle = LibObjC.SendAndGetHandle(
                    videoAlloc,
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

            public bool AlwaysDiscardsLateVideoFrames
            {
                get =>
                    LibObjC.SendAndGetBool(
                        Handle,
                        LibObjC.GetSelector("alwaysDiscardsLateVideoFrames"));
                set =>
                    LibObjC.SendNoResult(
                        Handle,
                        LibObjC.GetSelector("setAlwaysDiscardsLateVideoFrames:"),
                        value);
            }

            public unsafe void SetPixelFormatType(int format)
            {
                format = 1111970369;
                
                //var number = LibCoreFoundation.CFNumberCreate(IntPtr.Zero, LibCoreFoundation.CFNumberType.sInt32Type, &format);
                
                var nsNumber = LibObjC.CreateNSNumber(format);
                
                LibCoreFoundation.CFRetain(nsNumber);
                
                //var keys = new IntPtr[] { LibCoreVideo.kCVPixelBufferPixelFormatTypeKey};
                //var values = new IntPtr[] { nsNumber };

                /*var dictionary = LibCoreFoundation.CFDictionaryCreate(
                    IntPtr.Zero,
                    keys,
                    values,
                    numValues: 1,
                    LibCoreFoundation.kCFCopyStringDictionaryKeyCallBacks,
                    LibCoreFoundation.kCFTypeDictionaryValueCallBacks);*/

                IntPtr nsDictionaryClass = LibCoreFoundation.BasicClassesHelper.NSDictionary;
                IntPtr dictSel =  LibObjC.GetSelector("dictionaryWithObject:forKey:");
                
               //LibCoreFoundation.CFRetain(dictionary);

                IntPtr nsPixelFormatKey = LibCoreVideo.kCVPixelBufferPixelFormatTypeKey;
               
                IntPtr videoSettings =  LibObjC.SendAndGetHandle(nsDictionaryClass, dictSel, nsNumber, nsPixelFormatKey);
                LibCoreFoundation.CFRetain(videoSettings);
                IntPtr setVideoSettingsSel = LibObjC.GetSelector("setVideoSettings:");
                
                LibObjC.SendNoResult(
                    Handle,
                    setVideoSettingsSel,
                    videoSettings);

                LibCoreFoundation.CFRelease(videoSettings);
                LibCoreFoundation.CFRelease(nsNumber);
                //LibCoreFoundation.CFRelease(strTrue.Handle);
            }

            public void SetSampleBufferDelegate(AVCaptureVideoDataOutputSampleBuffer sampleBufferDelegate, LibCoreFoundation.DispatchQueue sampleBufferCallbackQueue) =>
                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("setSampleBufferDelegate:queue:"),
                    sampleBufferDelegate.Handle,
                    sampleBufferCallbackQueue.Handle);
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

                LibObjC.AddMethod(
                    handle,
                    LibObjC.GetSelector("dealloc"),
                    Marshal.GetFunctionPointerForDelegate(DeallocTrampoline),
                    types: "v@:");

                LibObjC.AddMethod(
                    handle,
                    LibObjC.GetSelector("captureOutput:didDropSampleBuffer:fromConnection:"),
                    Marshal.GetFunctionPointerForDelegate(DidDropSampleBufferTrampoline),
                    types: "v@:@@@");

                LibObjC.AddMethod(
                    handle,
                    LibObjC.GetSelector("captureOutput:didOutputSampleBuffer:fromConnection:"),
                    Marshal.GetFunctionPointerForDelegate(DidOutputSampleBufferTrampoline),
                    types: "v@:@@@");

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
                    types: "^v");

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
                    (IntPtr)GCHandle.Alloc(this));
            }

            public abstract void DidDropSampleBuffer(IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection);

            public abstract void DidOutputSampleBuffer(IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection);

            private delegate void DidDropSampleBufferDelegate(IntPtr self, IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection);
            public delegate void DidOutputSampleBufferDelegate(IntPtr self, IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection);
            private delegate void DeallocDelegate(IntPtr self);

            private static DidDropSampleBufferDelegate DidDropSampleBufferTrampoline = (self, captureOutput, sampleBuffer, connection) =>
            {
                var handle = LibObjC.GetVariable(self, HandleVariableDescriptor);
                var obj = GCHandle.FromIntPtr(handle).Target as AVCaptureVideoDataOutputSampleBuffer;

                obj?.DidDropSampleBuffer(captureOutput, sampleBuffer, connection);
            };

            public static DidOutputSampleBufferDelegate DidOutputSampleBufferTrampoline = (self, captureOutput, sampleBuffer, connection) =>
            {
                var handle = LibObjC.GetVariable(self, HandleVariableDescriptor);
                var obj = GCHandle.FromIntPtr(handle).Target as AVCaptureVideoDataOutputSampleBuffer;

                obj?.DidOutputSampleBuffer(captureOutput, sampleBuffer, connection);
            };

            private static DeallocDelegate DeallocTrampoline = (self) =>
            {
                var handle = LibObjC.GetVariable(self, HandleVariableDescriptor);

                GCHandle
                    .FromIntPtr(handle)
                    .Free();
            };
        }

        public sealed class AVCaptureSession : LibObjC.NSObject
        {
            public AVCaptureSession() : base(IntPtr.Zero, false)
            {
                Init();
            }

            private void Init()
            {
                var sessionClass = LibObjC.SendAndGetHandle(
                    LibObjC.GetClass("AVCaptureSession"),
                    LibObjC.GetSelector(LibObjC.AllocSelector));
                
                var sessionObj = LibObjC.SendAndGetHandle(
                    sessionClass,
                    LibObjC.GetSelector("init"));
                
                Handle = sessionObj;
                
                LibCoreFoundation.CFRetain(this.Handle);
                
            }

            public void AddInput(AVCaptureInput input) =>
                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("addInput:"),
                    input.Handle);

            public void AddOutput(AVCaptureOutput output)
            {
                IntPtr allocSel = LibObjC.GetSelector("alloc");
                IntPtr initSel = LibObjC.GetSelector("init");
            
                // Criação da saída de vídeo: AVCaptureVideoDataOutput.
                IntPtr videoDataOutputClass = LibObjC.GetClass("AVCaptureVideoDataOutput");
                IntPtr videoOutputAlloc = LibObjC.SendAndGetHandle(videoDataOutputClass, allocSel);
                IntPtr videoDataOutput = LibObjC.SendAndGetHandle(videoOutputAlloc, initSel);    
                
                IntPtr pixelFormatTypeKeyPtr =  Dlfcn.dlsym(LibCoreVideo.Handle, "kCVPixelBufferPixelFormatTypeKey");
                if (pixelFormatTypeKeyPtr == IntPtr.Zero)
                {
                    Console.WriteLine("Não foi possível obter kCVPixelBufferPixelFormatTypeKey.");
                    return;
                }
                
                // Obtém o valor real da NSString a partir da constante
                IntPtr nsPixelFormatKey = Marshal.ReadIntPtr(pixelFormatTypeKeyPtr);
                
                int pixelFormat = 1111970369; // Exemplo utilizando BGRA
                IntPtr nsNumber = LibObjC.CreateNSNumber(pixelFormat);
                
                // Utiliza a chave reconhecida pelo Core Video: kCVPixelBufferPixelFormatTypeKey
                //IntPtr nsPixelFormatKey = CreateNSString("kCVPixelBufferPixelFormatTypeKey");
                
                IntPtr nsDictionaryClass = LibObjC.GetClass("NSDictionary");
                IntPtr dictSel = LibObjC.GetSelector("dictionaryWithObject:forKey:");
                IntPtr videoSettings = LibObjC.SendAndGetHandle(nsDictionaryClass, dictSel, nsNumber, nsPixelFormatKey);
                IntPtr setVideoSettingsSel = LibObjC.GetSelector("setVideoSettings:");
                LibObjC.SendNoResult(videoDataOutput, setVideoSettingsSel, videoSettings);

                // Criação e registro da classe delegate dinâmica que implementa o protocolo AVCaptureVideoDataOutputSampleBufferDelegate.
                IntPtr nsObjectClass = LibObjC.GetClass("NSObject");
                IntPtr delegateClass = LibObjC.objc_allocateClassPair(nsObjectClass, "CaptureDelegate", IntPtr.Zero);
                // Seleciona o método a ser implementado.
                IntPtr selDidOutput = LibObjC.GetSelector("captureOutput:didOutputSampleBuffer:fromConnection:");
                
                
                
                CaptureOutputDidOutputSampleBuffer callbackDelegate = new CaptureOutputDidOutputSampleBuffer(CaptureOutputCallback);
                
                
                IntPtr impCallback = Marshal.GetFunctionPointerForDelegate(callbackDelegate);
            
                // A string de tipos "v@:@@@" indica um método que retorna void e recebe (self, _cmd, output, sampleBuffer, connection).
                string types = "v@:@@@";
                bool added = LibObjC.class_addMethod(delegateClass, selDidOutput, impCallback, types);
                if (!added)
                {
                    Console.WriteLine("Falha ao adicionar método ao delegate.");
                    return;
                }
                LibObjC.objc_registerClassPair(delegateClass);

                // Cria uma instância do delegate.
                IntPtr delegateInstanceAlloc = LibObjC.SendAndGetHandle(delegateClass, allocSel);
                IntPtr delegateInstance = LibObjC.SendAndGetHandle(delegateInstanceAlloc, initSel);

                // Cria uma fila de despacho (dispatch queue) para os callbacks.
                IntPtr dispatchQueue = LibObjC.dispatch_queue_create("VideoMovie", IntPtr.Zero);

                // Define o delegate para a saída de vídeo:
                // [videoDataOutput setSampleBufferDelegate:delegateInstance queue:dispatchQueue]
                IntPtr setDelegateSel = LibObjC.GetSelector("setSampleBufferDelegate:queue:");
                LibObjC.SendNoResult(videoDataOutput, setDelegateSel, delegateInstance, dispatchQueue);
                
                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("addOutput:"),
                    videoDataOutput);
                
                
            }

            // Delegate correspondente ao método:
            // - (void)captureOutput:(id)output didOutputSampleBuffer:(id)sampleBuffer fromConnection:(id)connection;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void CaptureOutputDidOutputSampleBuffer(IntPtr self, IntPtr _cmd, IntPtr output, IntPtr sampleBuffer, IntPtr connection);

            
            public static void CaptureOutputCallback(IntPtr self, IntPtr _cmd, IntPtr output, IntPtr sampleBuffer, IntPtr connection)
            {
                Console.WriteLine("Frame capturado.");
                // Processamento adicional do sampleBuffer pode ser implementado aqui.
            
                var pixelBuffer = LibCoreMedia.CMSampleBufferGetImageBuffer(sampleBuffer);
            
                Console.WriteLine("PixelBuffer: " + pixelBuffer);
            }
            
            public bool CanAddOutput(AVCaptureOutput output) =>
                LibObjC.SendAndGetBool(
                    Handle,
                    LibObjC.GetSelector("canAddOutput:"),
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
            public static readonly IntPtr Video = Dlfcn.GetSymbolIndirect(LibAVFoundation.Handle, "AVMediaTypeVideo");
            //public static readonly IntPtr Video = Dlfcn.GetSymbolIndirect(LibAVFoundation.Handle, "AVMediaTypeVideo");
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
