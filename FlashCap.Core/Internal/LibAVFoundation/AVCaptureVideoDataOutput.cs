using System;
using System.Runtime.InteropServices;
using FlashCap.Devices;

namespace FlashCap.Internal;

public static partial class NativeMethods_AVFoundation
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

            public void SetSampleBufferDelegate(AVFoundationDevice.VideoBufferHandler sampleBufferDelegate,
                LibCoreFoundation.DispatchQueue sampleBufferCallbackQueue)
            {
                
                if (sampleBufferDelegate == null)
                {
                    Console.WriteLine("AVCaptureVideoDataOutputSampleBufferDelegate is null");
                    return;
                }
                
                IntPtr allocSel = LibObjC.GetSelector("alloc");
                IntPtr initSel = LibObjC.GetSelector("init");
                
                // Criação e registro da classe delegate dinâmica que implementa o protocolo AVCaptureVideoDataOutputSampleBufferDelegate.
                IntPtr nsObjectClass = LibObjC.GetClass("NSObject");
                IntPtr delegateClass = LibObjC.objc_allocateClassPair(nsObjectClass, "CaptureDelegate_" + Handle, IntPtr.Zero);
                // Seleciona o método a ser implementado.
                IntPtr selDidOutput = LibObjC.GetSelector("captureOutput:didOutputSampleBuffer:fromConnection:");
                
                AVCaptureVideoDataOutputSampleBuffer.CaptureOutputDidOutputSampleBuffer callbackDelegate = sampleBufferDelegate.CaptureOutputCallback;
                
                IntPtr impCallback = Marshal.GetFunctionPointerForDelegate(callbackDelegate);

                // A string de tipos "v@:@@@" indica um método que retorna void e recebe (self, _cmd, output, sampleBuffer, connection).
                string types = "v@:@@@";
                bool added = LibObjC.class_addMethod(delegateClass, selDidOutput, impCallback, types);
                if (!added)
                {
                    Console.WriteLine("Delegate already registred.");
                    return;
                }

                LibObjC.objc_registerClassPair(delegateClass);

                // Cria uma instância do delegate.
                IntPtr delegateInstanceAlloc = LibObjC.SendAndGetHandle(delegateClass, allocSel);
                IntPtr delegateInstance = LibObjC.SendAndGetHandle(delegateInstanceAlloc, initSel);

                // Cria uma fila de despacho (dispatch queue) para os callbacks.
                //IntPtr dispatchQueue = LibObjC.dispatch_queue_create("VideoMovie", IntPtr.Zero);

                // Define o delegate para a saída de vídeo:
                // [videoDataOutput setSampleBufferDelegate:delegateInstance queue:dispatchQueue]
                IntPtr setDelegateSel = LibObjC.GetSelector("setSampleBufferDelegate:queue:");
                LibObjC.SendNoResult(Handle, setDelegateSel, delegateInstance, sampleBufferCallbackQueue.Handle);
                
                
                /*
                
                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("setSampleBufferDelegate:queue:"),
                    sampleBufferDelegate.Handle,
                    sampleBufferCallbackQueue.Handle);
                */
            }

        }
    }
}