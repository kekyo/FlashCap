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
using System.Runtime.InteropServices;
using static FlashCap.Internal.NativeMethods_AVFoundation;

namespace FlashCap.Internal.AVFoundation;

partial class LibAVFoundation
{
    public sealed class AVCaptureSession : LibObjC.NSObject
    {
        /*private IntPtr _handle;
        public new IntPtr Handle
        {
            get
            {
                return _handle;
            }
            set
            {
                Console.WriteLine("Setting AVCaptureSession handle: " + value);
                _handle = value;
                base.Handle = value;
            }
        }*/

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
            
            if (sessionObj == IntPtr.Zero)
            {
                throw new Exception("Failed to create AVCaptureSession");
            }

            Handle = sessionObj;

            LibCoreFoundation.CFRetain(this.Handle);
        }
        
        private void ValidateHandle()
        {
            if (Handle == IntPtr.Zero)
            {
                throw new ObjectDisposedException(nameof(AVCaptureSession), "Handle invalid.");
            }
        }

        public void AddInput(AVCaptureInput input)
        {
            ValidateHandle();
            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("addInput:"),
                input.Handle);
        }


        public void AddOutput(AVCaptureOutput output)
        {
            ValidateHandle();
            IntPtr allocSel = LibObjC.GetSelector("alloc");
            IntPtr initSel = LibObjC.GetSelector("init");
            
            var videoDataOutputObj = output as AVCaptureVideoDataOutput ;

            if (videoDataOutputObj == null)
            {
                throw new Exception("Failed to get video data output");
            }
            
            var videoDataOutput = videoDataOutputObj.Handle;
            
            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("addOutput:"),
                videoDataOutput);
        }

        public bool CanAddOutput(AVCaptureOutput output)
        {
            ValidateHandle();
            return LibObjC.SendAndGetBool(
                Handle,
                LibObjC.GetSelector("canAddOutput:"),
                output.Handle);
        }


        public void StartRunning()
        {
            ValidateHandle();
            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("startRunning"));
        }

        public void StopRunning()
        {
            ValidateHandle();
            var selector = LibObjC.GetSelector("stopRunning");
            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("stopRunning"));
                
        }
        
        protected override void Dispose(bool disposing)
        {
            if (Handle != IntPtr.Zero)
            {
                LibCoreFoundation.CFRelease(Handle);
                Handle = IntPtr.Zero;
            }

            base.Dispose(disposing);
        }

    }
}
