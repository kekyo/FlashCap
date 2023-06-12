using System;
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

		// https://clang.llvm.org/docs/Block-ABI-Apple.html
		public unsafe struct BlockLiteral : IDisposable
		{
			private static readonly IntPtr NSConcreteStackBlock = Dlfcn.GetSymbol(LibSystem.Handle, "_NSConcreteStackBlock");
			
			private IntPtr _isa;
			private Flags _flags;
			private int _reserved;
			private IntPtr _invoke;
			private IntPtr _descriptor;
			private IntPtr _targetHandle;

			public BlockLiteral(byte[] signature, void* trampoline, Delegate target)
			{
				_isa = NSConcreteStackBlock;
				_flags = Flags.BLOCK_HAS_COPY_DISPOSE | Flags.BLOCK_HAS_SIGNATURE;
				_reserved = 0;
				_invoke = new IntPtr(trampoline);
				_descriptor = Marshal.AllocHGlobal(sizeof(Descriptor) + signature.Length);
				_targetHandle = (IntPtr)GCHandle.Alloc(target);
				
				delegate* unmanaged<IntPtr, IntPtr, void> copy = &CopyHandler;
				delegate* unmanaged<IntPtr, void> dispose = &DisposeHandler;

				var descriptor = (Descriptor*)_descriptor;

				descriptor->Copy = new IntPtr(copy);
				descriptor->Dispose = new IntPtr(dispose);
				descriptor->Size = new IntPtr(sizeof(BlockLiteral));
				descriptor->Signature = default;

			}
			
			public void Dispose ()
			{
				if (_targetHandle != IntPtr.Zero)
				{
					GCHandle
						.FromIntPtr(_targetHandle)
						.Free();

					_targetHandle = IntPtr.Zero;
				}

				if (_descriptor != IntPtr.Zero) {
					var descriptor = (Descriptor*)_descriptor;

					if (Interlocked.Decrement(ref descriptor->References) == 0)
					{
						Marshal.FreeHGlobal (_descriptor);
					}

					_descriptor = IntPtr.Zero;
				}
			}

			public static TDelegate GetTarget<TDelegate>(IntPtr block)
				where TDelegate : class
			{
				var self = (BlockLiteral*)block;
				var target = GCHandle.FromIntPtr(self->_targetHandle).Target;

				return (TDelegate)target!;
			}

			public static byte[] CreateSignature(Type returnType, params Type[] parameterTypes)
			{
				static string GetSignature(Type type)
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
						"System.Int16"=> "s",
						"System.UInt16"=> "S",
						"System.Int32"=> "i",
						"System.UInt32"=> "I",
						"System.Int64"=> "q",
						"System.UInt64"=> "Q",
						"System.Single"=> "f",
						"System.Double"=> "d",
						"System.Boolean" => RuntimeInformation.ProcessArchitecture switch
						{
							Architecture.X64 or Architecture.Arm64 => "B",
							Architecture.X86 or Architecture.Arm => "c",
							_ => throw new NotSupportedException($"The current architecture is not supported.")
						},
						_ => typeof(NSObject).IsAssignableFrom(type)
							? "@" : throw new NotSupportedException($"Type '{type}' is not supported for interop.")
					};
				}

				var signature = new StringBuilder()
					.Append(GetSignature(returnType))
					.Append("@?");

				foreach (var parameterType in parameterTypes)
				{
					signature.Append(GetSignature(parameterType));
				}

				return Encoding.UTF8.GetBytes(signature.ToString());
			}

			[UnmanagedCallersOnly]
			private static void CopyHandler(IntPtr dst, IntPtr src)
			{
				var dstLiteral = (BlockLiteral*)dst;
				var srcLiteral = (BlockLiteral*)src;
				var context = GCHandle.FromIntPtr(srcLiteral->_targetHandle);

				dstLiteral->_targetHandle = (IntPtr)GCHandle.Alloc(context);
				dstLiteral->_descriptor = srcLiteral->_descriptor;

				var descriptor = (Descriptor*)dstLiteral->_descriptor;

				Interlocked.Increment(ref descriptor->References);
			}

			[UnmanagedCallersOnly]
			private static void DisposeHandler(IntPtr self)
			{
				var literal = (BlockLiteral*)self;
				var descriptor = (Descriptor*)literal->_descriptor;

				if (Interlocked.Decrement(ref descriptor->References) == 0)
				{
					Marshal.FreeHGlobal(literal->_descriptor);
				}

				GCHandle
					.FromIntPtr(literal->_targetHandle)
					.Free();

				literal->_targetHandle = IntPtr.Zero;
				literal->_descriptor = IntPtr.Zero;
			}

			[Flags]
			private enum Flags
			{
				BLOCK_REFCOUNT_MASK 	= 0xffff,
				BLOCK_NEEDS_FREE 		= 1 << 24,
				BLOCK_HAS_COPY_DISPOSE 	= 1 << 25,
				BLOCK_HAS_CTOR 			= 1 << 26,
				BLOCK_IS_GC 			= 1 << 27,
				BLOCK_IS_GLOBAL 		= 1 << 28,
				BLOCK_HAS_STRET 		= 1 << 29,
				BLOCK_HAS_SIGNATURE 	= 1 << 30,
			}

			[StructLayout (LayoutKind.Sequential)]
			private struct Descriptor
			{
				public IntPtr Reserved;
				public IntPtr Size;
				public IntPtr Copy;
				public IntPtr Dispose;
				public IntPtr Signature;
				public int References;
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

		[DllImport (Path)]
		public extern static unsafe IntPtr CFStringCreateWithCharacters(IntPtr allocator, char* str, nint count);
	}

	[StructLayout (LayoutKind.Sequential)]
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
		public static T[] ToArray<T>(IntPtr handle, Func<IntPtr, T> constructor)
		{
			var count = LibCoreFoundation.CFArrayGetCount(handle).ToInt32();
			if (count == 0)
				return Array.Empty<T>();

			var buffer = count <= 256
				? stackalloc IntPtr[count]
				: new IntPtr[count];

			unsafe {
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

		public void Dispose ()
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

		private static byte[]? RequestAccessForMediaTypeSignature;

		public static unsafe void RequestAccessForMediaType(string mediaType, AVRequestAccessStatus completion)
		{
			[UnmanagedCallersOnly]
			static void Invoke(IntPtr block, byte accessGranted)
			{
				LibObjC.BlockLiteral
					.GetTarget<AVRequestAccessStatus>(block)
					.Invoke(accessGranted != 0);
			}

			RequestAccessForMediaTypeSignature ??= LibObjC.BlockLiteral.CreateSignature(
				typeof(void), typeof(IntPtr), typeof(byte));

			delegate* unmanaged<IntPtr, byte, void> trampoline = &Invoke;
			using var blockLiteral = new LibObjC.BlockLiteral(RequestAccessForMediaTypeSignature, trampoline, completion);
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
