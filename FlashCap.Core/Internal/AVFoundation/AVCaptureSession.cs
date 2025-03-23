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
using static FlashCap.Internal.NativeMethods_AVFoundation;

namespace FlashCap.Internal.AVFoundation;

partial class LibAVFoundation
{
    public sealed partial class AVCaptureSession : LibObjC.NSObject
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
}
