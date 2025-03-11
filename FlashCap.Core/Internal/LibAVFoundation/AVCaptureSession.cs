using System;
using System.Runtime.InteropServices;

namespace FlashCap.Internal;


internal static partial class NativeMethods_AVFoundation
{
    public static partial class LibAVFoundation
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

                // Criação da saída de vídeo: AVCaptureVideoDataOutput.
                IntPtr videoDataOutputClass = LibObjC.GetClass("AVCaptureVideoDataOutput");
                IntPtr videoOutputAlloc = LibObjC.SendAndGetHandle(videoDataOutputClass, allocSel);
                IntPtr videoDataOutput = LibObjC.SendAndGetHandle(videoOutputAlloc, initSel);

                IntPtr pixelFormatTypeKeyPtr = Dlfcn.dlsym(LibCoreVideo.Handle, "kCVPixelBufferPixelFormatTypeKey");
                if (pixelFormatTypeKeyPtr == IntPtr.Zero)
                {
                    Console.WriteLine("Não foi possível obter kCVPixelBufferPixelFormatTypeKey.");
                    return;
                }

                // Obtém o valor real da NSString a partir da constante
                IntPtr nsPixelFormatKey = Marshal.ReadIntPtr(pixelFormatTypeKeyPtr);

                int pixelFormat = 1111970369; // Exemplo utilizando BGRA
                IntPtr nsNumber = LibObjC.CreateNSNumber(pixelFormat);

                // Utiliza a chave reconhecida pelo Core Video: kCVPixelBufferPixelFormatTypeKey
                //IntPtr nsPixelFormatKey = CreateNSString("kCVPixelBufferPixelFormatTypeKey");

                IntPtr nsDictionaryClass = LibObjC.GetClass("NSDictionary");
                IntPtr dictSel = LibObjC.GetSelector("dictionaryWithObject:forKey:");
                IntPtr videoSettings = LibObjC.SendAndGetHandle(nsDictionaryClass, dictSel, nsNumber, nsPixelFormatKey);
                IntPtr setVideoSettingsSel = LibObjC.GetSelector("setVideoSettings:");
                LibObjC.SendNoResult(videoDataOutput, setVideoSettingsSel, videoSettings);

                // Criação e registro da classe delegate dinâmica que implementa o protocolo AVCaptureVideoDataOutputSampleBufferDelegate.
                IntPtr nsObjectClass = LibObjC.GetClass("NSObject");
                IntPtr delegateClass = LibObjC.objc_allocateClassPair(nsObjectClass, "CaptureDelegate", IntPtr.Zero);
                // Seleciona o método a ser implementado.
                IntPtr selDidOutput = LibObjC.GetSelector("captureOutput:didOutputSampleBuffer:fromConnection:");



                CaptureOutputDidOutputSampleBuffer callbackDelegate =
                    new CaptureOutputDidOutputSampleBuffer(CaptureOutputCallback);


                IntPtr impCallback = Marshal.GetFunctionPointerForDelegate(callbackDelegate);

                // A string de tipos "v@:@@@" indica um método que retorna void e recebe (self, _cmd, output, sampleBuffer, connection).
                string types = "v@:@@@";
                bool added = LibObjC.class_addMethod(delegateClass, selDidOutput, impCallback, types);
                if (!added)
                {
                    Console.WriteLine("Falha ao adicionar método ao delegate.");
                    return;
                }

                LibObjC.objc_registerClassPair(delegateClass);

                // Cria uma instância do delegate.
                IntPtr delegateInstanceAlloc = LibObjC.SendAndGetHandle(delegateClass, allocSel);
                IntPtr delegateInstance = LibObjC.SendAndGetHandle(delegateInstanceAlloc, initSel);

                // Cria uma fila de despacho (dispatch queue) para os callbacks.
                IntPtr dispatchQueue = LibObjC.dispatch_queue_create("VideoMovie", IntPtr.Zero);

                // Define o delegate para a saída de vídeo:
                // [videoDataOutput setSampleBufferDelegate:delegateInstance queue:dispatchQueue]
                IntPtr setDelegateSel = LibObjC.GetSelector("setSampleBufferDelegate:queue:");
                LibObjC.SendNoResult(videoDataOutput, setDelegateSel, delegateInstance, dispatchQueue);

                LibObjC.SendNoResult(
                    Handle,
                    LibObjC.GetSelector("addOutput:"),
                    videoDataOutput);


            }

            // Delegate correspondente ao método:
            // - (void)captureOutput:(id)output didOutputSampleBuffer:(id)sampleBuffer fromConnection:(id)connection;
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void CaptureOutputDidOutputSampleBuffer(IntPtr self, IntPtr _cmd, IntPtr output,
                IntPtr sampleBuffer, IntPtr connection);


            public static void CaptureOutputCallback(IntPtr self, IntPtr _cmd, IntPtr output, IntPtr sampleBuffer,
                IntPtr connection)
            {
                Console.WriteLine("Frame capturado.");
                // Processamento adicional do sampleBuffer pode ser implementado aqui.

                var pixelBuffer = LibCoreMedia.CMSampleBufferGetImageBuffer(sampleBuffer);

                Console.WriteLine("PixelBuffer: " + pixelBuffer);
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
}