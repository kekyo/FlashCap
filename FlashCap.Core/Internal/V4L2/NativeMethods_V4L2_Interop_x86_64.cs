// This is auto generated code by FlashCap.V4L2Generator [0.14.6]. Do not edit.
// Linux version 5.10.102.1-microsoft-standard-WSL2 (oe-user@oe-host) (x86_64-msft-linux-gcc (GCC) 9.3.0, GNU ld (GNU Binutils) 2.34.0.20200220) #1 SMP Wed Mar 2 00:30:59 UTC 2022
// Ubuntu clang version 11.1.0-6
// gcc version 11.4.0 (Ubuntu 11.4.0-1ubuntu1~22.04) 
// Fri, 22 Mar 2024 10:16:40 GMT

using System;
using System.Runtime.InteropServices;

namespace FlashCap.Internal.V4L2
{
    internal sealed class NativeMethods_V4L2_Interop_x86_64 : NativeMethods_V4L2_Interop
    {
        // Common
        public override string Label => "Linux version 5.10.102.1-microsoft-standard-WSL2 (oe-user@oe-host) (x86_64-msft-linux-gcc (GCC) 9.3.0, GNU ld (GNU Binutils) 2.34.0.20200220) #1 SMP Wed Mar 2 00:30:59 UTC 2022";
        public override string Architecture => "x86_64";
        public override string ClangVersion => "Ubuntu clang version 11.1.0-6";
        public override string GccVersion => "gcc version 11.4.0 (Ubuntu 11.4.0-1ubuntu1~22.04) ";
        public override int sizeof_size_t => 8;
        public override int sizeof_off_t => 8;

        // Definitions
        public override uint V4L2_CAP_ASYNCIO => 33554432U;
        public override uint V4L2_CAP_AUDIO => 131072U;
        public override uint V4L2_CAP_DEVICE_CAPS => 2147483648U;
        public override uint V4L2_CAP_EXT_PIX_FORMAT => 2097152U;
        public override uint V4L2_CAP_HW_FREQ_SEEK => 1024U;
        public override uint V4L2_CAP_IO_MC => 536870912U;
        public override uint V4L2_CAP_META_CAPTURE => 8388608U;
        public override uint V4L2_CAP_META_OUTPUT => 134217728U;
        public override uint V4L2_CAP_MODULATOR => 524288U;
        public override uint V4L2_CAP_RADIO => 262144U;
        public override uint V4L2_CAP_RDS_CAPTURE => 256U;
        public override uint V4L2_CAP_RDS_OUTPUT => 2048U;
        public override uint V4L2_CAP_READWRITE => 16777216U;
        public override uint V4L2_CAP_SDR_CAPTURE => 1048576U;
        public override uint V4L2_CAP_SDR_OUTPUT => 4194304U;
        public override uint V4L2_CAP_SLICED_VBI_CAPTURE => 64U;
        public override uint V4L2_CAP_SLICED_VBI_OUTPUT => 128U;
        public override uint V4L2_CAP_STREAMING => 67108864U;
        public override uint V4L2_CAP_TIMEPERFRAME => 4096U;
        public override uint V4L2_CAP_TOUCH => 268435456U;
        public override uint V4L2_CAP_TUNER => 65536U;
        public override uint V4L2_CAP_VBI_CAPTURE => 16U;
        public override uint V4L2_CAP_VBI_OUTPUT => 32U;
        public override uint V4L2_CAP_VIDEO_CAPTURE => 1U;
        public override uint V4L2_CAP_VIDEO_CAPTURE_MPLANE => 4096U;
        public override uint V4L2_CAP_VIDEO_M2M => 32768U;
        public override uint V4L2_CAP_VIDEO_M2M_MPLANE => 16384U;
        public override uint V4L2_CAP_VIDEO_OUTPUT => 2U;
        public override uint V4L2_CAP_VIDEO_OUTPUT_MPLANE => 8192U;
        public override uint V4L2_CAP_VIDEO_OUTPUT_OVERLAY => 512U;
        public override uint V4L2_CAP_VIDEO_OVERLAY => 4U;
        public override uint V4L2_PIX_FMT_ABGR32 => 875713089U;
        public override uint V4L2_PIX_FMT_ABGR444 => 842089025U;
        public override uint V4L2_PIX_FMT_ABGR555 => 892420673U;
        public override uint V4L2_PIX_FMT_ARGB32 => 875708738U;
        public override uint V4L2_PIX_FMT_ARGB444 => 842093121U;
        public override uint V4L2_PIX_FMT_ARGB555 => 892424769U;
        public override uint V4L2_PIX_FMT_ARGB555X => 3039908417U;
        public override uint V4L2_PIX_FMT_AYUV32 => 1448433985U;
        public override uint V4L2_PIX_FMT_BGR24 => 861030210U;
        public override uint V4L2_PIX_FMT_BGR32 => 877807426U;
        public override uint V4L2_PIX_FMT_BGR666 => 1213351746U;
        public override uint V4L2_PIX_FMT_BGRA32 => 875708754U;
        public override uint V4L2_PIX_FMT_BGRA444 => 842088775U;
        public override uint V4L2_PIX_FMT_BGRA555 => 892420418U;
        public override uint V4L2_PIX_FMT_BGRX32 => 875714642U;
        public override uint V4L2_PIX_FMT_BGRX444 => 842094658U;
        public override uint V4L2_PIX_FMT_BGRX555 => 892426306U;
        public override uint V4L2_PIX_FMT_CIT_YYVYUY => 1448364355U;
        public override uint V4L2_PIX_FMT_CNF4 => 877022787U;
        public override uint V4L2_PIX_FMT_CPIA1 => 1095323715U;
        public override uint V4L2_PIX_FMT_DV => 1685288548U;
        public override uint V4L2_PIX_FMT_ET61X251 => 892483141U;
        public override uint V4L2_PIX_FMT_FLAG_PREMUL_ALPHA => 1U;
        public override uint V4L2_PIX_FMT_FLAG_SET_CSC => 2U;
        public override uint V4L2_PIX_FMT_FWHT => 1414027078U;
        public override uint V4L2_PIX_FMT_FWHT_STATELESS => 1213679187U;
        public override uint V4L2_PIX_FMT_GREY => 1497715271U;
        public override uint V4L2_PIX_FMT_H263 => 859189832U;
        public override uint V4L2_PIX_FMT_H264 => 875967048U;
        public override uint V4L2_PIX_FMT_H264_MVC => 875967053U;
        public override uint V4L2_PIX_FMT_H264_NO_SC => 826496577U;
        public override uint V4L2_PIX_FMT_H264_SLICE => 875967059U;
        public override uint V4L2_PIX_FMT_HEVC => 1129727304U;
        public override uint V4L2_PIX_FMT_HI240 => 875710792U;
        public override uint V4L2_PIX_FMT_HM12 => 842091848U;
        public override uint V4L2_PIX_FMT_HSV24 => 861295432U;
        public override uint V4L2_PIX_FMT_HSV32 => 878072648U;
        public override uint V4L2_PIX_FMT_INZI => 1230655049U;
        public override uint V4L2_PIX_FMT_IPU3_SBGGR10 => 1647538281U;
        public override uint V4L2_PIX_FMT_IPU3_SGBRG10 => 1731424361U;
        public override uint V4L2_PIX_FMT_IPU3_SGRBG10 => 1194553449U;
        public override uint V4L2_PIX_FMT_IPU3_SRGGB10 => 1915973737U;
        public override uint V4L2_PIX_FMT_JL2005BCD => 808602698U;
        public override uint V4L2_PIX_FMT_JPEG => 1195724874U;
        public override uint V4L2_PIX_FMT_JPGL => 1279742026U;
        public override uint V4L2_PIX_FMT_KONICA420 => 1229868875U;
        public override uint V4L2_PIX_FMT_M420 => 808596557U;
        public override uint V4L2_PIX_FMT_MJPEG => 1196444237U;
        public override uint V4L2_PIX_FMT_MPEG => 1195724877U;
        public override uint V4L2_PIX_FMT_MPEG1 => 826757197U;
        public override uint V4L2_PIX_FMT_MPEG2 => 843534413U;
        public override uint V4L2_PIX_FMT_MPEG2_SLICE => 1395803981U;
        public override uint V4L2_PIX_FMT_MPEG4 => 877088845U;
        public override uint V4L2_PIX_FMT_MR97310A => 808530765U;
        public override uint V4L2_PIX_FMT_MT21C => 825381965U;
        public override uint V4L2_PIX_FMT_NV12 => 842094158U;
        public override uint V4L2_PIX_FMT_NV12M => 842091854U;
        public override uint V4L2_PIX_FMT_NV12MT => 842091860U;
        public override uint V4L2_PIX_FMT_NV12MT_16X16 => 842091862U;
        public override uint V4L2_PIX_FMT_NV16 => 909203022U;
        public override uint V4L2_PIX_FMT_NV16M => 909200718U;
        public override uint V4L2_PIX_FMT_NV21 => 825382478U;
        public override uint V4L2_PIX_FMT_NV21M => 825380174U;
        public override uint V4L2_PIX_FMT_NV24 => 875714126U;
        public override uint V4L2_PIX_FMT_NV42 => 842290766U;
        public override uint V4L2_PIX_FMT_NV61 => 825644622U;
        public override uint V4L2_PIX_FMT_NV61M => 825642318U;
        public override uint V4L2_PIX_FMT_OV511 => 825308495U;
        public override uint V4L2_PIX_FMT_OV518 => 942749007U;
        public override uint V4L2_PIX_FMT_PAC207 => 925905488U;
        public override uint V4L2_PIX_FMT_PAL8 => 944521552U;
        public override uint V4L2_PIX_FMT_PJPG => 1196444240U;
        public override uint V4L2_PIX_FMT_PRIV_MAGIC => 4276996862U;
        public override uint V4L2_PIX_FMT_PWC1 => 826496848U;
        public override uint V4L2_PIX_FMT_PWC2 => 843274064U;
        public override uint V4L2_PIX_FMT_RGB24 => 859981650U;
        public override uint V4L2_PIX_FMT_RGB32 => 876758866U;
        public override uint V4L2_PIX_FMT_RGB332 => 826427218U;
        public override uint V4L2_PIX_FMT_RGB444 => 875836498U;
        public override uint V4L2_PIX_FMT_RGB555 => 1329743698U;
        public override uint V4L2_PIX_FMT_RGB555X => 1363298130U;
        public override uint V4L2_PIX_FMT_RGB565 => 1346520914U;
        public override uint V4L2_PIX_FMT_RGB565X => 1380075346U;
        public override uint V4L2_PIX_FMT_RGBA32 => 875708993U;
        public override uint V4L2_PIX_FMT_RGBA444 => 842088786U;
        public override uint V4L2_PIX_FMT_RGBA555 => 892420434U;
        public override uint V4L2_PIX_FMT_RGBX32 => 875709016U;
        public override uint V4L2_PIX_FMT_RGBX444 => 842094674U;
        public override uint V4L2_PIX_FMT_RGBX555 => 892426322U;
        public override uint V4L2_PIX_FMT_S5C_UYVY_JPG => 1229141331U;
        public override uint V4L2_PIX_FMT_SBGGR10 => 808535874U;
        public override uint V4L2_PIX_FMT_SBGGR10ALAW8 => 943800929U;
        public override uint V4L2_PIX_FMT_SBGGR10DPCM8 => 943800930U;
        public override uint V4L2_PIX_FMT_SBGGR10P => 1094795888U;
        public override uint V4L2_PIX_FMT_SBGGR12 => 842090306U;
        public override uint V4L2_PIX_FMT_SBGGR12P => 1128481392U;
        public override uint V4L2_PIX_FMT_SBGGR14 => 875644738U;
        public override uint V4L2_PIX_FMT_SBGGR14P => 1162166896U;
        public override uint V4L2_PIX_FMT_SBGGR16 => 844257602U;
        public override uint V4L2_PIX_FMT_SBGGR8 => 825770306U;
        public override uint V4L2_PIX_FMT_SE401 => 825242707U;
        public override uint V4L2_PIX_FMT_SGBRG10 => 808534599U;
        public override uint V4L2_PIX_FMT_SGBRG10ALAW8 => 943802209U;
        public override uint V4L2_PIX_FMT_SGBRG10DPCM8 => 943802210U;
        public override uint V4L2_PIX_FMT_SGBRG10P => 1094797168U;
        public override uint V4L2_PIX_FMT_SGBRG12 => 842089031U;
        public override uint V4L2_PIX_FMT_SGBRG12P => 1128482672U;
        public override uint V4L2_PIX_FMT_SGBRG14 => 875643463U;
        public override uint V4L2_PIX_FMT_SGBRG14P => 1162168176U;
        public override uint V4L2_PIX_FMT_SGBRG16 => 909197895U;
        public override uint V4L2_PIX_FMT_SGBRG8 => 1196573255U;
        public override uint V4L2_PIX_FMT_SGRBG10 => 808534338U;
        public override uint V4L2_PIX_FMT_SGRBG10ALAW8 => 943810401U;
        public override uint V4L2_PIX_FMT_SGRBG10DPCM8 => 808535106U;
        public override uint V4L2_PIX_FMT_SGRBG10P => 1094805360U;
        public override uint V4L2_PIX_FMT_SGRBG12 => 842088770U;
        public override uint V4L2_PIX_FMT_SGRBG12P => 1128490864U;
        public override uint V4L2_PIX_FMT_SGRBG14 => 875647559U;
        public override uint V4L2_PIX_FMT_SGRBG14P => 1162176368U;
        public override uint V4L2_PIX_FMT_SGRBG16 => 909201991U;
        public override uint V4L2_PIX_FMT_SGRBG8 => 1195528775U;
        public override uint V4L2_PIX_FMT_SN9C10X => 808532307U;
        public override uint V4L2_PIX_FMT_SN9C2028 => 1481527123U;
        public override uint V4L2_PIX_FMT_SN9C20X_I420 => 808597843U;
        public override uint V4L2_PIX_FMT_SPCA501 => 825242963U;
        public override uint V4L2_PIX_FMT_SPCA505 => 892351827U;
        public override uint V4L2_PIX_FMT_SPCA508 => 942683475U;
        public override uint V4L2_PIX_FMT_SPCA561 => 825636179U;
        public override uint V4L2_PIX_FMT_SQ905C => 1127559225U;
        public override uint V4L2_PIX_FMT_SRGGB10 => 808535890U;
        public override uint V4L2_PIX_FMT_SRGGB10ALAW8 => 943805025U;
        public override uint V4L2_PIX_FMT_SRGGB10DPCM8 => 943805026U;
        public override uint V4L2_PIX_FMT_SRGGB10P => 1094799984U;
        public override uint V4L2_PIX_FMT_SRGGB12 => 842090322U;
        public override uint V4L2_PIX_FMT_SRGGB12P => 1128485488U;
        public override uint V4L2_PIX_FMT_SRGGB14 => 875644754U;
        public override uint V4L2_PIX_FMT_SRGGB14P => 1162170992U;
        public override uint V4L2_PIX_FMT_SRGGB16 => 909199186U;
        public override uint V4L2_PIX_FMT_SRGGB8 => 1111967570U;
        public override uint V4L2_PIX_FMT_STV0680 => 808990291U;
        public override uint V4L2_PIX_FMT_SUNXI_TILED_NV12 => 842093651U;
        public override uint V4L2_PIX_FMT_TM6000 => 808865108U;
        public override uint V4L2_PIX_FMT_UV8 => 540563029U;
        public override uint V4L2_PIX_FMT_UYVY => 1498831189U;
        public override uint V4L2_PIX_FMT_VC1_ANNEX_G => 1194410838U;
        public override uint V4L2_PIX_FMT_VC1_ANNEX_L => 1278296918U;
        public override uint V4L2_PIX_FMT_VP8 => 808996950U;
        public override uint V4L2_PIX_FMT_VP8_FRAME => 1178095702U;
        public override uint V4L2_PIX_FMT_VP9 => 809062486U;
        public override uint V4L2_PIX_FMT_VUYA32 => 1096373590U;
        public override uint V4L2_PIX_FMT_VUYX32 => 1482249558U;
        public override uint V4L2_PIX_FMT_VYUY => 1498765654U;
        public override uint V4L2_PIX_FMT_WNVA => 1096175191U;
        public override uint V4L2_PIX_FMT_XBGR32 => 875713112U;
        public override uint V4L2_PIX_FMT_XBGR444 => 842089048U;
        public override uint V4L2_PIX_FMT_XBGR555 => 892420696U;
        public override uint V4L2_PIX_FMT_XRGB32 => 875714626U;
        public override uint V4L2_PIX_FMT_XRGB444 => 842093144U;
        public override uint V4L2_PIX_FMT_XRGB555 => 892424792U;
        public override uint V4L2_PIX_FMT_XRGB555X => 3039908440U;
        public override uint V4L2_PIX_FMT_XVID => 1145656920U;
        public override uint V4L2_PIX_FMT_XYUV32 => 1448434008U;
        public override uint V4L2_PIX_FMT_Y10 => 540029273U;
        public override uint V4L2_PIX_FMT_Y10BPACK => 1110454617U;
        public override uint V4L2_PIX_FMT_Y10P => 1345335641U;
        public override uint V4L2_PIX_FMT_Y12 => 540160345U;
        public override uint V4L2_PIX_FMT_Y12I => 1228026201U;
        public override uint V4L2_PIX_FMT_Y14 => 540291417U;
        public override uint V4L2_PIX_FMT_Y16 => 540422489U;
        public override uint V4L2_PIX_FMT_Y16_BE => 2687906137U;
        public override uint V4L2_PIX_FMT_Y4 => 540291161U;
        public override uint V4L2_PIX_FMT_Y41P => 1345401945U;
        public override uint V4L2_PIX_FMT_Y6 => 540422233U;
        public override uint V4L2_PIX_FMT_Y8I => 541669465U;
        public override uint V4L2_PIX_FMT_YUV24 => 861295961U;
        public override uint V4L2_PIX_FMT_YUV32 => 878073177U;
        public override uint V4L2_PIX_FMT_YUV410 => 961959257U;
        public override uint V4L2_PIX_FMT_YUV411P => 1345401140U;
        public override uint V4L2_PIX_FMT_YUV420 => 842093913U;
        public override uint V4L2_PIX_FMT_YUV420M => 842091865U;
        public override uint V4L2_PIX_FMT_YUV422M => 909200729U;
        public override uint V4L2_PIX_FMT_YUV422P => 1345466932U;
        public override uint V4L2_PIX_FMT_YUV444 => 875836505U;
        public override uint V4L2_PIX_FMT_YUV444M => 875711833U;
        public override uint V4L2_PIX_FMT_YUV555 => 1331058009U;
        public override uint V4L2_PIX_FMT_YUV565 => 1347835225U;
        public override uint V4L2_PIX_FMT_YUYV => 1448695129U;
        public override uint V4L2_PIX_FMT_YVU410 => 961893977U;
        public override uint V4L2_PIX_FMT_YVU420 => 842094169U;
        public override uint V4L2_PIX_FMT_YVU420M => 825380185U;
        public override uint V4L2_PIX_FMT_YVU422M => 825642329U;
        public override uint V4L2_PIX_FMT_YVU444M => 842288473U;
        public override uint V4L2_PIX_FMT_YVYU => 1431918169U;
        public override uint V4L2_PIX_FMT_YYUV => 1448434009U;
        public override uint V4L2_PIX_FMT_Z16 => 540422490U;
        public override uint VIDIOC_CREATE_BUFS => 3238024796U;
        public override uint VIDIOC_CROPCAP => 3224131130U;
        public override uint VIDIOC_DBG_G_CHIP_INFO => 3234354790U;
        public override uint VIDIOC_DBG_G_REGISTER => 3224917584U;
        public override uint VIDIOC_DBG_S_REGISTER => 1077433935U;
        public override uint VIDIOC_DECODER_CMD => 3225966176U;
        public override uint VIDIOC_DQBUF => 3227014673U;
        public override uint VIDIOC_DQEVENT => 2156418649U;
        public override uint VIDIOC_DV_TIMINGS_CAP => 3230684772U;
        public override uint VIDIOC_ENCODER_CMD => 3223869005U;
        public override uint VIDIOC_ENUM_DV_TIMINGS => 3230946914U;
        public override uint VIDIOC_ENUM_FMT => 3225441794U;
        public override uint VIDIOC_ENUM_FRAMEINTERVALS => 3224655435U;
        public override uint VIDIOC_ENUM_FRAMESIZES => 3224131146U;
        public override uint VIDIOC_ENUM_FREQ_BANDS => 3225441893U;
        public override uint VIDIOC_ENUMAUDIO => 3224655425U;
        public override uint VIDIOC_ENUMAUDOUT => 3224655426U;
        public override uint VIDIOC_ENUMINPUT => 3226490394U;
        public override uint VIDIOC_ENUMOUTPUT => 3225966128U;
        public override uint VIDIOC_ENUMSTD => 3225966105U;
        public override uint VIDIOC_EXPBUF => 3225441808U;
        public override uint VIDIOC_G_AUDIO => 2150913569U;
        public override uint VIDIOC_G_AUDOUT => 2150913585U;
        public override uint VIDIOC_G_CROP => 3222558267U;
        public override uint VIDIOC_G_CTRL => 3221771803U;
        public override uint VIDIOC_G_DV_TIMINGS => 3229898328U;
        public override uint VIDIOC_G_EDID => 3223868968U;
        public override uint VIDIOC_G_ENC_INDEX => 2283296332U;
        public override uint VIDIOC_G_EXT_CTRLS => 3223344711U;
        public override uint VIDIOC_G_FBUF => 2150651402U;
        public override uint VIDIOC_G_FMT => 3234878980U;
        public override uint VIDIOC_G_FREQUENCY => 3224131128U;
        public override uint VIDIOC_G_INPUT => 2147767846U;
        public override uint VIDIOC_G_JPEGCOMP => 2156680765U;
        public override uint VIDIOC_G_MODULATOR => 3225703990U;
        public override uint VIDIOC_G_OUTPUT => 2147767854U;
        public override uint VIDIOC_G_PARM => 3234616853U;
        public override uint VIDIOC_G_PRIORITY => 2147767875U;
        public override uint VIDIOC_G_SELECTION => 3225441886U;
        public override uint VIDIOC_G_SLICED_VBI_CAP => 3228849733U;
        public override uint VIDIOC_G_STD => 2148029975U;
        public override uint VIDIOC_G_TUNER => 3226752541U;
        public override uint VIDIOC_LOG_STATUS => 22086U;
        public override uint VIDIOC_OVERLAY => 1074025998U;
        public override uint VIDIOC_PREPARE_BUF => 3227014749U;
        public override uint VIDIOC_QBUF => 3227014671U;
        public override uint VIDIOC_QUERY_DV_TIMINGS => 2156156515U;
        public override uint VIDIOC_QUERY_EXT_CTRL => 3236451943U;
        public override uint VIDIOC_QUERYBUF => 3227014665U;
        public override uint VIDIOC_QUERYCAP => 2154321408U;
        public override uint VIDIOC_QUERYCTRL => 3225703972U;
        public override uint VIDIOC_QUERYMENU => 3224131109U;
        public override uint VIDIOC_QUERYSTD => 2148030015U;
        public override uint VIDIOC_REQBUFS => 3222558216U;
        public override uint VIDIOC_S_AUDIO => 1077171746U;
        public override uint VIDIOC_S_AUDOUT => 1077171762U;
        public override uint VIDIOC_S_CROP => 1075074620U;
        public override uint VIDIOC_S_CTRL => 3221771804U;
        public override uint VIDIOC_S_DV_TIMINGS => 3229898327U;
        public override uint VIDIOC_S_EDID => 3223868969U;
        public override uint VIDIOC_S_EXT_CTRLS => 3223344712U;
        public override uint VIDIOC_S_FBUF => 1076909579U;
        public override uint VIDIOC_S_FMT => 3234878981U;
        public override uint VIDIOC_S_FREQUENCY => 1076647481U;
        public override uint VIDIOC_S_HW_FREQ_SEEK => 1076909650U;
        public override uint VIDIOC_S_INPUT => 3221509671U;
        public override uint VIDIOC_S_JPEGCOMP => 1082938942U;
        public override uint VIDIOC_S_MODULATOR => 1078220343U;
        public override uint VIDIOC_S_OUTPUT => 3221509679U;
        public override uint VIDIOC_S_PARM => 3234616854U;
        public override uint VIDIOC_S_PRIORITY => 1074026052U;
        public override uint VIDIOC_S_SELECTION => 3225441887U;
        public override uint VIDIOC_S_STD => 1074288152U;
        public override uint VIDIOC_S_TUNER => 1079268894U;
        public override uint VIDIOC_STREAMOFF => 1074026003U;
        public override uint VIDIOC_STREAMON => 1074026002U;
        public override uint VIDIOC_SUBSCRIBE_EVENT => 1075861082U;
        public override uint VIDIOC_TRY_DECODER_CMD => 3225966177U;
        public override uint VIDIOC_TRY_ENCODER_CMD => 3223869006U;
        public override uint VIDIOC_TRY_EXT_CTRLS => 3223344713U;
        public override uint VIDIOC_TRY_FMT => 3234879040U;
        public override uint VIDIOC_UNSUBSCRIBE_EVENT => 1075861083U;

        // Structures
        [StructLayout(LayoutKind.Explicit, Size=16)]
        private new unsafe struct timespec : NativeMethods_V4L2_Interop.timespec
        {
            [FieldOffset(0)] private IntPtr tv_sec_;   // long
            public IntPtr tv_sec
            {
                get => this.tv_sec_;
                set => this.tv_sec_ = (IntPtr)value;
            }

            [FieldOffset(8)] private IntPtr tv_nsec_;   // long
            public IntPtr tv_nsec
            {
                get => this.tv_nsec_;
                set => this.tv_nsec_ = (IntPtr)value;
            }

        }
        public override NativeMethods_V4L2_Interop.timespec Create_timespec() => new timespec();

        [StructLayout(LayoutKind.Explicit, Size=16)]
        private new unsafe struct timeval : NativeMethods_V4L2_Interop.timeval
        {
            [FieldOffset(0)] private IntPtr tv_sec_;   // long
            public IntPtr tv_sec
            {
                get => this.tv_sec_;
                set => this.tv_sec_ = (IntPtr)value;
            }

            [FieldOffset(8)] private IntPtr tv_usec_;   // long
            public IntPtr tv_usec
            {
                get => this.tv_usec_;
                set => this.tv_usec_ = (IntPtr)value;
            }

        }
        public override NativeMethods_V4L2_Interop.timeval Create_timeval() => new timeval();

        [StructLayout(LayoutKind.Explicit, Size=88)]
        private new unsafe struct v4l2_buffer : NativeMethods_V4L2_Interop.v4l2_buffer
        {
            [FieldOffset(0)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(4)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(8)] private uint bytesused_;
            public uint bytesused
            {
                get => this.bytesused_;
                set => this.bytesused_ = (uint)value;
            }

            [FieldOffset(12)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(16)] private uint field_;
            public uint field
            {
                get => this.field_;
                set => this.field_ = (uint)value;
            }

            [FieldOffset(24)] private timeval timestamp_;
            public NativeMethods_V4L2_Interop.timeval timestamp
            {
                get => this.timestamp_;
                set => this.timestamp_ = (timeval)value;
            }

            [FieldOffset(40)] private v4l2_timecode timecode_;
            public NativeMethods_V4L2_Interop.v4l2_timecode timecode
            {
                get => this.timecode_;
                set => this.timecode_ = (v4l2_timecode)value;
            }

            [FieldOffset(56)] private uint sequence_;
            public uint sequence
            {
                get => this.sequence_;
                set => this.sequence_ = (uint)value;
            }

            [FieldOffset(60)] private uint memory_;
            public uint memory
            {
                get => this.memory_;
                set => this.memory_ = (uint)value;
            }

            [FieldOffset(64)] private uint m_offset_;
            public uint m_offset
            {
                get => this.m_offset_;
                set => this.m_offset_ = (uint)value;
            }

            [FieldOffset(64)] private UIntPtr m_userptr_;   // unsigned long
            public UIntPtr m_userptr
            {
                get => this.m_userptr_;
                set => this.m_userptr_ = (UIntPtr)value;
            }

            [FieldOffset(64)] private v4l2_plane* m_planes_;
            public IntPtr m_planes
            {
                get => (IntPtr)this.m_planes_;
                set => this.m_planes_ = (v4l2_plane*)value.ToPointer();
            }

            [FieldOffset(64)] private int m_fd_;
            public int m_fd
            {
                get => this.m_fd_;
                set => this.m_fd_ = (int)value;
            }

            [FieldOffset(72)] private uint length_;
            public uint length
            {
                get => this.length_;
                set => this.length_ = (uint)value;
            }

            [FieldOffset(76)] private uint reserved2_;
            public uint reserved2
            {
                get => this.reserved2_;
                set => this.reserved2_ = (uint)value;
            }

            [FieldOffset(80)] private int request_fd_;
            public int request_fd
            {
                get => this.request_fd_;
                set => this.request_fd_ = (int)value;
            }

            [FieldOffset(80)] private uint reserved_;
            public uint reserved
            {
                get => this.reserved_;
                set => this.reserved_ = (uint)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_buffer Create_v4l2_buffer() => new v4l2_buffer();

        [StructLayout(LayoutKind.Explicit, Size=104)]
        private new unsafe struct v4l2_capability : NativeMethods_V4L2_Interop.v4l2_capability
        {
            [FieldOffset(0)] private fixed byte driver_[16];
            public byte[] driver
            {
                get { fixed (byte* p = this.driver_) { return get(p, 16); } }
                set { fixed (byte* p = this.driver_) { set(p, value, 16); } }
            }

            [FieldOffset(16)] private fixed byte card_[32];
            public byte[] card
            {
                get { fixed (byte* p = this.card_) { return get(p, 32); } }
                set { fixed (byte* p = this.card_) { set(p, value, 32); } }
            }

            [FieldOffset(48)] private fixed byte bus_info_[32];
            public byte[] bus_info
            {
                get { fixed (byte* p = this.bus_info_) { return get(p, 32); } }
                set { fixed (byte* p = this.bus_info_) { set(p, value, 32); } }
            }

            [FieldOffset(80)] private uint version_;
            public uint version
            {
                get => this.version_;
                set => this.version_ = (uint)value;
            }

            [FieldOffset(84)] private uint capabilities_;
            public uint capabilities
            {
                get => this.capabilities_;
                set => this.capabilities_ = (uint)value;
            }

            [FieldOffset(88)] private uint device_caps_;
            public uint device_caps
            {
                get => this.device_caps_;
                set => this.device_caps_ = (uint)value;
            }

            [FieldOffset(92)] private fixed uint reserved_[3];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 3); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 3); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_capability Create_v4l2_capability() => new v4l2_capability();

        [StructLayout(LayoutKind.Explicit, Size=24)]
        private new unsafe struct v4l2_clip : NativeMethods_V4L2_Interop.v4l2_clip
        {
            [FieldOffset(0)] private v4l2_rect c_;
            public NativeMethods_V4L2_Interop.v4l2_rect c
            {
                get => this.c_;
                set => this.c_ = (v4l2_rect)value;
            }

            [FieldOffset(16)] private v4l2_clip* next_;
            public IntPtr next
            {
                get => (IntPtr)this.next_;
                set => this.next_ = (v4l2_clip*)value.ToPointer();
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_clip Create_v4l2_clip() => new v4l2_clip();

        [StructLayout(LayoutKind.Explicit, Size=64)]
        private new unsafe struct v4l2_fmtdesc : NativeMethods_V4L2_Interop.v4l2_fmtdesc
        {
            [FieldOffset(0)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(4)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(8)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(12)] private fixed byte description_[32];
            public byte[] description
            {
                get { fixed (byte* p = this.description_) { return get(p, 32); } }
                set { fixed (byte* p = this.description_) { set(p, value, 32); } }
            }

            [FieldOffset(44)] private uint pixelformat_;
            public uint pixelformat
            {
                get => this.pixelformat_;
                set => this.pixelformat_ = (uint)value;
            }

            [FieldOffset(48)] private uint mbus_code_;
            public uint mbus_code
            {
                get => this.mbus_code_;
                set => this.mbus_code_ = (uint)value;
            }

            [FieldOffset(52)] private fixed uint reserved_[3];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 3); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 3); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_fmtdesc Create_v4l2_fmtdesc() => new v4l2_fmtdesc();

        [StructLayout(LayoutKind.Explicit, Size=208)]
        private new unsafe struct v4l2_format : NativeMethods_V4L2_Interop.v4l2_format
        {
            [FieldOffset(0)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(8)] private v4l2_pix_format fmt_pix_;
            public NativeMethods_V4L2_Interop.v4l2_pix_format fmt_pix
            {
                get => this.fmt_pix_;
                set => this.fmt_pix_ = (v4l2_pix_format)value;
            }

            [FieldOffset(8)] private v4l2_pix_format_mplane fmt_pix_mp_;
            public NativeMethods_V4L2_Interop.v4l2_pix_format_mplane fmt_pix_mp
            {
                get => this.fmt_pix_mp_;
                set => this.fmt_pix_mp_ = (v4l2_pix_format_mplane)value;
            }

            [FieldOffset(8)] private v4l2_window fmt_win_;
            public NativeMethods_V4L2_Interop.v4l2_window fmt_win
            {
                get => this.fmt_win_;
                set => this.fmt_win_ = (v4l2_window)value;
            }

            [FieldOffset(8)] private v4l2_vbi_format fmt_vbi_;
            public NativeMethods_V4L2_Interop.v4l2_vbi_format fmt_vbi
            {
                get => this.fmt_vbi_;
                set => this.fmt_vbi_ = (v4l2_vbi_format)value;
            }

            [FieldOffset(8)] private v4l2_sliced_vbi_format fmt_sliced_;
            public NativeMethods_V4L2_Interop.v4l2_sliced_vbi_format fmt_sliced
            {
                get => this.fmt_sliced_;
                set => this.fmt_sliced_ = (v4l2_sliced_vbi_format)value;
            }

            [FieldOffset(8)] private v4l2_sdr_format fmt_sdr_;
            public NativeMethods_V4L2_Interop.v4l2_sdr_format fmt_sdr
            {
                get => this.fmt_sdr_;
                set => this.fmt_sdr_ = (v4l2_sdr_format)value;
            }

            [FieldOffset(8)] private v4l2_meta_format fmt_meta_;
            public NativeMethods_V4L2_Interop.v4l2_meta_format fmt_meta
            {
                get => this.fmt_meta_;
                set => this.fmt_meta_ = (v4l2_meta_format)value;
            }

            [FieldOffset(8)] private fixed byte fmt_raw_data_[200];
            public byte[] fmt_raw_data
            {
                get { fixed (byte* p = this.fmt_raw_data_) { return get(p, 200); } }
                set { fixed (byte* p = this.fmt_raw_data_) { set(p, value, 200); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_format Create_v4l2_format() => new v4l2_format();

        [StructLayout(LayoutKind.Explicit, Size=8)]
        private new unsafe struct v4l2_fract : NativeMethods_V4L2_Interop.v4l2_fract
        {
            [FieldOffset(0)] private uint numerator_;
            public uint numerator
            {
                get => this.numerator_;
                set => this.numerator_ = (uint)value;
            }

            [FieldOffset(4)] private uint denominator_;
            public uint denominator
            {
                get => this.denominator_;
                set => this.denominator_ = (uint)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_fract Create_v4l2_fract() => new v4l2_fract();

        [StructLayout(LayoutKind.Explicit, Size=24)]
        private new unsafe struct v4l2_frmival_stepwise : NativeMethods_V4L2_Interop.v4l2_frmival_stepwise
        {
            [FieldOffset(0)] private v4l2_fract min_;
            public NativeMethods_V4L2_Interop.v4l2_fract min
            {
                get => this.min_;
                set => this.min_ = (v4l2_fract)value;
            }

            [FieldOffset(8)] private v4l2_fract max_;
            public NativeMethods_V4L2_Interop.v4l2_fract max
            {
                get => this.max_;
                set => this.max_ = (v4l2_fract)value;
            }

            [FieldOffset(16)] private v4l2_fract step_;
            public NativeMethods_V4L2_Interop.v4l2_fract step
            {
                get => this.step_;
                set => this.step_ = (v4l2_fract)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_frmival_stepwise Create_v4l2_frmival_stepwise() => new v4l2_frmival_stepwise();

        [StructLayout(LayoutKind.Explicit, Size=52)]
        private new unsafe struct v4l2_frmivalenum : NativeMethods_V4L2_Interop.v4l2_frmivalenum
        {
            [FieldOffset(0)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(4)] private uint pixel_format_;
            public uint pixel_format
            {
                get => this.pixel_format_;
                set => this.pixel_format_ = (uint)value;
            }

            [FieldOffset(8)] private uint width_;
            public uint width
            {
                get => this.width_;
                set => this.width_ = (uint)value;
            }

            [FieldOffset(12)] private uint height_;
            public uint height
            {
                get => this.height_;
                set => this.height_ = (uint)value;
            }

            [FieldOffset(16)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(20)] private v4l2_fract discrete_;
            public NativeMethods_V4L2_Interop.v4l2_fract discrete
            {
                get => this.discrete_;
                set => this.discrete_ = (v4l2_fract)value;
            }

            [FieldOffset(20)] private v4l2_frmival_stepwise stepwise_;
            public NativeMethods_V4L2_Interop.v4l2_frmival_stepwise stepwise
            {
                get => this.stepwise_;
                set => this.stepwise_ = (v4l2_frmival_stepwise)value;
            }

            [FieldOffset(44)] private fixed uint reserved_[2];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 2); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 2); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_frmivalenum Create_v4l2_frmivalenum() => new v4l2_frmivalenum();

        [StructLayout(LayoutKind.Explicit, Size=8)]
        private new unsafe struct v4l2_frmsize_discrete : NativeMethods_V4L2_Interop.v4l2_frmsize_discrete
        {
            [FieldOffset(0)] private uint width_;
            public uint width
            {
                get => this.width_;
                set => this.width_ = (uint)value;
            }

            [FieldOffset(4)] private uint height_;
            public uint height
            {
                get => this.height_;
                set => this.height_ = (uint)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_frmsize_discrete Create_v4l2_frmsize_discrete() => new v4l2_frmsize_discrete();

        [StructLayout(LayoutKind.Explicit, Size=24)]
        private new unsafe struct v4l2_frmsize_stepwise : NativeMethods_V4L2_Interop.v4l2_frmsize_stepwise
        {
            [FieldOffset(0)] private uint min_width_;
            public uint min_width
            {
                get => this.min_width_;
                set => this.min_width_ = (uint)value;
            }

            [FieldOffset(4)] private uint max_width_;
            public uint max_width
            {
                get => this.max_width_;
                set => this.max_width_ = (uint)value;
            }

            [FieldOffset(8)] private uint step_width_;
            public uint step_width
            {
                get => this.step_width_;
                set => this.step_width_ = (uint)value;
            }

            [FieldOffset(12)] private uint min_height_;
            public uint min_height
            {
                get => this.min_height_;
                set => this.min_height_ = (uint)value;
            }

            [FieldOffset(16)] private uint max_height_;
            public uint max_height
            {
                get => this.max_height_;
                set => this.max_height_ = (uint)value;
            }

            [FieldOffset(20)] private uint step_height_;
            public uint step_height
            {
                get => this.step_height_;
                set => this.step_height_ = (uint)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_frmsize_stepwise Create_v4l2_frmsize_stepwise() => new v4l2_frmsize_stepwise();

        [StructLayout(LayoutKind.Explicit, Size=44)]
        private new unsafe struct v4l2_frmsizeenum : NativeMethods_V4L2_Interop.v4l2_frmsizeenum
        {
            [FieldOffset(0)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(4)] private uint pixel_format_;
            public uint pixel_format
            {
                get => this.pixel_format_;
                set => this.pixel_format_ = (uint)value;
            }

            [FieldOffset(8)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(12)] private v4l2_frmsize_discrete discrete_;
            public NativeMethods_V4L2_Interop.v4l2_frmsize_discrete discrete
            {
                get => this.discrete_;
                set => this.discrete_ = (v4l2_frmsize_discrete)value;
            }

            [FieldOffset(12)] private v4l2_frmsize_stepwise stepwise_;
            public NativeMethods_V4L2_Interop.v4l2_frmsize_stepwise stepwise
            {
                get => this.stepwise_;
                set => this.stepwise_ = (v4l2_frmsize_stepwise)value;
            }

            [FieldOffset(36)] private fixed uint reserved_[2];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 2); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 2); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_frmsizeenum Create_v4l2_frmsizeenum() => new v4l2_frmsizeenum();

        [StructLayout(LayoutKind.Explicit, Size=8)]
        private new unsafe struct v4l2_meta_format : NativeMethods_V4L2_Interop.v4l2_meta_format
        {
            [FieldOffset(0)] private uint dataformat_;
            public uint dataformat
            {
                get => this.dataformat_;
                set => this.dataformat_ = (uint)value;
            }

            [FieldOffset(4)] private uint buffersize_;
            public uint buffersize
            {
                get => this.buffersize_;
                set => this.buffersize_ = (uint)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_meta_format Create_v4l2_meta_format() => new v4l2_meta_format();

        [StructLayout(LayoutKind.Explicit, Size=48)]
        private new unsafe struct v4l2_pix_format : NativeMethods_V4L2_Interop.v4l2_pix_format
        {
            [FieldOffset(0)] private uint width_;
            public uint width
            {
                get => this.width_;
                set => this.width_ = (uint)value;
            }

            [FieldOffset(4)] private uint height_;
            public uint height
            {
                get => this.height_;
                set => this.height_ = (uint)value;
            }

            [FieldOffset(8)] private uint pixelformat_;
            public uint pixelformat
            {
                get => this.pixelformat_;
                set => this.pixelformat_ = (uint)value;
            }

            [FieldOffset(12)] private uint field_;
            public uint field
            {
                get => this.field_;
                set => this.field_ = (uint)value;
            }

            [FieldOffset(16)] private uint bytesperline_;
            public uint bytesperline
            {
                get => this.bytesperline_;
                set => this.bytesperline_ = (uint)value;
            }

            [FieldOffset(20)] private uint sizeimage_;
            public uint sizeimage
            {
                get => this.sizeimage_;
                set => this.sizeimage_ = (uint)value;
            }

            [FieldOffset(24)] private uint colorspace_;
            public uint colorspace
            {
                get => this.colorspace_;
                set => this.colorspace_ = (uint)value;
            }

            [FieldOffset(28)] private uint priv_;
            public uint priv
            {
                get => this.priv_;
                set => this.priv_ = (uint)value;
            }

            [FieldOffset(32)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(36)] private uint ycbcr_enc_;
            public uint ycbcr_enc
            {
                get => this.ycbcr_enc_;
                set => this.ycbcr_enc_ = (uint)value;
            }

            [FieldOffset(36)] private uint hsv_enc_;
            public uint hsv_enc
            {
                get => this.hsv_enc_;
                set => this.hsv_enc_ = (uint)value;
            }

            [FieldOffset(40)] private uint quantization_;
            public uint quantization
            {
                get => this.quantization_;
                set => this.quantization_ = (uint)value;
            }

            [FieldOffset(44)] private uint xfer_func_;
            public uint xfer_func
            {
                get => this.xfer_func_;
                set => this.xfer_func_ = (uint)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_pix_format Create_v4l2_pix_format() => new v4l2_pix_format();

        [StructLayout(LayoutKind.Explicit, Size=192)]
        private new unsafe struct v4l2_pix_format_mplane : NativeMethods_V4L2_Interop.v4l2_pix_format_mplane
        {
            [FieldOffset(0)] private uint width_;
            public uint width
            {
                get => this.width_;
                set => this.width_ = (uint)value;
            }

            [FieldOffset(4)] private uint height_;
            public uint height
            {
                get => this.height_;
                set => this.height_ = (uint)value;
            }

            [FieldOffset(8)] private uint pixelformat_;
            public uint pixelformat
            {
                get => this.pixelformat_;
                set => this.pixelformat_ = (uint)value;
            }

            [FieldOffset(12)] private uint field_;
            public uint field
            {
                get => this.field_;
                set => this.field_ = (uint)value;
            }

            [FieldOffset(16)] private uint colorspace_;
            public uint colorspace
            {
                get => this.colorspace_;
                set => this.colorspace_ = (uint)value;
            }

            [FieldOffset(20)] private fixed byte plane_fmt_[20 * 8];   // sizeof(v4l2_plane_pix_format): 20
            public NativeMethods_V4L2_Interop.v4l2_plane_pix_format[] plane_fmt
            {
                get { fixed (byte* p = this.plane_fmt_) { return get<v4l2_plane_pix_format, NativeMethods_V4L2_Interop.v4l2_plane_pix_format>(p, 20, 8); } }
                set { fixed (byte* p = this.plane_fmt_) { set<v4l2_plane_pix_format, NativeMethods_V4L2_Interop.v4l2_plane_pix_format>(p, value, 20, 8); } }
            }

            [FieldOffset(180)] private byte num_planes_;
            public byte num_planes
            {
                get => this.num_planes_;
                set => this.num_planes_ = (byte)value;
            }

            [FieldOffset(181)] private byte flags_;
            public byte flags
            {
                get => this.flags_;
                set => this.flags_ = (byte)value;
            }

            [FieldOffset(182)] private byte ycbcr_enc_;
            public byte ycbcr_enc
            {
                get => this.ycbcr_enc_;
                set => this.ycbcr_enc_ = (byte)value;
            }

            [FieldOffset(182)] private byte hsv_enc_;
            public byte hsv_enc
            {
                get => this.hsv_enc_;
                set => this.hsv_enc_ = (byte)value;
            }

            [FieldOffset(183)] private byte quantization_;
            public byte quantization
            {
                get => this.quantization_;
                set => this.quantization_ = (byte)value;
            }

            [FieldOffset(184)] private byte xfer_func_;
            public byte xfer_func
            {
                get => this.xfer_func_;
                set => this.xfer_func_ = (byte)value;
            }

            [FieldOffset(185)] private fixed byte reserved_[7];
            public byte[] reserved
            {
                get { fixed (byte* p = this.reserved_) { return get(p, 7); } }
                set { fixed (byte* p = this.reserved_) { set(p, value, 7); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_pix_format_mplane Create_v4l2_pix_format_mplane() => new v4l2_pix_format_mplane();

        [StructLayout(LayoutKind.Explicit, Size=64)]
        private new unsafe struct v4l2_plane : NativeMethods_V4L2_Interop.v4l2_plane
        {
            [FieldOffset(0)] private uint bytesused_;
            public uint bytesused
            {
                get => this.bytesused_;
                set => this.bytesused_ = (uint)value;
            }

            [FieldOffset(4)] private uint length_;
            public uint length
            {
                get => this.length_;
                set => this.length_ = (uint)value;
            }

            [FieldOffset(8)] private uint m_mem_offset_;
            public uint m_mem_offset
            {
                get => this.m_mem_offset_;
                set => this.m_mem_offset_ = (uint)value;
            }

            [FieldOffset(8)] private UIntPtr m_userptr_;   // unsigned long
            public UIntPtr m_userptr
            {
                get => this.m_userptr_;
                set => this.m_userptr_ = (UIntPtr)value;
            }

            [FieldOffset(8)] private int m_fd_;
            public int m_fd
            {
                get => this.m_fd_;
                set => this.m_fd_ = (int)value;
            }

            [FieldOffset(16)] private uint data_offset_;
            public uint data_offset
            {
                get => this.data_offset_;
                set => this.data_offset_ = (uint)value;
            }

            [FieldOffset(20)] private fixed uint reserved_[11];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 11); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 11); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_plane Create_v4l2_plane() => new v4l2_plane();

        [StructLayout(LayoutKind.Explicit, Size=20)]
        private new unsafe struct v4l2_plane_pix_format : NativeMethods_V4L2_Interop.v4l2_plane_pix_format
        {
            [FieldOffset(0)] private uint sizeimage_;
            public uint sizeimage
            {
                get => this.sizeimage_;
                set => this.sizeimage_ = (uint)value;
            }

            [FieldOffset(4)] private uint bytesperline_;
            public uint bytesperline
            {
                get => this.bytesperline_;
                set => this.bytesperline_ = (uint)value;
            }

            [FieldOffset(8)] private fixed ushort reserved_[6];
            public ushort[] reserved
            {
                get { fixed (ushort* p = this.reserved_) { return get(p, 6); } }
                set { fixed (ushort* p = this.reserved_) { set(p, value, 6); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_plane_pix_format Create_v4l2_plane_pix_format() => new v4l2_plane_pix_format();

        [StructLayout(LayoutKind.Explicit, Size=16)]
        private new unsafe struct v4l2_rect : NativeMethods_V4L2_Interop.v4l2_rect
        {
            [FieldOffset(0)] private int left_;
            public int left
            {
                get => this.left_;
                set => this.left_ = (int)value;
            }

            [FieldOffset(4)] private int top_;
            public int top
            {
                get => this.top_;
                set => this.top_ = (int)value;
            }

            [FieldOffset(8)] private uint width_;
            public uint width
            {
                get => this.width_;
                set => this.width_ = (uint)value;
            }

            [FieldOffset(12)] private uint height_;
            public uint height
            {
                get => this.height_;
                set => this.height_ = (uint)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_rect Create_v4l2_rect() => new v4l2_rect();

        [StructLayout(LayoutKind.Explicit, Size=20)]
        private new unsafe struct v4l2_requestbuffers : NativeMethods_V4L2_Interop.v4l2_requestbuffers
        {
            [FieldOffset(0)] private uint count_;
            public uint count
            {
                get => this.count_;
                set => this.count_ = (uint)value;
            }

            [FieldOffset(4)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(8)] private uint memory_;
            public uint memory
            {
                get => this.memory_;
                set => this.memory_ = (uint)value;
            }

            [FieldOffset(12)] private uint capabilities_;
            public uint capabilities
            {
                get => this.capabilities_;
                set => this.capabilities_ = (uint)value;
            }

            [FieldOffset(16)] private fixed uint reserved_[1];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 1); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 1); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_requestbuffers Create_v4l2_requestbuffers() => new v4l2_requestbuffers();

        [StructLayout(LayoutKind.Explicit, Size=32)]
        private new unsafe struct v4l2_sdr_format : NativeMethods_V4L2_Interop.v4l2_sdr_format
        {
            [FieldOffset(0)] private uint pixelformat_;
            public uint pixelformat
            {
                get => this.pixelformat_;
                set => this.pixelformat_ = (uint)value;
            }

            [FieldOffset(4)] private uint buffersize_;
            public uint buffersize
            {
                get => this.buffersize_;
                set => this.buffersize_ = (uint)value;
            }

            [FieldOffset(8)] private fixed byte reserved_[24];
            public byte[] reserved
            {
                get { fixed (byte* p = this.reserved_) { return get(p, 24); } }
                set { fixed (byte* p = this.reserved_) { set(p, value, 24); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_sdr_format Create_v4l2_sdr_format() => new v4l2_sdr_format();

        [StructLayout(LayoutKind.Explicit, Size=112)]
        private new unsafe struct v4l2_sliced_vbi_format : NativeMethods_V4L2_Interop.v4l2_sliced_vbi_format
        {
            [FieldOffset(0)] private ushort service_set_;
            public ushort service_set
            {
                get => this.service_set_;
                set => this.service_set_ = (ushort)value;
            }

            [FieldOffset(2)] private fixed ushort service_lines_[2 * 24];
            public ushort[][] service_lines
            {
                get { fixed (ushort* p = this.service_lines_) { return get(p, 2,24); } }
                set { fixed (ushort* p = this.service_lines_) { set(p, value, 2,24); } }
            }

            [FieldOffset(100)] private uint io_size_;
            public uint io_size
            {
                get => this.io_size_;
                set => this.io_size_ = (uint)value;
            }

            [FieldOffset(104)] private fixed uint reserved_[2];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 2); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 2); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_sliced_vbi_format Create_v4l2_sliced_vbi_format() => new v4l2_sliced_vbi_format();

        [StructLayout(LayoutKind.Explicit, Size=16)]
        private new unsafe struct v4l2_timecode : NativeMethods_V4L2_Interop.v4l2_timecode
        {
            [FieldOffset(0)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(4)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(8)] private byte frames_;
            public byte frames
            {
                get => this.frames_;
                set => this.frames_ = (byte)value;
            }

            [FieldOffset(9)] private byte seconds_;
            public byte seconds
            {
                get => this.seconds_;
                set => this.seconds_ = (byte)value;
            }

            [FieldOffset(10)] private byte minutes_;
            public byte minutes
            {
                get => this.minutes_;
                set => this.minutes_ = (byte)value;
            }

            [FieldOffset(11)] private byte hours_;
            public byte hours
            {
                get => this.hours_;
                set => this.hours_ = (byte)value;
            }

            [FieldOffset(12)] private fixed byte userbits_[4];
            public byte[] userbits
            {
                get { fixed (byte* p = this.userbits_) { return get(p, 4); } }
                set { fixed (byte* p = this.userbits_) { set(p, value, 4); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_timecode Create_v4l2_timecode() => new v4l2_timecode();

        [StructLayout(LayoutKind.Explicit, Size=44)]
        private new unsafe struct v4l2_vbi_format : NativeMethods_V4L2_Interop.v4l2_vbi_format
        {
            [FieldOffset(0)] private uint sampling_rate_;
            public uint sampling_rate
            {
                get => this.sampling_rate_;
                set => this.sampling_rate_ = (uint)value;
            }

            [FieldOffset(4)] private uint offset_;
            public uint offset
            {
                get => this.offset_;
                set => this.offset_ = (uint)value;
            }

            [FieldOffset(8)] private uint samples_per_line_;
            public uint samples_per_line
            {
                get => this.samples_per_line_;
                set => this.samples_per_line_ = (uint)value;
            }

            [FieldOffset(12)] private uint sample_format_;
            public uint sample_format
            {
                get => this.sample_format_;
                set => this.sample_format_ = (uint)value;
            }

            [FieldOffset(16)] private fixed int start_[2];
            public int[] start
            {
                get { fixed (int* p = this.start_) { return get(p, 2); } }
                set { fixed (int* p = this.start_) { set(p, value, 2); } }
            }

            [FieldOffset(24)] private fixed uint count_[2];
            public uint[] count
            {
                get { fixed (uint* p = this.count_) { return get(p, 2); } }
                set { fixed (uint* p = this.count_) { set(p, value, 2); } }
            }

            [FieldOffset(32)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(36)] private fixed uint reserved_[2];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 2); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 2); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_vbi_format Create_v4l2_vbi_format() => new v4l2_vbi_format();

        [StructLayout(LayoutKind.Explicit, Size=56)]
        private new unsafe struct v4l2_window : NativeMethods_V4L2_Interop.v4l2_window
        {
            [FieldOffset(0)] private v4l2_rect w_;
            public NativeMethods_V4L2_Interop.v4l2_rect w
            {
                get => this.w_;
                set => this.w_ = (v4l2_rect)value;
            }

            [FieldOffset(16)] private uint field_;
            public uint field
            {
                get => this.field_;
                set => this.field_ = (uint)value;
            }

            [FieldOffset(20)] private uint chromakey_;
            public uint chromakey
            {
                get => this.chromakey_;
                set => this.chromakey_ = (uint)value;
            }

            [FieldOffset(24)] private v4l2_clip* clips_;
            public IntPtr clips
            {
                get => (IntPtr)this.clips_;
                set => this.clips_ = (v4l2_clip*)value.ToPointer();
            }

            [FieldOffset(32)] private uint clipcount_;
            public uint clipcount
            {
                get => this.clipcount_;
                set => this.clipcount_ = (uint)value;
            }

            [FieldOffset(40)] private void* bitmap_;
            public IntPtr bitmap
            {
                get => (IntPtr)this.bitmap_;
                set => this.bitmap_ = (void*)value.ToPointer();
            }

            [FieldOffset(48)] private byte global_alpha_;
            public byte global_alpha
            {
                get => this.global_alpha_;
                set => this.global_alpha_ = (byte)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_window Create_v4l2_window() => new v4l2_window();


    }
}

