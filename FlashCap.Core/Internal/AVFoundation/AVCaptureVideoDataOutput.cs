////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Yoh Deadfall (@YohDeadfall)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FlashCap.Devices;
using static FlashCap.Internal.NativeMethods_AVFoundation;

namespace FlashCap.Internal.AVFoundation;

partial class LibAVFoundation
{
    public sealed class AVCaptureVideoDataOutput : AVCaptureOutput
    {
        private AVCaptureVideoDataOutputSampleBuffer.CaptureOutputDidOutputSampleBuffer? callbackDelegate;
        
        public AVCaptureVideoDataOutput() : base(IntPtr.Zero, retain: false)
        {
            Init();
        }

        private void Init()
        {
            IntPtr allocSel = LibObjC.GetSelector("alloc");
            IntPtr initSel = LibObjC.GetSelector("init");
            IntPtr videoDataOutputClass = LibObjC.GetClass("AVCaptureVideoDataOutput");
            IntPtr videoOutputAlloc = LibObjC.SendAndGetHandle(videoDataOutputClass, allocSel);
            IntPtr videoDataOutput = LibObjC.SendAndGetHandle(videoOutputAlloc, initSel);
            
            Handle = videoDataOutput;
            
            LibCoreFoundation.CFRetain(Handle);
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

        public void SetPixelFormatType(int format)
        {
            var pixelFormat = format;
            
            IntPtr pixelFormatTypeKeyPtr = Dlfcn.dlsym(LibCoreVideo.Handle, "kCVPixelBufferPixelFormatTypeKey");
            if (pixelFormatTypeKeyPtr == IntPtr.Zero)
            {
                throw new Exception("Error comunicating with the AVCaptureVideoDataOutput");
            }

            // Get NSString value
            IntPtr nsPixelFormatKey = Marshal.ReadIntPtr(pixelFormatTypeKeyPtr);
            IntPtr nsNumber = LibObjC.CreateNSNumber(pixelFormat);

            IntPtr nsDictionaryClass = LibObjC.GetClass("NSDictionary");
            IntPtr dictSel = LibObjC.GetSelector("dictionaryWithObject:forKey:");
            IntPtr videoSettings = LibObjC.SendAndGetHandle(nsDictionaryClass, dictSel, nsNumber, nsPixelFormatKey);
            IntPtr setVideoSettingsSel = LibObjC.GetSelector("setVideoSettings:");
            LibObjC.SendNoResult(this.Handle, setVideoSettingsSel, videoSettings);
            
        }

        public void SetSampleBufferDelegate(AVFoundationDevice.VideoBufferHandler sampleBufferDelegate,
            LibCoreFoundation.DispatchQueue sampleBufferCallbackQueue)
        {
            if (sampleBufferDelegate == null)
            {
                Debug.WriteLine("AVCaptureVideoDataOutputSampleBufferDelegate is null");
                return;
            }
            
            IntPtr allocSel = LibObjC.GetSelector("alloc");
            IntPtr initSel = LibObjC.GetSelector("init");
            IntPtr nsObjectClass = LibObjC.GetClass("NSObject");
            IntPtr delegateClass = LibObjC.objc_allocateClassPair(nsObjectClass, "CaptureDelegate_" + Handle, IntPtr.Zero);
            IntPtr selDidOutput = LibObjC.GetSelector("captureOutput:didOutputSampleBuffer:fromConnection:");
            
            callbackDelegate = sampleBufferDelegate.CaptureOutputCallback;
            
            IntPtr impCallback = Marshal.GetFunctionPointerForDelegate(callbackDelegate);

            // "v@:@@@" this means the methood returns void and receives (self, _cmd, output, sampleBuffer, connection).
            string types = "v@:@@@";
            bool added = LibObjC.class_addMethod(delegateClass, selDidOutput, impCallback, types);
            if (!added)
            {
                return;
            }

            LibObjC.objc_registerClassPair(delegateClass);

            // Delegate creation
            IntPtr delegateInstanceAlloc = LibObjC.SendAndGetHandle(delegateClass, allocSel);
            IntPtr delegateInstance = LibObjC.SendAndGetHandle(delegateInstanceAlloc, initSel);
            
            IntPtr setDelegateSel = LibObjC.GetSelector("setSampleBufferDelegate:queue:");
            LibObjC.SendNoResult(Handle, setDelegateSel, delegateInstance, sampleBufferCallbackQueue.Handle);
        }
    }
}
