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
        public AVCaptureSession() : base(IntPtr.Zero, false)
        {
            Init();
        }

        private void ValidateHandle(string method)
        {
            if (Handle == IntPtr.Zero)
            {
                throw new NullReferenceException($"{nameof(AVCaptureSession)} handle is NULL in '{method}'.");
            }
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
            ValidateHandle(nameof(Init));

            LibCoreFoundation.CFRetain(this.Handle);
        }

        public void AddInput(AVCaptureInput input)
        {
            ValidateHandle(nameof(AddInput));

            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("addInput:"),
                input.Handle);
        }

        public void AddOutput(AVCaptureOutput output)
        {
            IntPtr allocSel = LibObjC.GetSelector("alloc");
            IntPtr initSel = LibObjC.GetSelector("init");
            
            var videoDataOutputObj = output as AVCaptureVideoDataOutput 
                ?? throw new Exception("Failed to get video data output") ;

            var videoDataOutput = videoDataOutputObj.Handle;

            ValidateHandle(nameof(AddOutput));
            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("addOutput:"),
                videoDataOutput);
        }

        public bool CanAddOutput(AVCaptureOutput output)
        {
            ValidateHandle(nameof(CanAddOutput));
            return LibObjC.SendAndGetBool(
                Handle,
                LibObjC.GetSelector("canAddOutput:"),
                output.Handle);
        }

        public void StartRunning()
        {
            ValidateHandle(nameof(StartRunning));
            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("startRunning"));
        }

        public void StopRunning()
        {
            ValidateHandle(nameof(StopRunning));
            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("stopRunning"));
        }
    }
}
