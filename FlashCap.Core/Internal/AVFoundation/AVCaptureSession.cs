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
using static FlashCap.Internal.NativeMethods_AVFoundation;

namespace FlashCap.Internal.AVFoundation;

partial class LibAVFoundation
{
    public sealed class AVCaptureSession : LibObjC.NSObject
    {
        private AVCaptureInput? _videoDataInput;
        private AVCaptureVideoDataOutput? _videoDataOutput;

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

            if (_videoDataInput is not null)
            {
                throw new InvalidOperationException("Only one video data input can be added to the session.");
            }

            _videoDataInput = input;
                
            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("addInput:"),
                input.Handle);
        }

        public void AddOutput(AVCaptureVideoDataOutput output)
        {
            ValidateHandle(nameof(AddOutput));

            if (_videoDataOutput is not null)
            {
                throw new InvalidOperationException("Only one video data output can be added to the session.");
            }

            _videoDataOutput = output;

            LibObjC.SendNoResult(
                Handle,
                LibObjC.GetSelector("addOutput:"),
                output.Handle);
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
        
        protected override void Dispose(bool disposing)
        {
            _videoDataOutput?.Dispose();
            _videoDataOutput = null;

            _videoDataInput?.Dispose();
            _videoDataInput = null;

            base.Dispose(disposing);
        }
    }
}
