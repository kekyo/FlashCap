// This is auto generated code by FlashCap.V4L2Generator [0.14.6]. Do not edit.
// Linux version 5.10.102.1-microsoft-standard-WSL2 (oe-user@oe-host) (x86_64-msft-linux-gcc (GCC) 9.3.0, GNU ld (GNU Binutils) 2.34.0.20200220) #1 SMP Wed Mar 2 00:30:59 UTC 2022
// Ubuntu clang version 11.1.0-6
// gcc version 11.4.0 (Ubuntu 11.4.0-1ubuntu1~22.04) 
// Fri, 22 Mar 2024 10:16:40 GMT

using System;
using System.Runtime.InteropServices;

namespace FlashCap.Internal.V4L2
{
    internal abstract partial class NativeMethods_V4L2_Interop
    {
        // Common
        public abstract string Label { get; }
        public abstract string Architecture { get; }
        public virtual string ClangVersion => throw new NotImplementedException();
        public virtual string GccVersion => throw new NotImplementedException();
        public abstract int sizeof_size_t { get; }
        public abstract int sizeof_off_t { get; }

        // Definitions
        public virtual uint V4L2_CAP_ASYNCIO => throw new NotImplementedException();
        public virtual uint V4L2_CAP_AUDIO => throw new NotImplementedException();
        public virtual uint V4L2_CAP_DEVICE_CAPS => throw new NotImplementedException();
        public virtual uint V4L2_CAP_EXT_PIX_FORMAT => throw new NotImplementedException();
        public virtual uint V4L2_CAP_HW_FREQ_SEEK => throw new NotImplementedException();
        public virtual uint V4L2_CAP_IO_MC => throw new NotImplementedException();
        public virtual uint V4L2_CAP_META_CAPTURE => throw new NotImplementedException();
        public virtual uint V4L2_CAP_META_OUTPUT => throw new NotImplementedException();
        public virtual uint V4L2_CAP_MODULATOR => throw new NotImplementedException();
        public virtual uint V4L2_CAP_RADIO => throw new NotImplementedException();
        public virtual uint V4L2_CAP_RDS_CAPTURE => throw new NotImplementedException();
        public virtual uint V4L2_CAP_RDS_OUTPUT => throw new NotImplementedException();
        public virtual uint V4L2_CAP_READWRITE => throw new NotImplementedException();
        public virtual uint V4L2_CAP_SDR_CAPTURE => throw new NotImplementedException();
        public virtual uint V4L2_CAP_SDR_OUTPUT => throw new NotImplementedException();
        public virtual uint V4L2_CAP_SLICED_VBI_CAPTURE => throw new NotImplementedException();
        public virtual uint V4L2_CAP_SLICED_VBI_OUTPUT => throw new NotImplementedException();
        public virtual uint V4L2_CAP_STREAMING => throw new NotImplementedException();
        public virtual uint V4L2_CAP_TIMEPERFRAME => throw new NotImplementedException();
        public virtual uint V4L2_CAP_TOUCH => throw new NotImplementedException();
        public virtual uint V4L2_CAP_TUNER => throw new NotImplementedException();
        public virtual uint V4L2_CAP_VBI_CAPTURE => throw new NotImplementedException();
        public virtual uint V4L2_CAP_VBI_OUTPUT => throw new NotImplementedException();
        public virtual uint V4L2_CAP_VIDEO_CAPTURE => throw new NotImplementedException();
        public virtual uint V4L2_CAP_VIDEO_CAPTURE_MPLANE => throw new NotImplementedException();
        public virtual uint V4L2_CAP_VIDEO_M2M => throw new NotImplementedException();
        public virtual uint V4L2_CAP_VIDEO_M2M_MPLANE => throw new NotImplementedException();
        public virtual uint V4L2_CAP_VIDEO_OUTPUT => throw new NotImplementedException();
        public virtual uint V4L2_CAP_VIDEO_OUTPUT_MPLANE => throw new NotImplementedException();
        public virtual uint V4L2_CAP_VIDEO_OUTPUT_OVERLAY => throw new NotImplementedException();
        public virtual uint V4L2_CAP_VIDEO_OVERLAY => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_ABGR32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_ABGR444 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_ABGR555 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_ARGB32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_ARGB444 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_ARGB555 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_ARGB555X => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_AYUV32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_BGR24 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_BGR32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_BGR666 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_BGRA32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_BGRA444 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_BGRA555 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_BGRX32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_BGRX444 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_BGRX555 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_CIT_YYVYUY => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_CNF4 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_CPIA1 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_DV => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_ET61X251 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_FLAG_PREMUL_ALPHA => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_FLAG_SET_CSC => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_FWHT => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_FWHT_STATELESS => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_GREY => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_H263 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_H264 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_H264_MVC => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_H264_NO_SC => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_H264_SLICE => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_HEVC => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_HI240 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_HM12 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_HSV24 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_HSV32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_INZI => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_IPU3_SBGGR10 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_IPU3_SGBRG10 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_IPU3_SGRBG10 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_IPU3_SRGGB10 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_JL2005BCD => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_JPEG => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_JPGL => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_KONICA420 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_M420 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_MJPEG => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_MPEG => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_MPEG1 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_MPEG2 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_MPEG2_SLICE => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_MPEG4 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_MR97310A => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_MT21C => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_NV12 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_NV12M => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_NV12MT => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_NV12MT_16X16 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_NV16 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_NV16M => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_NV21 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_NV21M => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_NV24 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_NV42 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_NV61 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_NV61M => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_OV511 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_OV518 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_PAC207 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_PAL8 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_PJPG => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_PRIV_MAGIC => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_PWC1 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_PWC2 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_RGB24 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_RGB32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_RGB332 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_RGB444 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_RGB555 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_RGB555X => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_RGB565 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_RGB565X => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_RGBA32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_RGBA444 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_RGBA555 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_RGBX32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_RGBX444 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_RGBX555 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_S5C_UYVY_JPG => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SBGGR10 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SBGGR10ALAW8 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SBGGR10DPCM8 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SBGGR10P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SBGGR12 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SBGGR12P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SBGGR14 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SBGGR14P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SBGGR16 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SBGGR8 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SE401 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGBRG10 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGBRG10ALAW8 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGBRG10DPCM8 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGBRG10P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGBRG12 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGBRG12P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGBRG14 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGBRG14P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGBRG16 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGBRG8 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGRBG10 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGRBG10ALAW8 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGRBG10DPCM8 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGRBG10P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGRBG12 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGRBG12P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGRBG14 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGRBG14P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGRBG16 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SGRBG8 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SN9C10X => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SN9C2028 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SN9C20X_I420 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SPCA501 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SPCA505 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SPCA508 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SPCA561 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SQ905C => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SRGGB10 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SRGGB10ALAW8 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SRGGB10DPCM8 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SRGGB10P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SRGGB12 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SRGGB12P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SRGGB14 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SRGGB14P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SRGGB16 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SRGGB8 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_STV0680 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_SUNXI_TILED_NV12 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_TM6000 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_UV8 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_UYVY => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_VC1_ANNEX_G => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_VC1_ANNEX_L => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_VP8 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_VP8_FRAME => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_VP9 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_VUYA32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_VUYX32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_VYUY => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_WNVA => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_XBGR32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_XBGR444 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_XBGR555 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_XRGB32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_XRGB444 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_XRGB555 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_XRGB555X => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_XVID => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_XYUV32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_Y10 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_Y10BPACK => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_Y10P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_Y12 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_Y12I => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_Y14 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_Y16 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_Y16_BE => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_Y4 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_Y41P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_Y6 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_Y8I => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YUV24 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YUV32 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YUV410 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YUV411P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YUV420 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YUV420M => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YUV422M => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YUV422P => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YUV444 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YUV444M => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YUV555 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YUV565 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YUYV => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YVU410 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YVU420 => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YVU420M => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YVU422M => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YVU444M => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YVYU => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_YYUV => throw new NotImplementedException();
        public virtual uint V4L2_PIX_FMT_Z16 => throw new NotImplementedException();
        public virtual uint VIDIOC_CREATE_BUFS => throw new NotImplementedException();
        public virtual uint VIDIOC_CROPCAP => throw new NotImplementedException();
        public virtual uint VIDIOC_DBG_G_CHIP_INFO => throw new NotImplementedException();
        public virtual uint VIDIOC_DBG_G_REGISTER => throw new NotImplementedException();
        public virtual uint VIDIOC_DBG_S_REGISTER => throw new NotImplementedException();
        public virtual uint VIDIOC_DECODER_CMD => throw new NotImplementedException();
        public virtual uint VIDIOC_DQBUF => throw new NotImplementedException();
        public virtual uint VIDIOC_DQEVENT => throw new NotImplementedException();
        public virtual uint VIDIOC_DV_TIMINGS_CAP => throw new NotImplementedException();
        public virtual uint VIDIOC_ENCODER_CMD => throw new NotImplementedException();
        public virtual uint VIDIOC_ENUM_DV_TIMINGS => throw new NotImplementedException();
        public virtual uint VIDIOC_ENUM_FMT => throw new NotImplementedException();
        public virtual uint VIDIOC_ENUM_FRAMEINTERVALS => throw new NotImplementedException();
        public virtual uint VIDIOC_ENUM_FRAMESIZES => throw new NotImplementedException();
        public virtual uint VIDIOC_ENUM_FREQ_BANDS => throw new NotImplementedException();
        public virtual uint VIDIOC_ENUMAUDIO => throw new NotImplementedException();
        public virtual uint VIDIOC_ENUMAUDOUT => throw new NotImplementedException();
        public virtual uint VIDIOC_ENUMINPUT => throw new NotImplementedException();
        public virtual uint VIDIOC_ENUMOUTPUT => throw new NotImplementedException();
        public virtual uint VIDIOC_ENUMSTD => throw new NotImplementedException();
        public virtual uint VIDIOC_EXPBUF => throw new NotImplementedException();
        public virtual uint VIDIOC_G_AUDIO => throw new NotImplementedException();
        public virtual uint VIDIOC_G_AUDOUT => throw new NotImplementedException();
        public virtual uint VIDIOC_G_CROP => throw new NotImplementedException();
        public virtual uint VIDIOC_G_CTRL => throw new NotImplementedException();
        public virtual uint VIDIOC_G_DV_TIMINGS => throw new NotImplementedException();
        public virtual uint VIDIOC_G_EDID => throw new NotImplementedException();
        public virtual uint VIDIOC_G_ENC_INDEX => throw new NotImplementedException();
        public virtual uint VIDIOC_G_EXT_CTRLS => throw new NotImplementedException();
        public virtual uint VIDIOC_G_FBUF => throw new NotImplementedException();
        public virtual uint VIDIOC_G_FMT => throw new NotImplementedException();
        public virtual uint VIDIOC_G_FREQUENCY => throw new NotImplementedException();
        public virtual uint VIDIOC_G_INPUT => throw new NotImplementedException();
        public virtual uint VIDIOC_G_JPEGCOMP => throw new NotImplementedException();
        public virtual uint VIDIOC_G_MODULATOR => throw new NotImplementedException();
        public virtual uint VIDIOC_G_OUTPUT => throw new NotImplementedException();
        public virtual uint VIDIOC_G_PARM => throw new NotImplementedException();
        public virtual uint VIDIOC_G_PRIORITY => throw new NotImplementedException();
        public virtual uint VIDIOC_G_SELECTION => throw new NotImplementedException();
        public virtual uint VIDIOC_G_SLICED_VBI_CAP => throw new NotImplementedException();
        public virtual uint VIDIOC_G_STD => throw new NotImplementedException();
        public virtual uint VIDIOC_G_TUNER => throw new NotImplementedException();
        public virtual uint VIDIOC_LOG_STATUS => throw new NotImplementedException();
        public virtual uint VIDIOC_OVERLAY => throw new NotImplementedException();
        public virtual uint VIDIOC_PREPARE_BUF => throw new NotImplementedException();
        public virtual uint VIDIOC_QBUF => throw new NotImplementedException();
        public virtual uint VIDIOC_QUERY_DV_TIMINGS => throw new NotImplementedException();
        public virtual uint VIDIOC_QUERY_EXT_CTRL => throw new NotImplementedException();
        public virtual uint VIDIOC_QUERYBUF => throw new NotImplementedException();
        public virtual uint VIDIOC_QUERYCAP => throw new NotImplementedException();
        public virtual uint VIDIOC_QUERYCTRL => throw new NotImplementedException();
        public virtual uint VIDIOC_QUERYMENU => throw new NotImplementedException();
        public virtual uint VIDIOC_QUERYSTD => throw new NotImplementedException();
        public virtual uint VIDIOC_REQBUFS => throw new NotImplementedException();
        public virtual uint VIDIOC_S_AUDIO => throw new NotImplementedException();
        public virtual uint VIDIOC_S_AUDOUT => throw new NotImplementedException();
        public virtual uint VIDIOC_S_CROP => throw new NotImplementedException();
        public virtual uint VIDIOC_S_CTRL => throw new NotImplementedException();
        public virtual uint VIDIOC_S_DV_TIMINGS => throw new NotImplementedException();
        public virtual uint VIDIOC_S_EDID => throw new NotImplementedException();
        public virtual uint VIDIOC_S_EXT_CTRLS => throw new NotImplementedException();
        public virtual uint VIDIOC_S_FBUF => throw new NotImplementedException();
        public virtual uint VIDIOC_S_FMT => throw new NotImplementedException();
        public virtual uint VIDIOC_S_FREQUENCY => throw new NotImplementedException();
        public virtual uint VIDIOC_S_HW_FREQ_SEEK => throw new NotImplementedException();
        public virtual uint VIDIOC_S_INPUT => throw new NotImplementedException();
        public virtual uint VIDIOC_S_JPEGCOMP => throw new NotImplementedException();
        public virtual uint VIDIOC_S_MODULATOR => throw new NotImplementedException();
        public virtual uint VIDIOC_S_OUTPUT => throw new NotImplementedException();
        public virtual uint VIDIOC_S_PARM => throw new NotImplementedException();
        public virtual uint VIDIOC_S_PRIORITY => throw new NotImplementedException();
        public virtual uint VIDIOC_S_SELECTION => throw new NotImplementedException();
        public virtual uint VIDIOC_S_STD => throw new NotImplementedException();
        public virtual uint VIDIOC_S_TUNER => throw new NotImplementedException();
        public virtual uint VIDIOC_STREAMOFF => throw new NotImplementedException();
        public virtual uint VIDIOC_STREAMON => throw new NotImplementedException();
        public virtual uint VIDIOC_SUBSCRIBE_EVENT => throw new NotImplementedException();
        public virtual uint VIDIOC_TRY_DECODER_CMD => throw new NotImplementedException();
        public virtual uint VIDIOC_TRY_ENCODER_CMD => throw new NotImplementedException();
        public virtual uint VIDIOC_TRY_EXT_CTRLS => throw new NotImplementedException();
        public virtual uint VIDIOC_TRY_FMT => throw new NotImplementedException();
        public virtual uint VIDIOC_UNSUBSCRIBE_EVENT => throw new NotImplementedException();

        // Enums
        public enum v4l2_buf_type
        {
            VIDEO_CAPTURE = 1,
            VIDEO_OUTPUT = 2,
            VIDEO_OVERLAY = 3,
            VBI_CAPTURE = 4,
            VBI_OUTPUT = 5,
            SLICED_VBI_CAPTURE = 6,
            SLICED_VBI_OUTPUT = 7,
            VIDEO_OUTPUT_OVERLAY = 8,
            VIDEO_CAPTURE_MPLANE = 9,
            VIDEO_OUTPUT_MPLANE = 10,
            SDR_CAPTURE = 11,
            SDR_OUTPUT = 12,
            META_CAPTURE = 13,
            META_OUTPUT = 14,
            PRIVATE = 128,
        }

        public enum v4l2_field
        {
            ANY = 0,
            NONE = 1,
            TOP = 2,
            BOTTOM = 3,
            INTERLACED = 4,
            SEQ_TB = 5,
            SEQ_BT = 6,
            ALTERNATE = 7,
            INTERLACED_TB = 8,
            INTERLACED_BT = 9,
        }

        public enum v4l2_frmivaltypes
        {
            DISCRETE = 1,
            CONTINUOUS = 2,
            STEPWISE = 3,
        }

        public enum v4l2_frmsizetypes
        {
            DISCRETE = 1,
            CONTINUOUS = 2,
            STEPWISE = 3,
        }

        public enum v4l2_memory
        {
            MMAP = 1,
            USERPTR = 2,
            OVERLAY = 3,
            DMABUF = 4,
        }


        // Structures
        public interface timespec
        {
            IntPtr tv_sec
            {
                get;
                set;
            }

            IntPtr tv_nsec
            {
                get;
                set;
            }

        }
        public virtual timespec Create_timespec() => throw new NotImplementedException();

        public interface timeval
        {
            IntPtr tv_sec
            {
                get;
                set;
            }

            IntPtr tv_usec
            {
                get;
                set;
            }

        }
        public virtual timeval Create_timeval() => throw new NotImplementedException();

        public interface v4l2_buffer
        {
            uint index
            {
                get;
                set;
            }

            uint type
            {
                get;
                set;
            }

            uint bytesused
            {
                get;
                set;
            }

            uint flags
            {
                get;
                set;
            }

            uint field
            {
                get;
                set;
            }

            timeval timestamp
            {
                get;
                set;
            }

            v4l2_timecode timecode
            {
                get;
                set;
            }

            uint sequence
            {
                get;
                set;
            }

            uint memory
            {
                get;
                set;
            }

            uint m_offset
            {
                get;
                set;
            }

            UIntPtr m_userptr
            {
                get;
                set;
            }

            IntPtr m_planes
            {
                get;
                set;
            }

            int m_fd
            {
                get;
                set;
            }

            uint length
            {
                get;
                set;
            }

            uint reserved2
            {
                get;
                set;
            }

            int request_fd
            {
                get;
                set;
            }

            uint reserved
            {
                get;
                set;
            }

        }
        public virtual v4l2_buffer Create_v4l2_buffer() => throw new NotImplementedException();

        public interface v4l2_capability
        {
            byte[] driver
            {
                get;
                set;
            }

            byte[] card
            {
                get;
                set;
            }

            byte[] bus_info
            {
                get;
                set;
            }

            uint version
            {
                get;
                set;
            }

            uint capabilities
            {
                get;
                set;
            }

            uint device_caps
            {
                get;
                set;
            }

            uint[] reserved
            {
                get;
                set;
            }

        }
        public virtual v4l2_capability Create_v4l2_capability() => throw new NotImplementedException();

        public interface v4l2_clip
        {
            v4l2_rect c
            {
                get;
                set;
            }

            IntPtr next
            {
                get;
                set;
            }

        }
        public virtual v4l2_clip Create_v4l2_clip() => throw new NotImplementedException();

        public interface v4l2_fmtdesc
        {
            uint index
            {
                get;
                set;
            }

            uint type
            {
                get;
                set;
            }

            uint flags
            {
                get;
                set;
            }

            byte[] description
            {
                get;
                set;
            }

            uint pixelformat
            {
                get;
                set;
            }

            uint mbus_code
            {
                get;
                set;
            }

            uint[] reserved
            {
                get;
                set;
            }

        }
        public virtual v4l2_fmtdesc Create_v4l2_fmtdesc() => throw new NotImplementedException();

        public interface v4l2_format
        {
            uint type
            {
                get;
                set;
            }

            v4l2_pix_format fmt_pix
            {
                get;
                set;
            }

            v4l2_pix_format_mplane fmt_pix_mp
            {
                get;
                set;
            }

            v4l2_window fmt_win
            {
                get;
                set;
            }

            v4l2_vbi_format fmt_vbi
            {
                get;
                set;
            }

            v4l2_sliced_vbi_format fmt_sliced
            {
                get;
                set;
            }

            v4l2_sdr_format fmt_sdr
            {
                get;
                set;
            }

            v4l2_meta_format fmt_meta
            {
                get;
                set;
            }

            byte[] fmt_raw_data
            {
                get;
                set;
            }

        }
        public virtual v4l2_format Create_v4l2_format() => throw new NotImplementedException();

        public interface v4l2_fract
        {
            uint numerator
            {
                get;
                set;
            }

            uint denominator
            {
                get;
                set;
            }

        }
        public virtual v4l2_fract Create_v4l2_fract() => throw new NotImplementedException();

        public interface v4l2_frmival_stepwise
        {
            v4l2_fract min
            {
                get;
                set;
            }

            v4l2_fract max
            {
                get;
                set;
            }

            v4l2_fract step
            {
                get;
                set;
            }

        }
        public virtual v4l2_frmival_stepwise Create_v4l2_frmival_stepwise() => throw new NotImplementedException();

        public interface v4l2_frmivalenum
        {
            uint index
            {
                get;
                set;
            }

            uint pixel_format
            {
                get;
                set;
            }

            uint width
            {
                get;
                set;
            }

            uint height
            {
                get;
                set;
            }

            uint type
            {
                get;
                set;
            }

            v4l2_fract discrete
            {
                get;
                set;
            }

            v4l2_frmival_stepwise stepwise
            {
                get;
                set;
            }

            uint[] reserved
            {
                get;
                set;
            }

        }
        public virtual v4l2_frmivalenum Create_v4l2_frmivalenum() => throw new NotImplementedException();

        public interface v4l2_frmsize_discrete
        {
            uint width
            {
                get;
                set;
            }

            uint height
            {
                get;
                set;
            }

        }
        public virtual v4l2_frmsize_discrete Create_v4l2_frmsize_discrete() => throw new NotImplementedException();

        public interface v4l2_frmsize_stepwise
        {
            uint min_width
            {
                get;
                set;
            }

            uint max_width
            {
                get;
                set;
            }

            uint step_width
            {
                get;
                set;
            }

            uint min_height
            {
                get;
                set;
            }

            uint max_height
            {
                get;
                set;
            }

            uint step_height
            {
                get;
                set;
            }

        }
        public virtual v4l2_frmsize_stepwise Create_v4l2_frmsize_stepwise() => throw new NotImplementedException();

        public interface v4l2_frmsizeenum
        {
            uint index
            {
                get;
                set;
            }

            uint pixel_format
            {
                get;
                set;
            }

            uint type
            {
                get;
                set;
            }

            v4l2_frmsize_discrete discrete
            {
                get;
                set;
            }

            v4l2_frmsize_stepwise stepwise
            {
                get;
                set;
            }

            uint[] reserved
            {
                get;
                set;
            }

        }
        public virtual v4l2_frmsizeenum Create_v4l2_frmsizeenum() => throw new NotImplementedException();

        public interface v4l2_meta_format
        {
            uint dataformat
            {
                get;
                set;
            }

            uint buffersize
            {
                get;
                set;
            }

        }
        public virtual v4l2_meta_format Create_v4l2_meta_format() => throw new NotImplementedException();

        public interface v4l2_pix_format
        {
            uint width
            {
                get;
                set;
            }

            uint height
            {
                get;
                set;
            }

            uint pixelformat
            {
                get;
                set;
            }

            uint field
            {
                get;
                set;
            }

            uint bytesperline
            {
                get;
                set;
            }

            uint sizeimage
            {
                get;
                set;
            }

            uint colorspace
            {
                get;
                set;
            }

            uint priv
            {
                get;
                set;
            }

            uint flags
            {
                get;
                set;
            }

            uint ycbcr_enc
            {
                get;
                set;
            }

            uint hsv_enc
            {
                get;
                set;
            }

            uint quantization
            {
                get;
                set;
            }

            uint xfer_func
            {
                get;
                set;
            }

        }
        public virtual v4l2_pix_format Create_v4l2_pix_format() => throw new NotImplementedException();

        public interface v4l2_pix_format_mplane
        {
            uint width
            {
                get;
                set;
            }

            uint height
            {
                get;
                set;
            }

            uint pixelformat
            {
                get;
                set;
            }

            uint field
            {
                get;
                set;
            }

            uint colorspace
            {
                get;
                set;
            }

            v4l2_plane_pix_format[] plane_fmt
            {
                get;
                set;
            }

            byte num_planes
            {
                get;
                set;
            }

            byte flags
            {
                get;
                set;
            }

            byte ycbcr_enc
            {
                get;
                set;
            }

            byte hsv_enc
            {
                get;
                set;
            }

            byte quantization
            {
                get;
                set;
            }

            byte xfer_func
            {
                get;
                set;
            }

            byte[] reserved
            {
                get;
                set;
            }

        }
        public virtual v4l2_pix_format_mplane Create_v4l2_pix_format_mplane() => throw new NotImplementedException();

        public interface v4l2_plane
        {
            uint bytesused
            {
                get;
                set;
            }

            uint length
            {
                get;
                set;
            }

            uint m_mem_offset
            {
                get;
                set;
            }

            UIntPtr m_userptr
            {
                get;
                set;
            }

            int m_fd
            {
                get;
                set;
            }

            uint data_offset
            {
                get;
                set;
            }

            uint[] reserved
            {
                get;
                set;
            }

        }
        public virtual v4l2_plane Create_v4l2_plane() => throw new NotImplementedException();

        public interface v4l2_plane_pix_format
        {
            uint sizeimage
            {
                get;
                set;
            }

            uint bytesperline
            {
                get;
                set;
            }

            ushort[] reserved
            {
                get;
                set;
            }

        }
        public virtual v4l2_plane_pix_format Create_v4l2_plane_pix_format() => throw new NotImplementedException();

        public interface v4l2_rect
        {
            int left
            {
                get;
                set;
            }

            int top
            {
                get;
                set;
            }

            uint width
            {
                get;
                set;
            }

            uint height
            {
                get;
                set;
            }

        }
        public virtual v4l2_rect Create_v4l2_rect() => throw new NotImplementedException();

        public interface v4l2_requestbuffers
        {
            uint count
            {
                get;
                set;
            }

            uint type
            {
                get;
                set;
            }

            uint memory
            {
                get;
                set;
            }

            uint capabilities
            {
                get;
                set;
            }

            uint[] reserved
            {
                get;
                set;
            }

        }
        public virtual v4l2_requestbuffers Create_v4l2_requestbuffers() => throw new NotImplementedException();

        public interface v4l2_sdr_format
        {
            uint pixelformat
            {
                get;
                set;
            }

            uint buffersize
            {
                get;
                set;
            }

            byte[] reserved
            {
                get;
                set;
            }

        }
        public virtual v4l2_sdr_format Create_v4l2_sdr_format() => throw new NotImplementedException();

        public interface v4l2_sliced_vbi_format
        {
            ushort service_set
            {
                get;
                set;
            }

            ushort[][] service_lines
            {
                get;
                set;
            }

            uint io_size
            {
                get;
                set;
            }

            uint[] reserved
            {
                get;
                set;
            }

        }
        public virtual v4l2_sliced_vbi_format Create_v4l2_sliced_vbi_format() => throw new NotImplementedException();

        public interface v4l2_timecode
        {
            uint type
            {
                get;
                set;
            }

            uint flags
            {
                get;
                set;
            }

            byte frames
            {
                get;
                set;
            }

            byte seconds
            {
                get;
                set;
            }

            byte minutes
            {
                get;
                set;
            }

            byte hours
            {
                get;
                set;
            }

            byte[] userbits
            {
                get;
                set;
            }

        }
        public virtual v4l2_timecode Create_v4l2_timecode() => throw new NotImplementedException();

        public interface v4l2_vbi_format
        {
            uint sampling_rate
            {
                get;
                set;
            }

            uint offset
            {
                get;
                set;
            }

            uint samples_per_line
            {
                get;
                set;
            }

            uint sample_format
            {
                get;
                set;
            }

            int[] start
            {
                get;
                set;
            }

            uint[] count
            {
                get;
                set;
            }

            uint flags
            {
                get;
                set;
            }

            uint[] reserved
            {
                get;
                set;
            }

        }
        public virtual v4l2_vbi_format Create_v4l2_vbi_format() => throw new NotImplementedException();

        public interface v4l2_window
        {
            v4l2_rect w
            {
                get;
                set;
            }

            uint field
            {
                get;
                set;
            }

            uint chromakey
            {
                get;
                set;
            }

            IntPtr clips
            {
                get;
                set;
            }

            uint clipcount
            {
                get;
                set;
            }

            IntPtr bitmap
            {
                get;
                set;
            }

            byte global_alpha
            {
                get;
                set;
            }

        }
        public virtual v4l2_window Create_v4l2_window() => throw new NotImplementedException();


    }
}

