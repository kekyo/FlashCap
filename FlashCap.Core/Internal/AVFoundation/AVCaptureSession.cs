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
            try
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
                
            } catch (Exception ex)
            {
                Console.WriteLine($"Error initializing AVCaptureSession: {ex.Message}");
                throw;
            }
        }

        public void AddInput(AVCaptureInput input)
        {
            try
            {
                ValidateHandle(nameof(AddInput));
                
                _videoDataInput = input as AVCaptureInput;
                
                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("addInput:"),
                    input.Handle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling addInput: {ex.Message}");
                throw;
            }
        }


        public void AddOutput(AVCaptureOutput output)
        {
            try
            {
                _videoDataOutput = output as AVCaptureVideoDataOutput ;

                if (_videoDataOutput == null)
                {
                    throw new Exception("Failed to get video data output");
                }
                
                var videoDataOutput = _videoDataOutput.Handle;

                ValidateHandle(nameof(AddOutput));
                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("addOutput:"),
                    videoDataOutput);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling addOutput: {ex.Message}");
                throw;
            }
        }

        public bool CanAddOutput(AVCaptureOutput output)
        {
            try
            {
                ValidateHandle(nameof(CanAddOutput));
                return LibObjC.SendAndGetBool(
                    Handle,
                    LibObjC.GetSelector("canAddOutput:"),
                    output.Handle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling canAddOutput: {ex.Message}");
                throw;
            }
        }


        public void StartRunning()
        {
            try
            {
                ValidateHandle(nameof(StartRunning));
                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("startRunning"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting AVCaptureSession: {ex.Message}");
            }
        }

        public void StopRunning()
        {
            try
            {
                ValidateHandle(nameof(StopRunning));
                var selector = LibObjC.GetSelector("stopRunning");
                LibObjC.SendNoResult(
                    Handle,
                    selector);
            } catch (Exception ex)
            {
                Console.WriteLine($"Error stopping AVCaptureSession: {ex.Message}");
            }
            
        }
        
        protected override void Dispose(bool disposing)
        {
            try
            {
                /*if (Handle != IntPtr.Zero)
                {
                    LibCoreFoundation.CFRelease(Handle);
                    Handle = IntPtr.Zero;
                }*/
                
                if (_videoDataOutput != null)
                {
                    _videoDataOutput.Dispose();
                    _videoDataOutput = null;
                }
                if (_videoDataInput != null)
                {
                    _videoDataInput.Dispose();
                    _videoDataInput = null;
                }

                base.Dispose(disposing);
            } catch (Exception ex)
            {
                Console.WriteLine($"Error disposing AVCaptureSession: {ex.Message}");
            }
        }

    }
}
