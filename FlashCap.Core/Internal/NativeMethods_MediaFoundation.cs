#if FLASHCAP_MEDIAFOUNDATION
////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
#if NET8_0_OR_GREATER
using System.Runtime.InteropServices.Marshalling;
#endif

namespace FlashCap.Internal;

[SupportedOSPlatform("windows6.1")]
[SuppressUnmanagedCodeSecurity]
internal static unsafe partial class NativeMethods_MediaFoundation
{
    public const uint MFSTARTUP_FULL = 0;
    public const uint MF_VERSION = 0x00020070;

    public const int MF_E_NO_MORE_TYPES = unchecked((int)0xc00d36b9);

    public static readonly Guid IID_IUnknown =
        new("00000000-0000-0000-C000-000000000046");
    public static readonly Guid IID_IMFAttributes =
        new("2CD2D921-C447-44A7-A13C-4ADABFC247E3");
    public static readonly Guid IID_IMFActivate =
        new("7FEE9E9A-4A89-47A6-899C-B6A53A70FB67");
    public static readonly Guid IID_IMFMediaBuffer =
        new("045FA593-8799-42B8-BC8D-8968C6453507");
    public static readonly Guid IID_IMFMediaSource =
        new("279A808D-AEC7-40C8-9C6B-A6B492C78A66");
    public static readonly Guid IID_IMFMediaType =
        new("44AE0FA8-EA31-4109-8D2E-4CAE4997C555");
    public static readonly Guid IID_IMFSample =
        new("C40A00F2-B93A-4D80-AE8C-5A1C634F58E4");
    public static readonly Guid IID_IMFSourceReader =
        new("70AE66F2-C809-4E4F-8915-BDCB406B7993");

    public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME =
        new(0x60d0e559, 0x52f8, 0x4fa2, 0xbb, 0xce, 0xac, 0xdb, 0x34, 0xa8, 0xec, 0x01);
    public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE =
        new(0xc60ac5fe, 0x252a, 0x478f, 0xa0, 0xef, 0xbc, 0x8f, 0xa5, 0xf7, 0xca, 0xd3);
    public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID =
        new(0x8ac3587a, 0x4ae7, 0x42d8, 0x99, 0xe0, 0x0a, 0x60, 0x13, 0xee, 0xf9, 0x0f);
    public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK =
        new(0x58f0aad8, 0x22bf, 0x4f8a, 0xbb, 0x3d, 0xd2, 0xc4, 0x97, 0x8c, 0x6e, 0x2f);
    public static readonly Guid MF_MT_DEFAULT_STRIDE =
        new(0x644b4e48, 0x1e02, 0x4516, 0xb0, 0xeb, 0xc0, 0x1c, 0xa9, 0xd4, 0x9a, 0xc6);
    public static readonly Guid MF_MT_FRAME_RATE =
        new(0xc459a2e8, 0x3d2c, 0x4e44, 0xb1, 0x32, 0xfe, 0xe5, 0x15, 0x6c, 0x7b, 0xb0);
    public static readonly Guid MF_MT_FRAME_SIZE =
        new(0x1652c33d, 0xd6b2, 0x4012, 0xb8, 0x34, 0x72, 0x03, 0x08, 0x49, 0xa3, 0x7d);
    public static readonly Guid MF_MT_MAJOR_TYPE =
        new(0x48eba18e, 0xf8c9, 0x4687, 0xbf, 0x11, 0x0a, 0x74, 0xc9, 0xf9, 0x6a, 0x8f);
    public static readonly Guid MF_MT_SUBTYPE =
        new(0xf7e34c9a, 0x42e8, 0x4714, 0xb7, 0x4b, 0xcb, 0x29, 0xd7, 0x2c, 0x35, 0xe5);
    public static readonly Guid MF_SOURCE_READER_ASYNC_CALLBACK =
        new(0x1e3dbeac, 0xbb43, 0x4c35, 0xb5, 0x07, 0xcd, 0x64, 0x44, 0x64, 0xc9, 0x65);
    public static readonly Guid MFMediaType_Video =
        new(0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    public static readonly Guid MFVideoFormat_ARGB32 =
        new(0x00000015, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    public static readonly Guid MFVideoFormat_MJPG =
        new(0x47504a4d, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    public static readonly Guid MFVideoFormat_NV12 =
        new(0x3231564e, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    public static readonly Guid MFVideoFormat_RGB24 =
        new(0x00000014, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    public static readonly Guid MFVideoFormat_RGB32 =
        new(0x00000016, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    public static readonly Guid MFVideoFormat_RGB555 =
        new(0x00000018, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    public static readonly Guid MFVideoFormat_RGB565 =
        new(0x00000017, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    public static readonly Guid MFVideoFormat_UYVY =
        new(0x59565955, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    public static readonly Guid MFVideoFormat_YUY2 =
        new(0x32595559, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

    public enum MF_SOURCE_READER_CONSTANTS
    {
        MF_SOURCE_READER_INVALID_STREAM_INDEX = -1,
        MF_SOURCE_READER_ALL_STREAMS = -2,
        MF_SOURCE_READER_ANY_STREAM = -2,
        MF_SOURCE_READER_FIRST_AUDIO_STREAM = -3,
        MF_SOURCE_READER_FIRST_VIDEO_STREAM = -4,
        MF_SOURCE_READER_MEDIASOURCE = -1,
    }

    [Flags]
    public enum MF_SOURCE_READER_FLAG
    {
        MF_SOURCE_READERF_ERROR = 0x00000001,
        MF_SOURCE_READERF_ENDOFSTREAM = 0x00000002,
        MF_SOURCE_READERF_NEWSTREAM = 0x00000004,
        MF_SOURCE_READERF_NATIVEMEDIATYPECHANGED = 0x00000010,
        MF_SOURCE_READERF_CURRENTMEDIATYPECHANGED = 0x00000020,
        MF_SOURCE_READERF_STREAMTICK = 0x00000100,
        MF_SOURCE_READERF_ALLEFFECTSREMOVED = 0x00000200,
    }

    [Guid("00000000-0000-0000-C000-000000000046")]
    [StructLayout(LayoutKind.Sequential)]
    public struct IUnknown
    {
        private void** lpVtbl;

        public uint Release()
        {
            fixed (IUnknown* self = &this)
            {
                return ((delegate* unmanaged[Stdcall]<IUnknown*, uint>)this.lpVtbl[2])(self);
            }
        }
    }

    [Guid("2CD2D921-C447-44A7-A13C-4ADABFC247E3")]
    [StructLayout(LayoutKind.Sequential)]
    public struct IMFAttributes
    {
        private void** lpVtbl;

        public int GetUINT32(in Guid guidKey, out uint value)
        {
            fixed (IMFAttributes* self = &this)
            fixed (Guid* guidKeyLocal = &guidKey)
            fixed (uint* valueLocal = &value)
            {
                return ((delegate* unmanaged[Stdcall]<IMFAttributes*, Guid*, uint*, int>)this.lpVtbl[7])(
                    self,
                    guidKeyLocal,
                    valueLocal);
            }
        }

        public int GetUINT64(in Guid guidKey, out ulong value)
        {
            fixed (IMFAttributes* self = &this)
            fixed (Guid* guidKeyLocal = &guidKey)
            fixed (ulong* valueLocal = &value)
            {
                return ((delegate* unmanaged[Stdcall]<IMFAttributes*, Guid*, ulong*, int>)this.lpVtbl[8])(
                    self,
                    guidKeyLocal,
                    valueLocal);
            }
        }

        public int GetGUID(in Guid guidKey, out Guid value)
        {
            fixed (IMFAttributes* self = &this)
            fixed (Guid* guidKeyLocal = &guidKey)
            fixed (Guid* valueLocal = &value)
            {
                return ((delegate* unmanaged[Stdcall]<IMFAttributes*, Guid*, Guid*, int>)this.lpVtbl[10])(
                    self,
                    guidKeyLocal,
                    valueLocal);
            }
        }

        public int GetAllocatedString(in Guid guidKey, out IntPtr value, out uint length)
        {
            fixed (IMFAttributes* self = &this)
            fixed (Guid* guidKeyLocal = &guidKey)
            fixed (IntPtr* valueLocal = &value)
            fixed (uint* lengthLocal = &length)
            {
                return ((delegate* unmanaged[Stdcall]<IMFAttributes*, Guid*, IntPtr*, uint*, int>)this.lpVtbl[13])(
                    self,
                    guidKeyLocal,
                    valueLocal,
                    lengthLocal);
            }
        }

        public int SetGUID(in Guid guidKey, in Guid value)
        {
            fixed (IMFAttributes* self = &this)
            fixed (Guid* guidKeyLocal = &guidKey)
            fixed (Guid* valueLocal = &value)
            {
                return ((delegate* unmanaged[Stdcall]<IMFAttributes*, Guid*, Guid*, int>)this.lpVtbl[24])(
                    self,
                    guidKeyLocal,
                    valueLocal);
            }
        }

        public int SetUnknown(in Guid guidKey, IUnknown* value)
        {
            fixed (IMFAttributes* self = &this)
            fixed (Guid* guidKeyLocal = &guidKey)
            {
                return ((delegate* unmanaged[Stdcall]<IMFAttributes*, Guid*, IUnknown*, int>)this.lpVtbl[27])(
                    self,
                    guidKeyLocal,
                    value);
            }
        }
    }

    [Guid("7FEE9E9A-4A89-47A6-899C-B6A53A70FB67")]
    [StructLayout(LayoutKind.Sequential)]
    public struct IMFActivate
    {
        private void** lpVtbl;

        public int GetAllocatedString(in Guid guidKey, out IntPtr value, out uint length)
        {
            fixed (IMFActivate* self = &this)
            fixed (Guid* guidKeyLocal = &guidKey)
            fixed (IntPtr* valueLocal = &value)
            fixed (uint* lengthLocal = &length)
            {
                return ((delegate* unmanaged[Stdcall]<IMFActivate*, Guid*, IntPtr*, uint*, int>)this.lpVtbl[13])(
                    self,
                    guidKeyLocal,
                    valueLocal,
                    lengthLocal);
            }
        }

        public int ActivateObject(in Guid iid, out void* value)
        {
            fixed (IMFActivate* self = &this)
            fixed (Guid* iidLocal = &iid)
            fixed (void** valueLocal = &value)
            {
                return ((delegate* unmanaged[Stdcall]<IMFActivate*, Guid*, void**, int>)this.lpVtbl[33])(
                    self,
                    iidLocal,
                    valueLocal);
            }
        }

        public int ShutdownObject()
        {
            fixed (IMFActivate* self = &this)
            {
                return ((delegate* unmanaged[Stdcall]<IMFActivate*, int>)this.lpVtbl[34])(self);
            }
        }
    }

    [Guid("045FA593-8799-42B8-BC8D-8968C6453507")]
    [StructLayout(LayoutKind.Sequential)]
    public struct IMFMediaBuffer
    {
        private void** lpVtbl;

        public int Lock(out byte* data, out uint maximumLength, out uint currentLength)
        {
            fixed (IMFMediaBuffer* self = &this)
            fixed (byte** dataLocal = &data)
            fixed (uint* maximumLengthLocal = &maximumLength)
            fixed (uint* currentLengthLocal = &currentLength)
            {
                return ((delegate* unmanaged[Stdcall]<IMFMediaBuffer*, byte**, uint*, uint*, int>)this.lpVtbl[3])(
                    self,
                    dataLocal,
                    maximumLengthLocal,
                    currentLengthLocal);
            }
        }

        public int Unlock()
        {
            fixed (IMFMediaBuffer* self = &this)
            {
                return ((delegate* unmanaged[Stdcall]<IMFMediaBuffer*, int>)this.lpVtbl[4])(self);
            }
        }
    }

    [Guid("279A808D-AEC7-40C8-9C6B-A6B492C78A66")]
    [StructLayout(LayoutKind.Sequential)]
    public struct IMFMediaSource
    {
        private void** lpVtbl;

        public int Shutdown()
        {
            fixed (IMFMediaSource* self = &this)
            {
                return ((delegate* unmanaged[Stdcall]<IMFMediaSource*, int>)this.lpVtbl[12])(self);
            }
        }
    }

    [Guid("44AE0FA8-EA31-4109-8D2E-4CAE4997C555")]
    [StructLayout(LayoutKind.Sequential)]
    public struct IMFMediaType
    {
        private void** lpVtbl;

        public int GetUINT32(in Guid guidKey, out uint value)
        {
            fixed (IMFMediaType* self = &this)
            fixed (Guid* guidKeyLocal = &guidKey)
            fixed (uint* valueLocal = &value)
            {
                return ((delegate* unmanaged[Stdcall]<IMFMediaType*, Guid*, uint*, int>)this.lpVtbl[7])(
                    self,
                    guidKeyLocal,
                    valueLocal);
            }
        }

        public int GetUINT64(in Guid guidKey, out ulong value)
        {
            fixed (IMFMediaType* self = &this)
            fixed (Guid* guidKeyLocal = &guidKey)
            fixed (ulong* valueLocal = &value)
            {
                return ((delegate* unmanaged[Stdcall]<IMFMediaType*, Guid*, ulong*, int>)this.lpVtbl[8])(
                    self,
                    guidKeyLocal,
                    valueLocal);
            }
        }

        public int GetGUID(in Guid guidKey, out Guid value)
        {
            fixed (IMFMediaType* self = &this)
            fixed (Guid* guidKeyLocal = &guidKey)
            fixed (Guid* valueLocal = &value)
            {
                return ((delegate* unmanaged[Stdcall]<IMFMediaType*, Guid*, Guid*, int>)this.lpVtbl[10])(
                    self,
                    guidKeyLocal,
                    valueLocal);
            }
        }
    }

    [Guid("C40A00F2-B93A-4D80-AE8C-5A1C634F58E4")]
    [StructLayout(LayoutKind.Sequential)]
    public struct IMFSample
    {
        private void** lpVtbl;

        public int ConvertToContiguousBuffer(out IMFMediaBuffer* buffer)
        {
            fixed (IMFSample* self = &this)
            fixed (IMFMediaBuffer** bufferLocal = &buffer)
            {
                return ((delegate* unmanaged[Stdcall]<IMFSample*, IMFMediaBuffer**, int>)this.lpVtbl[41])(
                    self,
                    bufferLocal);
            }
        }
    }

    [Guid("70AE66F2-C809-4E4F-8915-BDCB406B7993")]
    [StructLayout(LayoutKind.Sequential)]
    public struct IMFSourceReader
    {
        private void** lpVtbl;

        public int SetStreamSelection(uint streamIndex, bool selected)
        {
            fixed (IMFSourceReader* self = &this)
            {
                return ((delegate* unmanaged[Stdcall]<IMFSourceReader*, uint, int, int>)this.lpVtbl[4])(
                    self,
                    streamIndex,
                    selected ? 1 : 0);
            }
        }

        public int GetNativeMediaType(uint streamIndex, uint mediaTypeIndex, out IMFMediaType* mediaType)
        {
            fixed (IMFSourceReader* self = &this)
            fixed (IMFMediaType** mediaTypeLocal = &mediaType)
            {
                return ((delegate* unmanaged[Stdcall]<IMFSourceReader*, uint, uint, IMFMediaType**, int>)this.lpVtbl[5])(
                    self,
                    streamIndex,
                    mediaTypeIndex,
                    mediaTypeLocal);
            }
        }

        public int SetCurrentMediaType(uint streamIndex, IMFMediaType* mediaType)
        {
            fixed (IMFSourceReader* self = &this)
            {
                return ((delegate* unmanaged[Stdcall]<IMFSourceReader*, uint, uint*, IMFMediaType*, int>)this.lpVtbl[7])(
                    self,
                    streamIndex,
                    null,
                    mediaType);
            }
        }

        public int ReadSample(
            uint streamIndex,
            uint controlFlags,
            uint* actualStreamIndex,
            uint* streamFlags,
            long* timestamp,
            IMFSample** sample)
        {
            fixed (IMFSourceReader* self = &this)
            {
                return ((delegate* unmanaged[Stdcall]<IMFSourceReader*, uint, uint, uint*, uint*, long*, IMFSample**, int>)this.lpVtbl[9])(
                    self,
                    streamIndex,
                    controlFlags,
                    actualStreamIndex,
                    streamFlags,
                    timestamp,
                    sample);
            }
        }

        public int ReadSample(
            uint streamIndex,
            uint controlFlags,
            out uint actualStreamIndex,
            out uint streamFlags,
            out long timestamp,
            out IMFSample* sample)
        {
            fixed (uint* actualStreamIndexLocal = &actualStreamIndex)
            fixed (uint* streamFlagsLocal = &streamFlags)
            fixed (long* timestampLocal = &timestamp)
            fixed (IMFSample** sampleLocal = &sample)
            {
                return this.ReadSample(
                    streamIndex,
                    controlFlags,
                    actualStreamIndexLocal,
                    streamFlagsLocal,
                    timestampLocal,
                    sampleLocal);
            }
        }

        public int ReadSample(uint streamIndex, uint controlFlags) =>
            this.ReadSample(streamIndex, controlFlags, null, null, null, null);

        public int Flush(uint streamIndex)
        {
            fixed (IMFSourceReader* self = &this)
            {
                return ((delegate* unmanaged[Stdcall]<IMFSourceReader*, uint, int>)this.lpVtbl[10])(
                    self,
                    streamIndex);
            }
        }
    }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
#endif
    [DllImport("MFPlat.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern int MFCreateAttributes(IMFAttributes** attributes, uint initialSize);

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
#endif
    [DllImport("MFPlat.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern int MFStartup(uint version, uint flags);

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
#endif
    [DllImport("MFPlat.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern int MFShutdown();

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
#endif
    [DllImport("MFReadWrite.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern int MFCreateSourceReaderFromMediaSource(
        IMFMediaSource* mediaSource,
        IMFAttributes* attributes,
        IMFSourceReader** sourceReader);

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
#endif
    [DllImport("MF.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern int MFEnumDeviceSources(
        IMFAttributes* attributes,
        out IMFActivate** devices,
        out uint count);

    public static IntPtr GetComInterfaceForObject(IMFSourceReaderCallbackInterop callback)
    {
#if NET8_0_OR_GREATER
        return ComWrappers.GetOrCreateComInterfaceForObject(callback, CreateComInterfaceFlags.None);
#elif NETSTANDARD1_3
        return Marshal.GetComInterfaceForObject<
            IMFSourceReaderCallbackInterop,
            IMFSourceReaderCallbackInterop>(callback);
#else
        return Marshal.GetComInterfaceForObject(callback, typeof(IMFSourceReaderCallbackInterop));
#endif
    }

    public static void ReleaseComInterface(IntPtr pointer)
    {
        if (pointer != IntPtr.Zero)
        {
            _ = ((IUnknown*)pointer)->Release();
        }
    }

    public static void Release<T>(T* value)
        where T : unmanaged
    {
        if (value is not null)
        {
            _ = ((IUnknown*)value)->Release();
        }
    }

#if NET8_0_OR_GREATER
    private static readonly StrategyBasedComWrappers ComWrappers = new();
#endif
}

// The runtime COM marshaler requires this callback interface to be publicly visible.
// Keep the inbound raw COM definitions in the internal NativeMethods container.
#if NET8_0_OR_GREATER
[GeneratedComInterface]
#else
[ComImport]
[ComVisible(true)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
#endif
[Guid("DEEC8D99-FA1D-4D82-84C2-2C8969944867")]
public partial interface IMFSourceReaderCallbackInterop
{
    [PreserveSig]
    int OnReadSample(int status, uint streamIndex, uint streamFlags, long timestamp, IntPtr sample);

    [PreserveSig]
    int OnFlush(uint streamIndex);

    [PreserveSig]
    int OnEvent(uint streamIndex, IntPtr mediaEvent);
}
#endif
