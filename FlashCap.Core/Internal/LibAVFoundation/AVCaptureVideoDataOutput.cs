using System;
using System.Runtime.InteropServices;

namespace FlashCap.Internal;

internal static partial class NativeMethods_AVFoundation
{
    public static partial class LibAVFoundation
    {
        public sealed class AVCaptureVideoDataOutput : AVCaptureOutput
        {
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

            public unsafe void SetPixelFormatType(int format)
            {

                var pixelFormat = format;
                
                IntPtr pixelFormatTypeKeyPtr = Dlfcn.dlsym(LibCoreVideo.Handle, "kCVPixelBufferPixelFormatTypeKey");
                if (pixelFormatTypeKeyPtr == IntPtr.Zero)
                {
                    Console.WriteLine("Não foi possível obter kCVPixelBufferPixelFormatTypeKey.");
                    return;
                }

                // Obtém o valor real da NSString a partir da constante
                IntPtr nsPixelFormatKey = Marshal.ReadIntPtr(pixelFormatTypeKeyPtr);

                //int pixelFormat = 1111970369; // Exemplo utilizando BGRA
                IntPtr nsNumber = LibObjC.CreateNSNumber(pixelFormat);

                // Utiliza a chave reconhecida pelo Core Video: kCVPixelBufferPixelFormatTypeKey
                //IntPtr nsPixelFormatKey = CreateNSString("kCVPixelBufferPixelFormatTypeKey");

                IntPtr nsDictionaryClass = LibObjC.GetClass("NSDictionary");
                IntPtr dictSel = LibObjC.GetSelector("dictionaryWithObject:forKey:");
                IntPtr videoSettings = LibObjC.SendAndGetHandle(nsDictionaryClass, dictSel, nsNumber, nsPixelFormatKey);
                IntPtr setVideoSettingsSel = LibObjC.GetSelector("setVideoSettings:");
                LibObjC.SendNoResult(this.Handle, setVideoSettingsSel, videoSettings);
                
                
            }

            public void SetSampleBufferDelegate(AVCaptureVideoDataOutputSampleBuffer sampleBufferDelegate, LibCoreFoundation.DispatchQueue sampleBufferCallbackQueue) =>
                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("setSampleBufferDelegate:queue:"),
                    sampleBufferDelegate.Handle,
                    sampleBufferCallbackQueue.Handle);
        }
    }
}