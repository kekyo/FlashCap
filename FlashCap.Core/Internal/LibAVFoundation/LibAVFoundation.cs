////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Yoh Deadfall (@YohDeadfall)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Linq;

namespace FlashCap.Internal;

partial class NativeMethods_AVFoundation
{
    public static partial class LibAVFoundation
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

            public AVCaptureVideoDataOutputSampleBuffer() : base(
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
            private delegate void DidDropSampleBufferDelegate(IntPtr self, IntPtr captureOutput, IntPtr sampleBuffer, IntPtr connection);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void CaptureOutputDidOutputSampleBuffer(IntPtr self, IntPtr _cmd, IntPtr output,
                IntPtr sampleBuffer, IntPtr connection);
            
            private delegate void DeallocDelegate(IntPtr self);

            private static DidDropSampleBufferDelegate DidDropSampleBufferTrampoline = (self, captureOutput, sampleBuffer, connection) =>
            {
                var handle = LibObjC.GetVariable(self, HandleVariableDescriptor);
                var obj = GCHandle.FromIntPtr(handle).Target as AVCaptureVideoDataOutputSampleBuffer;

                obj?.DidDropSampleBuffer(captureOutput, sampleBuffer, connection);
            };

            private static DeallocDelegate DeallocTrampoline = (self) =>
            {
                var handle = LibObjC.GetVariable(self, HandleVariableDescriptor);

                GCHandle
                    .FromIntPtr(handle)
                    .Free();
            };
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
        }
    }
}