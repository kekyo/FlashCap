////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Yoh Deadfall (@YohDeadfall)
// Copyright (c) Felipe Ferreira Quintella (@ffquintella)
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
        
        private void ValidateHandle()
        {
            if (Handle == IntPtr.Zero)
            {
                throw new ObjectDisposedException(nameof(AVCaptureVideoDataOutput), "Handle invalid. 0H01X");
            }
        }

        public unsafe int[] AvailableVideoCVPixelFormatTypes
        {
            get
            {
                ValidateHandle();
                return LibCoreFoundation.CFArray.ToArray(
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
            }
        }

        public bool AlwaysDiscardsLateVideoFrames
        {
            get
            {
                ValidateHandle();
                return LibObjC.SendAndGetBool(
                    Handle,
                    LibObjC.GetSelector("alwaysDiscardsLateVideoFrames"));
            }
            set
            {
                ValidateHandle();
                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("setAlwaysDiscardsLateVideoFrames:"),
                    value);
            }
        }


        public void SetPixelFormatType(int format)
        {
            ValidateHandle();
            var pixelFormat = format;

            IntPtr pixelFormatTypeKeyPtr = Dlfcn.dlsym(LibCoreVideo.Handle, "kCVPixelBufferPixelFormatTypeKey");
            if (pixelFormatTypeKeyPtr == IntPtr.Zero)
            {
                throw new Exception("Error comunicating with the AVCaptureVideoDataOutput");
            }

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
            ValidateHandle();
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

            string types = "v@:@@@";
            bool added = LibObjC.class_addMethod(delegateClass, selDidOutput, impCallback, types);
            if (!added)
            {
                return;
            }

            LibObjC.objc_registerClassPair(delegateClass);

            IntPtr delegateInstanceAlloc = LibObjC.SendAndGetHandle(delegateClass, allocSel);
            IntPtr delegateInstance = LibObjC.SendAndGetHandle(delegateInstanceAlloc, initSel);

            IntPtr setDelegateSel = LibObjC.GetSelector("setSampleBufferDelegate:queue:");
            LibObjC.SendNoResult(Handle, setDelegateSel, delegateInstance, sampleBufferCallbackQueue.Handle);
        }
        
        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (Handle != IntPtr.Zero)
            {
                LibCoreFoundation.CFRelease(Handle);
                Handle = IntPtr.Zero;
            }

            if (disposing)
            {
                if (callbackDelegate != null)
                {
                    Marshal.FreeHGlobal(Marshal.GetFunctionPointerForDelegate(callbackDelegate));
                    callbackDelegate = null;
                }
            }

            base.Dispose(disposing);
        }

        ~AVCaptureVideoDataOutput()
        {
            Dispose(false);
        }
    }
}
