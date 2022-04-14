// This is auto generated code by FlashCap.V4L2Generator [0.14.6]. Do not edit.
// 
// Thu, 14 Apr 2022 12:09:49 GMT

using System;
using System.Runtime.InteropServices;

namespace FlashCap.Internal.V4L2
{
    internal sealed class NativeMethods_V4L2_Interop_aarch64 : NativeMethods_V4L2_Interop
    {
        // Definitions
        public override uint BASE_VIDIOC_PRIVATE => 192U;
        public override uint V4L2_AUDCAP_AVL => 2U;
        public override uint V4L2_AUDCAP_STEREO => 1U;
        public override uint V4L2_AUDMODE_AVL => 1U;
        public override uint V4L2_BAND_MODULATION_AM => 8U;
        public override uint V4L2_BAND_MODULATION_FM => 4U;
        public override uint V4L2_BAND_MODULATION_VSB => 2U;
        public override uint V4L2_BUF_CAP_SUPPORTS_DMABUF => 4U;
        public override uint V4L2_BUF_CAP_SUPPORTS_M2M_HOLD_CAPTURE_BUF => 32U;
        public override uint V4L2_BUF_CAP_SUPPORTS_MMAP => 1U;
        public override uint V4L2_BUF_CAP_SUPPORTS_MMAP_CACHE_HINTS => 64U;
        public override uint V4L2_BUF_CAP_SUPPORTS_ORPHANED_BUFS => 16U;
        public override uint V4L2_BUF_CAP_SUPPORTS_REQUESTS => 8U;
        public override uint V4L2_BUF_CAP_SUPPORTS_USERPTR => 2U;
        public override uint V4L2_BUF_FLAG_BFRAME => 32U;
        public override uint V4L2_BUF_FLAG_DONE => 4U;
        public override uint V4L2_BUF_FLAG_ERROR => 64U;
        public override uint V4L2_BUF_FLAG_IN_REQUEST => 128U;
        public override uint V4L2_BUF_FLAG_KEYFRAME => 8U;
        public override uint V4L2_BUF_FLAG_LAST => 1048576U;
        public override uint V4L2_BUF_FLAG_M2M_HOLD_CAPTURE_BUF => 512U;
        public override uint V4L2_BUF_FLAG_MAPPED => 1U;
        public override uint V4L2_BUF_FLAG_NO_CACHE_CLEAN => 4096U;
        public override uint V4L2_BUF_FLAG_NO_CACHE_INVALIDATE => 2048U;
        public override uint V4L2_BUF_FLAG_PFRAME => 16U;
        public override uint V4L2_BUF_FLAG_PREPARED => 1024U;
        public override uint V4L2_BUF_FLAG_QUEUED => 2U;
        public override uint V4L2_BUF_FLAG_REQUEST_FD => 8388608U;
        public override uint V4L2_BUF_FLAG_TIMECODE => 256U;
        public override uint V4L2_BUF_FLAG_TIMESTAMP_COPY => 16384U;
        public override uint V4L2_BUF_FLAG_TIMESTAMP_MASK => 57344U;
        public override uint V4L2_BUF_FLAG_TIMESTAMP_MONOTONIC => 8192U;
        public override uint V4L2_BUF_FLAG_TIMESTAMP_UNKNOWN => 0U;
        public override uint V4L2_BUF_FLAG_TSTAMP_SRC_EOF => 0U;
        public override uint V4L2_BUF_FLAG_TSTAMP_SRC_MASK => 458752U;
        public override uint V4L2_BUF_FLAG_TSTAMP_SRC_SOE => 65536U;
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
        public override uint V4L2_CHIP_FL_READABLE => 1U;
        public override uint V4L2_CHIP_FL_WRITABLE => 2U;
        public override uint V4L2_CHIP_MATCH_AC97 => 3U;
        public override uint V4L2_CHIP_MATCH_BRIDGE => 0U;
        public override uint V4L2_CHIP_MATCH_HOST => 0U;
        public override uint V4L2_CHIP_MATCH_I2C_ADDR => 2U;
        public override uint V4L2_CHIP_MATCH_I2C_DRIVER => 1U;
        public override uint V4L2_CHIP_MATCH_SUBDEV => 4U;
        public override uint V4L2_CID_MAX_CTRLS => 1024U;
        public override uint V4L2_CID_PRIVATE_BASE => 134217728U;
        public override uint V4L2_COLORSPACE_ADOBERGB => 9U;
        public override uint V4L2_CTRL_FLAG_DISABLED => 1U;
        public override uint V4L2_CTRL_FLAG_EXECUTE_ON_WRITE => 512U;
        public override uint V4L2_CTRL_FLAG_GRABBED => 2U;
        public override uint V4L2_CTRL_FLAG_HAS_PAYLOAD => 256U;
        public override uint V4L2_CTRL_FLAG_INACTIVE => 16U;
        public override uint V4L2_CTRL_FLAG_MODIFY_LAYOUT => 1024U;
        public override uint V4L2_CTRL_FLAG_NEXT_COMPOUND => 1073741824U;
        public override uint V4L2_CTRL_FLAG_NEXT_CTRL => 2147483648U;
        public override uint V4L2_CTRL_FLAG_READ_ONLY => 4U;
        public override uint V4L2_CTRL_FLAG_SLIDER => 32U;
        public override uint V4L2_CTRL_FLAG_UPDATE => 8U;
        public override uint V4L2_CTRL_FLAG_VOLATILE => 128U;
        public override uint V4L2_CTRL_FLAG_WRITE_ONLY => 64U;
        public override uint V4L2_CTRL_ID_MASK => 268435455U;
        public override uint V4L2_CTRL_MAX_DIMS => 4U;
        public override uint V4L2_CTRL_WHICH_CUR_VAL => 0U;
        public override uint V4L2_CTRL_WHICH_DEF_VAL => 251658240U;
        public override uint V4L2_CTRL_WHICH_REQUEST_VAL => 251723776U;
        public override uint V4L2_DEC_CMD_FLUSH => 4U;
        public override uint V4L2_DEC_CMD_PAUSE => 2U;
        public override uint V4L2_DEC_CMD_PAUSE_TO_BLACK => 1U;
        public override uint V4L2_DEC_CMD_RESUME => 3U;
        public override uint V4L2_DEC_CMD_START => 0U;
        public override uint V4L2_DEC_CMD_START_MUTE_AUDIO => 1U;
        public override uint V4L2_DEC_CMD_STOP => 1U;
        public override uint V4L2_DEC_CMD_STOP_IMMEDIATELY => 2U;
        public override uint V4L2_DEC_CMD_STOP_TO_BLACK => 1U;
        public override uint V4L2_DEC_START_FMT_GOP => 1U;
        public override uint V4L2_DEC_START_FMT_NONE => 0U;
        public override uint V4L2_DV_BT_656_1120 => 0U;
        public override uint V4L2_DV_BT_CAP_CUSTOM => 8U;
        public override uint V4L2_DV_BT_CAP_INTERLACED => 1U;
        public override uint V4L2_DV_BT_CAP_PROGRESSIVE => 2U;
        public override uint V4L2_DV_BT_CAP_REDUCED_BLANKING => 4U;
        public override uint V4L2_DV_BT_STD_CEA861 => 1U;
        public override uint V4L2_DV_BT_STD_CVT => 4U;
        public override uint V4L2_DV_BT_STD_DMT => 2U;
        public override uint V4L2_DV_BT_STD_GTF => 8U;
        public override uint V4L2_DV_BT_STD_SDI => 16U;
        public override uint V4L2_DV_FL_CAN_DETECT_REDUCED_FPS => 512U;
        public override uint V4L2_DV_FL_CAN_REDUCE_FPS => 2U;
        public override uint V4L2_DV_FL_FIRST_FIELD_EXTRA_LINE => 32U;
        public override uint V4L2_DV_FL_HALF_LINE => 8U;
        public override uint V4L2_DV_FL_HAS_CEA861_VIC => 128U;
        public override uint V4L2_DV_FL_HAS_HDMI_VIC => 256U;
        public override uint V4L2_DV_FL_HAS_PICTURE_ASPECT => 64U;
        public override uint V4L2_DV_FL_IS_CE_VIDEO => 16U;
        public override uint V4L2_DV_FL_REDUCED_BLANKING => 1U;
        public override uint V4L2_DV_FL_REDUCED_FPS => 4U;
        public override uint V4L2_DV_HSYNC_POS_POL => 2U;
        public override uint V4L2_DV_INTERLACED => 1U;
        public override uint V4L2_DV_PROGRESSIVE => 0U;
        public override uint V4L2_DV_VSYNC_POS_POL => 1U;
        public override uint V4L2_ENC_CMD_PAUSE => 2U;
        public override uint V4L2_ENC_CMD_RESUME => 3U;
        public override uint V4L2_ENC_CMD_START => 0U;
        public override uint V4L2_ENC_CMD_STOP => 1U;
        public override uint V4L2_ENC_CMD_STOP_AT_GOP_END => 1U;
        public override uint V4L2_ENC_IDX_ENTRIES => 64U;
        public override uint V4L2_ENC_IDX_FRAME_B => 2U;
        public override uint V4L2_ENC_IDX_FRAME_I => 0U;
        public override uint V4L2_ENC_IDX_FRAME_MASK => 15U;
        public override uint V4L2_ENC_IDX_FRAME_P => 1U;
        public override uint V4L2_EVENT_ALL => 0U;
        public override uint V4L2_EVENT_CTRL => 3U;
        public override uint V4L2_EVENT_CTRL_CH_FLAGS => 2U;
        public override uint V4L2_EVENT_CTRL_CH_RANGE => 4U;
        public override uint V4L2_EVENT_CTRL_CH_VALUE => 1U;
        public override uint V4L2_EVENT_EOS => 2U;
        public override uint V4L2_EVENT_FRAME_SYNC => 4U;
        public override uint V4L2_EVENT_MD_FL_HAVE_FRAME_SEQ => 1U;
        public override uint V4L2_EVENT_MOTION_DET => 6U;
        public override uint V4L2_EVENT_PRIVATE_START => 134217728U;
        public override uint V4L2_EVENT_SOURCE_CHANGE => 5U;
        public override uint V4L2_EVENT_SRC_CH_RESOLUTION => 1U;
        public override uint V4L2_EVENT_SUB_FL_ALLOW_FEEDBACK => 2U;
        public override uint V4L2_EVENT_SUB_FL_SEND_INITIAL => 1U;
        public override uint V4L2_EVENT_VSYNC => 1U;
        public override uint V4L2_FBUF_CAP_BITMAP_CLIPPING => 8U;
        public override uint V4L2_FBUF_CAP_CHROMAKEY => 2U;
        public override uint V4L2_FBUF_CAP_EXTERNOVERLAY => 1U;
        public override uint V4L2_FBUF_CAP_GLOBAL_ALPHA => 32U;
        public override uint V4L2_FBUF_CAP_LIST_CLIPPING => 4U;
        public override uint V4L2_FBUF_CAP_LOCAL_ALPHA => 16U;
        public override uint V4L2_FBUF_CAP_LOCAL_INV_ALPHA => 64U;
        public override uint V4L2_FBUF_CAP_SRC_CHROMAKEY => 128U;
        public override uint V4L2_FBUF_FLAG_CHROMAKEY => 4U;
        public override uint V4L2_FBUF_FLAG_GLOBAL_ALPHA => 16U;
        public override uint V4L2_FBUF_FLAG_LOCAL_ALPHA => 8U;
        public override uint V4L2_FBUF_FLAG_LOCAL_INV_ALPHA => 32U;
        public override uint V4L2_FBUF_FLAG_OVERLAY => 2U;
        public override uint V4L2_FBUF_FLAG_PRIMARY => 1U;
        public override uint V4L2_FBUF_FLAG_SRC_CHROMAKEY => 64U;
        public override uint V4L2_FMT_FLAG_COMPRESSED => 1U;
        public override uint V4L2_FMT_FLAG_CONTINUOUS_BYTESTREAM => 4U;
        public override uint V4L2_FMT_FLAG_CSC_COLORSPACE => 32U;
        public override uint V4L2_FMT_FLAG_CSC_HSV_ENC => 128U;
        public override uint V4L2_FMT_FLAG_CSC_QUANTIZATION => 256U;
        public override uint V4L2_FMT_FLAG_CSC_XFER_FUNC => 64U;
        public override uint V4L2_FMT_FLAG_CSC_YCBCR_ENC => 128U;
        public override uint V4L2_FMT_FLAG_DYN_RESOLUTION => 8U;
        public override uint V4L2_FMT_FLAG_EMULATED => 2U;
        public override uint V4L2_FMT_FLAG_ENC_CAP_FRAME_INTERVAL => 16U;
        public override uint V4L2_FOURCC_CONV => 1663394597U;
        public override uint V4L2_IN_CAP_CUSTOM_TIMINGS => 2U;
        public override uint V4L2_IN_CAP_DV_TIMINGS => 2U;
        public override uint V4L2_IN_CAP_NATIVE_SIZE => 8U;
        public override uint V4L2_IN_CAP_STD => 4U;
        public override uint V4L2_IN_ST_COLOR_KILL => 512U;
        public override uint V4L2_IN_ST_HFLIP => 16U;
        public override uint V4L2_IN_ST_MACROVISION => 16777216U;
        public override uint V4L2_IN_ST_NO_ACCESS => 33554432U;
        public override uint V4L2_IN_ST_NO_CARRIER => 262144U;
        public override uint V4L2_IN_ST_NO_COLOR => 4U;
        public override uint V4L2_IN_ST_NO_EQU => 131072U;
        public override uint V4L2_IN_ST_NO_H_LOCK => 256U;
        public override uint V4L2_IN_ST_NO_POWER => 1U;
        public override uint V4L2_IN_ST_NO_SIGNAL => 2U;
        public override uint V4L2_IN_ST_NO_STD_LOCK => 2048U;
        public override uint V4L2_IN_ST_NO_SYNC => 65536U;
        public override uint V4L2_IN_ST_NO_V_LOCK => 1024U;
        public override uint V4L2_IN_ST_VFLIP => 32U;
        public override uint V4L2_IN_ST_VTR => 67108864U;
        public override uint V4L2_INPUT_TYPE_CAMERA => 2U;
        public override uint V4L2_INPUT_TYPE_TOUCH => 3U;
        public override uint V4L2_INPUT_TYPE_TUNER => 1U;
        public override uint V4L2_JPEG_MARKER_APP => 128U;
        public override uint V4L2_JPEG_MARKER_COM => 64U;
        public override uint V4L2_JPEG_MARKER_DHT => 8U;
        public override uint V4L2_JPEG_MARKER_DQT => 16U;
        public override uint V4L2_JPEG_MARKER_DRI => 32U;
        public override uint V4L2_META_FMT_BCM2835_ISP_STATS => 1096045378U;
        public override uint V4L2_META_FMT_D4XX => 1482175556U;
        public override uint V4L2_META_FMT_SENSOR_DATA => 1397638483U;
        public override uint V4L2_META_FMT_UVC => 1212372565U;
        public override uint V4L2_META_FMT_VIVID => 1146505558U;
        public override uint V4L2_META_FMT_VSP1_HGO => 1213223766U;
        public override uint V4L2_META_FMT_VSP1_HGT => 1414550358U;
        public override uint V4L2_MODE_HIGHQUALITY => 1U;
        public override uint V4L2_MPEG_VBI_IVTV_CAPTION_525 => 4U;
        public override uint V4L2_MPEG_VBI_IVTV_MAGIC0 => 813069417U;
        public override uint V4L2_MPEG_VBI_IVTV_MAGIC1 => 810964041U;
        public override uint V4L2_MPEG_VBI_IVTV_TELETEXT_B => 1U;
        public override uint V4L2_MPEG_VBI_IVTV_VPS => 7U;
        public override uint V4L2_MPEG_VBI_IVTV_WSS_625 => 5U;
        public override uint V4L2_OUT_CAP_CUSTOM_TIMINGS => 2U;
        public override uint V4L2_OUT_CAP_DV_TIMINGS => 2U;
        public override uint V4L2_OUT_CAP_NATIVE_SIZE => 8U;
        public override uint V4L2_OUT_CAP_STD => 4U;
        public override uint V4L2_OUTPUT_TYPE_ANALOG => 2U;
        public override uint V4L2_OUTPUT_TYPE_ANALOGVGAOVERLAY => 3U;
        public override uint V4L2_OUTPUT_TYPE_MODULATOR => 1U;
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
        public override uint V4L2_PIX_FMT_NV12_10_COL128 => 808665934U;
        public override uint V4L2_PIX_FMT_NV12_COL128 => 842089294U;
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
        public override uint V4L2_PIX_FMT_Y12P => 1345466713U;
        public override uint V4L2_PIX_FMT_Y14 => 540291417U;
        public override uint V4L2_PIX_FMT_Y14P => 1345597785U;
        public override uint V4L2_PIX_FMT_Y16 => 540422489U;
        public override uint V4L2_PIX_FMT_Y16_BE => 2687906137U;
        public override uint V4L2_PIX_FMT_Y4 => 540291161U;
        public override uint V4L2_PIX_FMT_Y41P => 1345401945U;
        public override uint V4L2_PIX_FMT_Y6 => 540422233U;
        public override uint V4L2_PIX_FMT_Y8I => 541669465U;
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
        public override uint V4L2_RDS_BLOCK_A => 0U;
        public override uint V4L2_RDS_BLOCK_B => 1U;
        public override uint V4L2_RDS_BLOCK_C => 2U;
        public override uint V4L2_RDS_BLOCK_C_ALT => 4U;
        public override uint V4L2_RDS_BLOCK_CORRECTED => 64U;
        public override uint V4L2_RDS_BLOCK_D => 3U;
        public override uint V4L2_RDS_BLOCK_ERROR => 128U;
        public override uint V4L2_RDS_BLOCK_INVALID => 7U;
        public override uint V4L2_RDS_BLOCK_MSK => 7U;
        public override uint V4L2_SDR_FMT_CS14LE => 875647811U;
        public override uint V4L2_SDR_FMT_CS8 => 942691139U;
        public override uint V4L2_SDR_FMT_CU16LE => 909202755U;
        public override uint V4L2_SDR_FMT_CU8 => 942691651U;
        public override uint V4L2_SDR_FMT_PCU16BE => 909198160U;
        public override uint V4L2_SDR_FMT_PCU18BE => 942752592U;
        public override uint V4L2_SDR_FMT_PCU20BE => 808600400U;
        public override uint V4L2_SDR_FMT_RU12LE => 842093906U;
        public override uint V4L2_SLICED_CAPTION_525 => 4096U;
        public override uint V4L2_SLICED_TELETEXT_B => 1U;
        public override uint V4L2_SLICED_VBI_525 => 4096U;
        public override uint V4L2_SLICED_VBI_625 => 17409U;
        public override uint V4L2_SLICED_VPS => 1024U;
        public override uint V4L2_SLICED_WSS_625 => 16384U;
        public override uint V4L2_STD_525_60 => 63744U;
        public override uint V4L2_STD_625_50 => 16713471U;
        public override uint V4L2_STD_ALL => 16777215U;
        public override uint V4L2_STD_ATSC => 50331648U;
        public override uint V4L2_STD_ATSC_16_VSB => 33554432U;
        public override uint V4L2_STD_ATSC_8_VSB => 16777216U;
        public override uint V4L2_STD_B => 65539U;
        public override uint V4L2_STD_BG => 327687U;
        public override uint V4L2_STD_DK => 3277024U;
        public override uint V4L2_STD_G => 262148U;
        public override uint V4L2_STD_GH => 786444U;
        public override uint V4L2_STD_H => 524296U;
        public override uint V4L2_STD_L => 12582912U;
        public override uint V4L2_STD_MN => 46848U;
        public override uint V4L2_STD_MTS => 5888U;
        public override uint V4L2_STD_NTSC => 45056U;
        public override uint V4L2_STD_NTSC_443 => 16384U;
        public override uint V4L2_STD_NTSC_M => 4096U;
        public override uint V4L2_STD_NTSC_M_JP => 8192U;
        public override uint V4L2_STD_NTSC_M_KR => 32768U;
        public override uint V4L2_STD_PAL => 255U;
        public override uint V4L2_STD_PAL_60 => 2048U;
        public override uint V4L2_STD_PAL_B => 1U;
        public override uint V4L2_STD_PAL_B1 => 2U;
        public override uint V4L2_STD_PAL_BG => 7U;
        public override uint V4L2_STD_PAL_D => 32U;
        public override uint V4L2_STD_PAL_D1 => 64U;
        public override uint V4L2_STD_PAL_DK => 224U;
        public override uint V4L2_STD_PAL_G => 4U;
        public override uint V4L2_STD_PAL_H => 8U;
        public override uint V4L2_STD_PAL_I => 16U;
        public override uint V4L2_STD_PAL_K => 128U;
        public override uint V4L2_STD_PAL_M => 256U;
        public override uint V4L2_STD_PAL_N => 512U;
        public override uint V4L2_STD_PAL_Nc => 1024U;
        public override uint V4L2_STD_SECAM => 16711680U;
        public override uint V4L2_STD_SECAM_B => 65536U;
        public override uint V4L2_STD_SECAM_D => 131072U;
        public override uint V4L2_STD_SECAM_DK => 3276800U;
        public override uint V4L2_STD_SECAM_G => 262144U;
        public override uint V4L2_STD_SECAM_H => 524288U;
        public override uint V4L2_STD_SECAM_K => 1048576U;
        public override uint V4L2_STD_SECAM_K1 => 2097152U;
        public override uint V4L2_STD_SECAM_L => 4194304U;
        public override uint V4L2_STD_SECAM_LC => 8388608U;
        public override uint V4L2_STD_UNKNOWN => 0U;
        public override uint V4L2_TC_FLAG_COLORFRAME => 2U;
        public override uint V4L2_TC_FLAG_DROPFRAME => 1U;
        public override uint V4L2_TC_TYPE_24FPS => 1U;
        public override uint V4L2_TC_TYPE_25FPS => 2U;
        public override uint V4L2_TC_TYPE_30FPS => 3U;
        public override uint V4L2_TC_TYPE_50FPS => 4U;
        public override uint V4L2_TC_TYPE_60FPS => 5U;
        public override uint V4L2_TC_USERBITS_8BITCHARS => 8U;
        public override uint V4L2_TC_USERBITS_field => 12U;
        public override uint V4L2_TC_USERBITS_USERDEFINED => 0U;
        public override uint V4L2_TCH_FMT_DELTA_TD08 => 942687316U;
        public override uint V4L2_TCH_FMT_DELTA_TD16 => 909198420U;
        public override uint V4L2_TCH_FMT_TU08 => 942691668U;
        public override uint V4L2_TCH_FMT_TU16 => 909202772U;
        public override uint V4L2_TUNER_ADC => 4U;
        public override uint V4L2_TUNER_CAP_1HZ => 4096U;
        public override uint V4L2_TUNER_CAP_FREQ_BANDS => 1024U;
        public override uint V4L2_TUNER_CAP_HWSEEK_BOUNDED => 4U;
        public override uint V4L2_TUNER_CAP_HWSEEK_PROG_LIM => 2048U;
        public override uint V4L2_TUNER_CAP_HWSEEK_WRAP => 8U;
        public override uint V4L2_TUNER_CAP_LANG1 => 64U;
        public override uint V4L2_TUNER_CAP_LANG2 => 32U;
        public override uint V4L2_TUNER_CAP_LOW => 1U;
        public override uint V4L2_TUNER_CAP_NORM => 2U;
        public override uint V4L2_TUNER_CAP_RDS => 128U;
        public override uint V4L2_TUNER_CAP_RDS_BLOCK_IO => 256U;
        public override uint V4L2_TUNER_CAP_RDS_CONTROLS => 512U;
        public override uint V4L2_TUNER_CAP_SAP => 32U;
        public override uint V4L2_TUNER_CAP_STEREO => 16U;
        public override uint V4L2_TUNER_MODE_LANG1 => 3U;
        public override uint V4L2_TUNER_MODE_LANG1_LANG2 => 4U;
        public override uint V4L2_TUNER_MODE_LANG2 => 2U;
        public override uint V4L2_TUNER_MODE_MONO => 0U;
        public override uint V4L2_TUNER_MODE_SAP => 2U;
        public override uint V4L2_TUNER_MODE_STEREO => 1U;
        public override uint V4L2_TUNER_SUB_LANG1 => 8U;
        public override uint V4L2_TUNER_SUB_LANG2 => 4U;
        public override uint V4L2_TUNER_SUB_MONO => 1U;
        public override uint V4L2_TUNER_SUB_RDS => 16U;
        public override uint V4L2_TUNER_SUB_SAP => 4U;
        public override uint V4L2_TUNER_SUB_STEREO => 2U;
        public override uint V4L2_VBI_INTERLACED => 2U;
        public override uint V4L2_VBI_ITU_525_F1_START => 1U;
        public override uint V4L2_VBI_ITU_525_F2_START => 264U;
        public override uint V4L2_VBI_ITU_625_F1_START => 1U;
        public override uint V4L2_VBI_ITU_625_F2_START => 314U;
        public override uint V4L2_VBI_UNSYNC => 1U;
        public override uint V4L2_XFER_FUNC_ADOBERGB => 3U;
        public override uint VIDEO_MAX_FRAME => 32U;
        public override uint VIDEO_MAX_PLANES => 8U;
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
        [StructLayout(LayoutKind.Explicit, Size=32)]
        private new unsafe struct itimerval : NativeMethods_V4L2_Interop.itimerval
        {
            [FieldOffset(0)] private timeval it_interval_;
            public NativeMethods_V4L2_Interop.timeval it_interval
            {
                get => this.it_interval_;
                set => this.it_interval_ = (timeval)value;
            }

            [FieldOffset(16)] private timeval it_value_;
            public NativeMethods_V4L2_Interop.timeval it_value
            {
                get => this.it_value_;
                set => this.it_value_ = (timeval)value;
            }

        }
        public override NativeMethods_V4L2_Interop.itimerval Create_itimerval() => new itimerval();

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

        [StructLayout(LayoutKind.Explicit, Size=8)]
        private new unsafe struct timezone : NativeMethods_V4L2_Interop.timezone
        {
            [FieldOffset(0)] private int tz_minuteswest_;
            public int tz_minuteswest
            {
                get => this.tz_minuteswest_;
                set => this.tz_minuteswest_ = (int)value;
            }

            [FieldOffset(4)] private int tz_dsttime_;
            public int tz_dsttime
            {
                get => this.tz_dsttime_;
                set => this.tz_dsttime_ = (int)value;
            }

        }
        public override NativeMethods_V4L2_Interop.timezone Create_timezone() => new timezone();

        [StructLayout(LayoutKind.Explicit, Size=8)]
        private new unsafe struct v4l2_area : NativeMethods_V4L2_Interop.v4l2_area
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
        public override NativeMethods_V4L2_Interop.v4l2_area Create_v4l2_area() => new v4l2_area();

        [StructLayout(LayoutKind.Explicit, Size=52)]
        private new unsafe struct v4l2_audio : NativeMethods_V4L2_Interop.v4l2_audio
        {
            [FieldOffset(0)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(4)] private fixed byte name_[32];
            public byte[] name
            {
                get { fixed (byte* p = this.name_) { return get(p, 32); } }
                set { fixed (byte* p = this.name_) { set(p, value, 32); } }
            }

            [FieldOffset(36)] private uint capability_;
            public uint capability
            {
                get => this.capability_;
                set => this.capability_ = (uint)value;
            }

            [FieldOffset(40)] private uint mode_;
            public uint mode
            {
                get => this.mode_;
                set => this.mode_ = (uint)value;
            }

            [FieldOffset(44)] private fixed uint reserved_[2];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 2); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 2); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_audio Create_v4l2_audio() => new v4l2_audio();

        [StructLayout(LayoutKind.Explicit, Size=52)]
        private new unsafe struct v4l2_audioout : NativeMethods_V4L2_Interop.v4l2_audioout
        {
            [FieldOffset(0)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(4)] private fixed byte name_[32];
            public byte[] name
            {
                get { fixed (byte* p = this.name_) { return get(p, 32); } }
                set { fixed (byte* p = this.name_) { set(p, value, 32); } }
            }

            [FieldOffset(36)] private uint capability_;
            public uint capability
            {
                get => this.capability_;
                set => this.capability_ = (uint)value;
            }

            [FieldOffset(40)] private uint mode_;
            public uint mode
            {
                get => this.mode_;
                set => this.mode_ = (uint)value;
            }

            [FieldOffset(44)] private fixed uint reserved_[2];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 2); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 2); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_audioout Create_v4l2_audioout() => new v4l2_audioout();

        [StructLayout(LayoutKind.Explicit, Size=124)]
        private new unsafe struct v4l2_bt_timings : NativeMethods_V4L2_Interop.v4l2_bt_timings
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

            [FieldOffset(8)] private uint interlaced_;
            public uint interlaced
            {
                get => this.interlaced_;
                set => this.interlaced_ = (uint)value;
            }

            [FieldOffset(12)] private uint polarities_;
            public uint polarities
            {
                get => this.polarities_;
                set => this.polarities_ = (uint)value;
            }

            [FieldOffset(16)] private ulong pixelclock_;
            public ulong pixelclock
            {
                get => this.pixelclock_;
                set => this.pixelclock_ = (ulong)value;
            }

            [FieldOffset(24)] private uint hfrontporch_;
            public uint hfrontporch
            {
                get => this.hfrontporch_;
                set => this.hfrontporch_ = (uint)value;
            }

            [FieldOffset(28)] private uint hsync_;
            public uint hsync
            {
                get => this.hsync_;
                set => this.hsync_ = (uint)value;
            }

            [FieldOffset(32)] private uint hbackporch_;
            public uint hbackporch
            {
                get => this.hbackporch_;
                set => this.hbackporch_ = (uint)value;
            }

            [FieldOffset(36)] private uint vfrontporch_;
            public uint vfrontporch
            {
                get => this.vfrontporch_;
                set => this.vfrontporch_ = (uint)value;
            }

            [FieldOffset(40)] private uint vsync_;
            public uint vsync
            {
                get => this.vsync_;
                set => this.vsync_ = (uint)value;
            }

            [FieldOffset(44)] private uint vbackporch_;
            public uint vbackporch
            {
                get => this.vbackporch_;
                set => this.vbackporch_ = (uint)value;
            }

            [FieldOffset(48)] private uint il_vfrontporch_;
            public uint il_vfrontporch
            {
                get => this.il_vfrontporch_;
                set => this.il_vfrontporch_ = (uint)value;
            }

            [FieldOffset(52)] private uint il_vsync_;
            public uint il_vsync
            {
                get => this.il_vsync_;
                set => this.il_vsync_ = (uint)value;
            }

            [FieldOffset(56)] private uint il_vbackporch_;
            public uint il_vbackporch
            {
                get => this.il_vbackporch_;
                set => this.il_vbackporch_ = (uint)value;
            }

            [FieldOffset(60)] private uint standards_;
            public uint standards
            {
                get => this.standards_;
                set => this.standards_ = (uint)value;
            }

            [FieldOffset(64)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(68)] private v4l2_fract picture_aspect_;
            public NativeMethods_V4L2_Interop.v4l2_fract picture_aspect
            {
                get => this.picture_aspect_;
                set => this.picture_aspect_ = (v4l2_fract)value;
            }

            [FieldOffset(76)] private byte cea861_vic_;
            public byte cea861_vic
            {
                get => this.cea861_vic_;
                set => this.cea861_vic_ = (byte)value;
            }

            [FieldOffset(77)] private byte hdmi_vic_;
            public byte hdmi_vic
            {
                get => this.hdmi_vic_;
                set => this.hdmi_vic_ = (byte)value;
            }

            [FieldOffset(78)] private fixed byte reserved_[46];
            public byte[] reserved
            {
                get { fixed (byte* p = this.reserved_) { return get(p, 46); } }
                set { fixed (byte* p = this.reserved_) { set(p, value, 46); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_bt_timings Create_v4l2_bt_timings() => new v4l2_bt_timings();

        [StructLayout(LayoutKind.Explicit, Size=104)]
        private new unsafe struct v4l2_bt_timings_cap : NativeMethods_V4L2_Interop.v4l2_bt_timings_cap
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

            [FieldOffset(8)] private uint min_height_;
            public uint min_height
            {
                get => this.min_height_;
                set => this.min_height_ = (uint)value;
            }

            [FieldOffset(12)] private uint max_height_;
            public uint max_height
            {
                get => this.max_height_;
                set => this.max_height_ = (uint)value;
            }

            [FieldOffset(16)] private ulong min_pixelclock_;
            public ulong min_pixelclock
            {
                get => this.min_pixelclock_;
                set => this.min_pixelclock_ = (ulong)value;
            }

            [FieldOffset(24)] private ulong max_pixelclock_;
            public ulong max_pixelclock
            {
                get => this.max_pixelclock_;
                set => this.max_pixelclock_ = (ulong)value;
            }

            [FieldOffset(32)] private uint standards_;
            public uint standards
            {
                get => this.standards_;
                set => this.standards_ = (uint)value;
            }

            [FieldOffset(36)] private uint capabilities_;
            public uint capabilities
            {
                get => this.capabilities_;
                set => this.capabilities_ = (uint)value;
            }

            [FieldOffset(40)] private fixed uint reserved_[16];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 16); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 16); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_bt_timings_cap Create_v4l2_bt_timings_cap() => new v4l2_bt_timings_cap();

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

        [StructLayout(LayoutKind.Explicit, Size=40)]
        private new unsafe struct v4l2_captureparm : NativeMethods_V4L2_Interop.v4l2_captureparm
        {
            [FieldOffset(0)] private uint capability_;
            public uint capability
            {
                get => this.capability_;
                set => this.capability_ = (uint)value;
            }

            [FieldOffset(4)] private uint capturemode_;
            public uint capturemode
            {
                get => this.capturemode_;
                set => this.capturemode_ = (uint)value;
            }

            [FieldOffset(8)] private v4l2_fract timeperframe_;
            public NativeMethods_V4L2_Interop.v4l2_fract timeperframe
            {
                get => this.timeperframe_;
                set => this.timeperframe_ = (v4l2_fract)value;
            }

            [FieldOffset(16)] private uint extendedmode_;
            public uint extendedmode
            {
                get => this.extendedmode_;
                set => this.extendedmode_ = (uint)value;
            }

            [FieldOffset(20)] private uint readbuffers_;
            public uint readbuffers
            {
                get => this.readbuffers_;
                set => this.readbuffers_ = (uint)value;
            }

            [FieldOffset(24)] private fixed uint reserved_[4];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 4); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 4); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_captureparm Create_v4l2_captureparm() => new v4l2_captureparm();

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

        [StructLayout(LayoutKind.Explicit, Size=8)]
        private new unsafe struct v4l2_control : NativeMethods_V4L2_Interop.v4l2_control
        {
            [FieldOffset(0)] private uint id_;
            public uint id
            {
                get => this.id_;
                set => this.id_ = (uint)value;
            }

            [FieldOffset(4)] private int value_;
            public int value
            {
                get => this.value_;
                set => this.value_ = (int)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_control Create_v4l2_control() => new v4l2_control();

        [StructLayout(LayoutKind.Explicit, Size=256)]
        private new unsafe struct v4l2_create_buffers : NativeMethods_V4L2_Interop.v4l2_create_buffers
        {
            [FieldOffset(0)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(4)] private uint count_;
            public uint count
            {
                get => this.count_;
                set => this.count_ = (uint)value;
            }

            [FieldOffset(8)] private uint memory_;
            public uint memory
            {
                get => this.memory_;
                set => this.memory_ = (uint)value;
            }

            [FieldOffset(16)] private v4l2_format format_;
            public NativeMethods_V4L2_Interop.v4l2_format format
            {
                get => this.format_;
                set => this.format_ = (v4l2_format)value;
            }

            [FieldOffset(224)] private uint capabilities_;
            public uint capabilities
            {
                get => this.capabilities_;
                set => this.capabilities_ = (uint)value;
            }

            [FieldOffset(228)] private fixed uint reserved_[7];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 7); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 7); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_create_buffers Create_v4l2_create_buffers() => new v4l2_create_buffers();

        [StructLayout(LayoutKind.Explicit, Size=20)]
        private new unsafe struct v4l2_crop : NativeMethods_V4L2_Interop.v4l2_crop
        {
            [FieldOffset(0)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(4)] private v4l2_rect c_;
            public NativeMethods_V4L2_Interop.v4l2_rect c
            {
                get => this.c_;
                set => this.c_ = (v4l2_rect)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_crop Create_v4l2_crop() => new v4l2_crop();

        [StructLayout(LayoutKind.Explicit, Size=44)]
        private new unsafe struct v4l2_cropcap : NativeMethods_V4L2_Interop.v4l2_cropcap
        {
            [FieldOffset(0)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(4)] private v4l2_rect bounds_;
            public NativeMethods_V4L2_Interop.v4l2_rect bounds
            {
                get => this.bounds_;
                set => this.bounds_ = (v4l2_rect)value;
            }

            [FieldOffset(20)] private v4l2_rect defrect_;
            public NativeMethods_V4L2_Interop.v4l2_rect defrect
            {
                get => this.defrect_;
                set => this.defrect_ = (v4l2_rect)value;
            }

            [FieldOffset(36)] private v4l2_fract pixelaspect_;
            public NativeMethods_V4L2_Interop.v4l2_fract pixelaspect
            {
                get => this.pixelaspect_;
                set => this.pixelaspect_ = (v4l2_fract)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_cropcap Create_v4l2_cropcap() => new v4l2_cropcap();

        [StructLayout(LayoutKind.Explicit, Size=200)]
        private new unsafe struct v4l2_dbg_chip_info : NativeMethods_V4L2_Interop.v4l2_dbg_chip_info
        {
            [FieldOffset(0)] private v4l2_dbg_match match_;
            public NativeMethods_V4L2_Interop.v4l2_dbg_match match
            {
                get => this.match_;
                set => this.match_ = (v4l2_dbg_match)value;
            }

            [FieldOffset(36)] private fixed byte name_[32];   // char
            public byte[] name
            {
                get { fixed (byte* p = this.name_) { return get(p, 32); } }
                set { fixed (byte* p = this.name_) { set(p, value, 32); } }
            }

            [FieldOffset(68)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(72)] private fixed uint reserved_[32];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 32); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 32); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_dbg_chip_info Create_v4l2_dbg_chip_info() => new v4l2_dbg_chip_info();

        [StructLayout(LayoutKind.Explicit, Size=36)]
        private new unsafe struct v4l2_dbg_match : NativeMethods_V4L2_Interop.v4l2_dbg_match
        {
            [FieldOffset(0)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(4)] private uint addr_;
            public uint addr
            {
                get => this.addr_;
                set => this.addr_ = (uint)value;
            }

            [FieldOffset(4)] private fixed byte name_[32];   // char
            public byte[] name
            {
                get { fixed (byte* p = this.name_) { return get(p, 32); } }
                set { fixed (byte* p = this.name_) { set(p, value, 32); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_dbg_match Create_v4l2_dbg_match() => new v4l2_dbg_match();

        [StructLayout(LayoutKind.Explicit, Size=56)]
        private new unsafe struct v4l2_dbg_register : NativeMethods_V4L2_Interop.v4l2_dbg_register
        {
            [FieldOffset(0)] private v4l2_dbg_match match_;
            public NativeMethods_V4L2_Interop.v4l2_dbg_match match
            {
                get => this.match_;
                set => this.match_ = (v4l2_dbg_match)value;
            }

            [FieldOffset(36)] private uint size_;
            public uint size
            {
                get => this.size_;
                set => this.size_ = (uint)value;
            }

            [FieldOffset(40)] private ulong reg_;
            public ulong reg
            {
                get => this.reg_;
                set => this.reg_ = (ulong)value;
            }

            [FieldOffset(48)] private ulong val_;
            public ulong val
            {
                get => this.val_;
                set => this.val_ = (ulong)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_dbg_register Create_v4l2_dbg_register() => new v4l2_dbg_register();

        [StructLayout(LayoutKind.Explicit, Size=72)]
        private new unsafe struct v4l2_decoder_cmd : NativeMethods_V4L2_Interop.v4l2_decoder_cmd
        {
            [FieldOffset(0)] private uint cmd_;
            public uint cmd
            {
                get => this.cmd_;
                set => this.cmd_ = (uint)value;
            }

            [FieldOffset(4)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(8)] private ulong stop_pts_;
            public ulong stop_pts
            {
                get => this.stop_pts_;
                set => this.stop_pts_ = (ulong)value;
            }

            [FieldOffset(8)] private int start_speed_;
            public int start_speed
            {
                get => this.start_speed_;
                set => this.start_speed_ = (int)value;
            }

            [FieldOffset(8)] private fixed uint raw_data_[16];
            public uint[] raw_data
            {
                get { fixed (uint* p = this.raw_data_) { return get(p, 16); } }
                set { fixed (uint* p = this.raw_data_) { set(p, value, 16); } }
            }

            [FieldOffset(12)] private uint start_format_;
            public uint start_format
            {
                get => this.start_format_;
                set => this.start_format_ = (uint)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_decoder_cmd Create_v4l2_decoder_cmd() => new v4l2_decoder_cmd();

        [StructLayout(LayoutKind.Explicit, Size=132)]
        private new unsafe struct v4l2_dv_timings : NativeMethods_V4L2_Interop.v4l2_dv_timings
        {
            [FieldOffset(0)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(4)] private v4l2_bt_timings bt_;
            public NativeMethods_V4L2_Interop.v4l2_bt_timings bt
            {
                get => this.bt_;
                set => this.bt_ = (v4l2_bt_timings)value;
            }

            [FieldOffset(4)] private fixed uint reserved_[32];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 32); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 32); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_dv_timings Create_v4l2_dv_timings() => new v4l2_dv_timings();

        [StructLayout(LayoutKind.Explicit, Size=144)]
        private new unsafe struct v4l2_dv_timings_cap : NativeMethods_V4L2_Interop.v4l2_dv_timings_cap
        {
            [FieldOffset(0)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(4)] private uint pad_;
            public uint pad
            {
                get => this.pad_;
                set => this.pad_ = (uint)value;
            }

            [FieldOffset(8)] private fixed uint reserved_[2];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 2); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 2); } }
            }

            [FieldOffset(16)] private v4l2_bt_timings_cap bt_;
            public NativeMethods_V4L2_Interop.v4l2_bt_timings_cap bt
            {
                get => this.bt_;
                set => this.bt_ = (v4l2_bt_timings_cap)value;
            }

            [FieldOffset(16)] private fixed uint raw_data_[32];
            public uint[] raw_data
            {
                get { fixed (uint* p = this.raw_data_) { return get(p, 32); } }
                set { fixed (uint* p = this.raw_data_) { set(p, value, 32); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_dv_timings_cap Create_v4l2_dv_timings_cap() => new v4l2_dv_timings_cap();

        [StructLayout(LayoutKind.Explicit, Size=40)]
        private new unsafe struct v4l2_edid : NativeMethods_V4L2_Interop.v4l2_edid
        {
            [FieldOffset(0)] private uint pad_;
            public uint pad
            {
                get => this.pad_;
                set => this.pad_ = (uint)value;
            }

            [FieldOffset(4)] private uint start_block_;
            public uint start_block
            {
                get => this.start_block_;
                set => this.start_block_ = (uint)value;
            }

            [FieldOffset(8)] private uint blocks_;
            public uint blocks
            {
                get => this.blocks_;
                set => this.blocks_ = (uint)value;
            }

            [FieldOffset(12)] private fixed uint reserved_[5];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 5); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 5); } }
            }

            [FieldOffset(32)] private byte* edid_;
            public IntPtr edid
            {
                get => (IntPtr)this.edid_;
                set => this.edid_ = (byte*)value.ToPointer();
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_edid Create_v4l2_edid() => new v4l2_edid();

        [StructLayout(LayoutKind.Explicit, Size=2072)]
        private new unsafe struct v4l2_enc_idx : NativeMethods_V4L2_Interop.v4l2_enc_idx
        {
            [FieldOffset(0)] private uint entries_;
            public uint entries
            {
                get => this.entries_;
                set => this.entries_ = (uint)value;
            }

            [FieldOffset(4)] private uint entries_cap_;
            public uint entries_cap
            {
                get => this.entries_cap_;
                set => this.entries_cap_ = (uint)value;
            }

            [FieldOffset(8)] private fixed uint reserved_[4];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 4); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 4); } }
            }

            [FieldOffset(24)] private fixed byte entry_[32 * 64];   // sizeof(v4l2_enc_idx_entry): 32
            public NativeMethods_V4L2_Interop.v4l2_enc_idx_entry[] entry
            {
                get { fixed (byte* p = this.entry_) { return get<v4l2_enc_idx_entry, NativeMethods_V4L2_Interop.v4l2_enc_idx_entry>(p, 32, 64); } }
                set { fixed (byte* p = this.entry_) { set<v4l2_enc_idx_entry, NativeMethods_V4L2_Interop.v4l2_enc_idx_entry>(p, value, 32, 64); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_enc_idx Create_v4l2_enc_idx() => new v4l2_enc_idx();

        [StructLayout(LayoutKind.Explicit, Size=32)]
        private new unsafe struct v4l2_enc_idx_entry : NativeMethods_V4L2_Interop.v4l2_enc_idx_entry
        {
            [FieldOffset(0)] private ulong offset_;
            public ulong offset
            {
                get => this.offset_;
                set => this.offset_ = (ulong)value;
            }

            [FieldOffset(8)] private ulong pts_;
            public ulong pts
            {
                get => this.pts_;
                set => this.pts_ = (ulong)value;
            }

            [FieldOffset(16)] private uint length_;
            public uint length
            {
                get => this.length_;
                set => this.length_ = (uint)value;
            }

            [FieldOffset(20)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(24)] private fixed uint reserved_[2];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 2); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 2); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_enc_idx_entry Create_v4l2_enc_idx_entry() => new v4l2_enc_idx_entry();

        [StructLayout(LayoutKind.Explicit, Size=40)]
        private new unsafe struct v4l2_encoder_cmd : NativeMethods_V4L2_Interop.v4l2_encoder_cmd
        {
            [FieldOffset(0)] private uint cmd_;
            public uint cmd
            {
                get => this.cmd_;
                set => this.cmd_ = (uint)value;
            }

            [FieldOffset(4)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(8)] private fixed uint raw_data_[8];
            public uint[] raw_data
            {
                get { fixed (uint* p = this.raw_data_) { return get(p, 8); } }
                set { fixed (uint* p = this.raw_data_) { set(p, value, 8); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_encoder_cmd Create_v4l2_encoder_cmd() => new v4l2_encoder_cmd();

        [StructLayout(LayoutKind.Explicit, Size=148)]
        private new unsafe struct v4l2_enum_dv_timings : NativeMethods_V4L2_Interop.v4l2_enum_dv_timings
        {
            [FieldOffset(0)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(4)] private uint pad_;
            public uint pad
            {
                get => this.pad_;
                set => this.pad_ = (uint)value;
            }

            [FieldOffset(8)] private fixed uint reserved_[2];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 2); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 2); } }
            }

            [FieldOffset(16)] private v4l2_dv_timings timings_;
            public NativeMethods_V4L2_Interop.v4l2_dv_timings timings
            {
                get => this.timings_;
                set => this.timings_ = (v4l2_dv_timings)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_enum_dv_timings Create_v4l2_enum_dv_timings() => new v4l2_enum_dv_timings();

        [StructLayout(LayoutKind.Explicit, Size=136)]
        private new unsafe struct v4l2_event : NativeMethods_V4L2_Interop.v4l2_event
        {
            [FieldOffset(0)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(8)] private v4l2_event_vsync u_vsync_;
            public NativeMethods_V4L2_Interop.v4l2_event_vsync u_vsync
            {
                get => this.u_vsync_;
                set => this.u_vsync_ = (v4l2_event_vsync)value;
            }

            [FieldOffset(8)] private v4l2_event_ctrl u_ctrl_;
            public NativeMethods_V4L2_Interop.v4l2_event_ctrl u_ctrl
            {
                get => this.u_ctrl_;
                set => this.u_ctrl_ = (v4l2_event_ctrl)value;
            }

            [FieldOffset(8)] private v4l2_event_frame_sync u_frame_sync_;
            public NativeMethods_V4L2_Interop.v4l2_event_frame_sync u_frame_sync
            {
                get => this.u_frame_sync_;
                set => this.u_frame_sync_ = (v4l2_event_frame_sync)value;
            }

            [FieldOffset(8)] private v4l2_event_src_change u_src_change_;
            public NativeMethods_V4L2_Interop.v4l2_event_src_change u_src_change
            {
                get => this.u_src_change_;
                set => this.u_src_change_ = (v4l2_event_src_change)value;
            }

            [FieldOffset(8)] private v4l2_event_motion_det u_motion_det_;
            public NativeMethods_V4L2_Interop.v4l2_event_motion_det u_motion_det
            {
                get => this.u_motion_det_;
                set => this.u_motion_det_ = (v4l2_event_motion_det)value;
            }

            [FieldOffset(8)] private fixed byte u_data_[64];
            public byte[] u_data
            {
                get { fixed (byte* p = this.u_data_) { return get(p, 64); } }
                set { fixed (byte* p = this.u_data_) { set(p, value, 64); } }
            }

            [FieldOffset(72)] private uint pending_;
            public uint pending
            {
                get => this.pending_;
                set => this.pending_ = (uint)value;
            }

            [FieldOffset(76)] private uint sequence_;
            public uint sequence
            {
                get => this.sequence_;
                set => this.sequence_ = (uint)value;
            }

            [FieldOffset(80)] private timespec timestamp_;
            public NativeMethods_V4L2_Interop.timespec timestamp
            {
                get => this.timestamp_;
                set => this.timestamp_ = (timespec)value;
            }

            [FieldOffset(96)] private uint id_;
            public uint id
            {
                get => this.id_;
                set => this.id_ = (uint)value;
            }

            [FieldOffset(100)] private fixed uint reserved_[8];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 8); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 8); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_event Create_v4l2_event() => new v4l2_event();

        [StructLayout(LayoutKind.Explicit, Size=40)]
        private new unsafe struct v4l2_event_ctrl : NativeMethods_V4L2_Interop.v4l2_event_ctrl
        {
            [FieldOffset(0)] private uint changes_;
            public uint changes
            {
                get => this.changes_;
                set => this.changes_ = (uint)value;
            }

            [FieldOffset(4)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(8)] private int value_;
            public int value
            {
                get => this.value_;
                set => this.value_ = (int)value;
            }

            [FieldOffset(8)] private long value64_;
            public long value64
            {
                get => this.value64_;
                set => this.value64_ = (long)value;
            }

            [FieldOffset(16)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(20)] private int minimum_;
            public int minimum
            {
                get => this.minimum_;
                set => this.minimum_ = (int)value;
            }

            [FieldOffset(24)] private int maximum_;
            public int maximum
            {
                get => this.maximum_;
                set => this.maximum_ = (int)value;
            }

            [FieldOffset(28)] private int step_;
            public int step
            {
                get => this.step_;
                set => this.step_ = (int)value;
            }

            [FieldOffset(32)] private int default_value_;
            public int default_value
            {
                get => this.default_value_;
                set => this.default_value_ = (int)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_event_ctrl Create_v4l2_event_ctrl() => new v4l2_event_ctrl();

        [StructLayout(LayoutKind.Explicit, Size=4)]
        private new unsafe struct v4l2_event_frame_sync : NativeMethods_V4L2_Interop.v4l2_event_frame_sync
        {
            [FieldOffset(0)] private uint frame_sequence_;
            public uint frame_sequence
            {
                get => this.frame_sequence_;
                set => this.frame_sequence_ = (uint)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_event_frame_sync Create_v4l2_event_frame_sync() => new v4l2_event_frame_sync();

        [StructLayout(LayoutKind.Explicit, Size=12)]
        private new unsafe struct v4l2_event_motion_det : NativeMethods_V4L2_Interop.v4l2_event_motion_det
        {
            [FieldOffset(0)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(4)] private uint frame_sequence_;
            public uint frame_sequence
            {
                get => this.frame_sequence_;
                set => this.frame_sequence_ = (uint)value;
            }

            [FieldOffset(8)] private uint region_mask_;
            public uint region_mask
            {
                get => this.region_mask_;
                set => this.region_mask_ = (uint)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_event_motion_det Create_v4l2_event_motion_det() => new v4l2_event_motion_det();

        [StructLayout(LayoutKind.Explicit, Size=4)]
        private new unsafe struct v4l2_event_src_change : NativeMethods_V4L2_Interop.v4l2_event_src_change
        {
            [FieldOffset(0)] private uint changes_;
            public uint changes
            {
                get => this.changes_;
                set => this.changes_ = (uint)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_event_src_change Create_v4l2_event_src_change() => new v4l2_event_src_change();

        [StructLayout(LayoutKind.Explicit, Size=32)]
        private new unsafe struct v4l2_event_subscription : NativeMethods_V4L2_Interop.v4l2_event_subscription
        {
            [FieldOffset(0)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(4)] private uint id_;
            public uint id
            {
                get => this.id_;
                set => this.id_ = (uint)value;
            }

            [FieldOffset(8)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(12)] private fixed uint reserved_[5];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 5); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 5); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_event_subscription Create_v4l2_event_subscription() => new v4l2_event_subscription();

        [StructLayout(LayoutKind.Explicit, Size=1)]
        private new unsafe struct v4l2_event_vsync : NativeMethods_V4L2_Interop.v4l2_event_vsync
        {
            [FieldOffset(0)] private byte field_;
            public byte field
            {
                get => this.field_;
                set => this.field_ = (byte)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_event_vsync Create_v4l2_event_vsync() => new v4l2_event_vsync();

        [StructLayout(LayoutKind.Explicit, Size=64)]
        private new unsafe struct v4l2_exportbuffer : NativeMethods_V4L2_Interop.v4l2_exportbuffer
        {
            [FieldOffset(0)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(4)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(8)] private uint plane_;
            public uint plane
            {
                get => this.plane_;
                set => this.plane_ = (uint)value;
            }

            [FieldOffset(12)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(16)] private int fd_;
            public int fd
            {
                get => this.fd_;
                set => this.fd_ = (int)value;
            }

            [FieldOffset(20)] private fixed uint reserved_[11];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 11); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 11); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_exportbuffer Create_v4l2_exportbuffer() => new v4l2_exportbuffer();

        [StructLayout(LayoutKind.Explicit, Size=20)]
        private new unsafe struct v4l2_ext_control : NativeMethods_V4L2_Interop.v4l2_ext_control
        {
            [FieldOffset(0)] private uint id_;
            public uint id
            {
                get => this.id_;
                set => this.id_ = (uint)value;
            }

            [FieldOffset(4)] private uint size_;
            public uint size
            {
                get => this.size_;
                set => this.size_ = (uint)value;
            }

            [FieldOffset(8)] private fixed uint reserved2_[1];
            public uint[] reserved2
            {
                get { fixed (uint* p = this.reserved2_) { return get(p, 1); } }
                set { fixed (uint* p = this.reserved2_) { set(p, value, 1); } }
            }

            [FieldOffset(12)] private int value_;
            public int value
            {
                get => this.value_;
                set => this.value_ = (int)value;
            }

            [FieldOffset(12)] private long value64_;
            public long value64
            {
                get => this.value64_;
                set => this.value64_ = (long)value;
            }

            [FieldOffset(12)] private byte* string__;   // char
            public IntPtr string_
            {
                get => (IntPtr)this.string__;
                set => this.string__ = (byte*)value.ToPointer();
            }

            [FieldOffset(12)] private byte* p_u8_;
            public IntPtr p_u8
            {
                get => (IntPtr)this.p_u8_;
                set => this.p_u8_ = (byte*)value.ToPointer();
            }

            [FieldOffset(12)] private ushort* p_u16_;
            public IntPtr p_u16
            {
                get => (IntPtr)this.p_u16_;
                set => this.p_u16_ = (ushort*)value.ToPointer();
            }

            [FieldOffset(12)] private uint* p_u32_;
            public IntPtr p_u32
            {
                get => (IntPtr)this.p_u32_;
                set => this.p_u32_ = (uint*)value.ToPointer();
            }

            [FieldOffset(12)] private v4l2_area* p_area_;
            public IntPtr p_area
            {
                get => (IntPtr)this.p_area_;
                set => this.p_area_ = (v4l2_area*)value.ToPointer();
            }

            [FieldOffset(12)] private void* ptr_;
            public IntPtr ptr
            {
                get => (IntPtr)this.ptr_;
                set => this.ptr_ = (void*)value.ToPointer();
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_ext_control Create_v4l2_ext_control() => new v4l2_ext_control();

        [StructLayout(LayoutKind.Explicit, Size=32)]
        private new unsafe struct v4l2_ext_controls : NativeMethods_V4L2_Interop.v4l2_ext_controls
        {
            [FieldOffset(0)] private uint ctrl_class_;
            public uint ctrl_class
            {
                get => this.ctrl_class_;
                set => this.ctrl_class_ = (uint)value;
            }

            [FieldOffset(0)] private uint which_;
            public uint which
            {
                get => this.which_;
                set => this.which_ = (uint)value;
            }

            [FieldOffset(4)] private uint count_;
            public uint count
            {
                get => this.count_;
                set => this.count_ = (uint)value;
            }

            [FieldOffset(8)] private uint error_idx_;
            public uint error_idx
            {
                get => this.error_idx_;
                set => this.error_idx_ = (uint)value;
            }

            [FieldOffset(12)] private int request_fd_;
            public int request_fd
            {
                get => this.request_fd_;
                set => this.request_fd_ = (int)value;
            }

            [FieldOffset(16)] private fixed uint reserved_[1];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 1); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 1); } }
            }

            [FieldOffset(24)] private v4l2_ext_control* controls_;
            public IntPtr controls
            {
                get => (IntPtr)this.controls_;
                set => this.controls_ = (v4l2_ext_control*)value.ToPointer();
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_ext_controls Create_v4l2_ext_controls() => new v4l2_ext_controls();

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

        [StructLayout(LayoutKind.Explicit, Size=48)]
        private new unsafe struct v4l2_framebuffer : NativeMethods_V4L2_Interop.v4l2_framebuffer
        {
            [FieldOffset(0)] private uint capability_;
            public uint capability
            {
                get => this.capability_;
                set => this.capability_ = (uint)value;
            }

            [FieldOffset(4)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(8)] private void* base__;
            public IntPtr base_
            {
                get => (IntPtr)this.base__;
                set => this.base__ = (void*)value.ToPointer();
            }

            [FieldOffset(16)] private uint fmt_width_;
            public uint fmt_width
            {
                get => this.fmt_width_;
                set => this.fmt_width_ = (uint)value;
            }

            [FieldOffset(20)] private uint fmt_height_;
            public uint fmt_height
            {
                get => this.fmt_height_;
                set => this.fmt_height_ = (uint)value;
            }

            [FieldOffset(24)] private uint fmt_pixelformat_;
            public uint fmt_pixelformat
            {
                get => this.fmt_pixelformat_;
                set => this.fmt_pixelformat_ = (uint)value;
            }

            [FieldOffset(28)] private uint fmt_field_;
            public uint fmt_field
            {
                get => this.fmt_field_;
                set => this.fmt_field_ = (uint)value;
            }

            [FieldOffset(32)] private uint fmt_bytesperline_;
            public uint fmt_bytesperline
            {
                get => this.fmt_bytesperline_;
                set => this.fmt_bytesperline_ = (uint)value;
            }

            [FieldOffset(36)] private uint fmt_sizeimage_;
            public uint fmt_sizeimage
            {
                get => this.fmt_sizeimage_;
                set => this.fmt_sizeimage_ = (uint)value;
            }

            [FieldOffset(40)] private uint fmt_colorspace_;
            public uint fmt_colorspace
            {
                get => this.fmt_colorspace_;
                set => this.fmt_colorspace_ = (uint)value;
            }

            [FieldOffset(44)] private uint fmt_priv_;
            public uint fmt_priv
            {
                get => this.fmt_priv_;
                set => this.fmt_priv_ = (uint)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_framebuffer Create_v4l2_framebuffer() => new v4l2_framebuffer();

        [StructLayout(LayoutKind.Explicit, Size=44)]
        private new unsafe struct v4l2_frequency : NativeMethods_V4L2_Interop.v4l2_frequency
        {
            [FieldOffset(0)] private uint tuner_;
            public uint tuner
            {
                get => this.tuner_;
                set => this.tuner_ = (uint)value;
            }

            [FieldOffset(4)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(8)] private uint frequency_;
            public uint frequency
            {
                get => this.frequency_;
                set => this.frequency_ = (uint)value;
            }

            [FieldOffset(12)] private fixed uint reserved_[8];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 8); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 8); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_frequency Create_v4l2_frequency() => new v4l2_frequency();

        [StructLayout(LayoutKind.Explicit, Size=64)]
        private new unsafe struct v4l2_frequency_band : NativeMethods_V4L2_Interop.v4l2_frequency_band
        {
            [FieldOffset(0)] private uint tuner_;
            public uint tuner
            {
                get => this.tuner_;
                set => this.tuner_ = (uint)value;
            }

            [FieldOffset(4)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(8)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(12)] private uint capability_;
            public uint capability
            {
                get => this.capability_;
                set => this.capability_ = (uint)value;
            }

            [FieldOffset(16)] private uint rangelow_;
            public uint rangelow
            {
                get => this.rangelow_;
                set => this.rangelow_ = (uint)value;
            }

            [FieldOffset(20)] private uint rangehigh_;
            public uint rangehigh
            {
                get => this.rangehigh_;
                set => this.rangehigh_ = (uint)value;
            }

            [FieldOffset(24)] private uint modulation_;
            public uint modulation
            {
                get => this.modulation_;
                set => this.modulation_ = (uint)value;
            }

            [FieldOffset(28)] private fixed uint reserved_[9];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 9); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 9); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_frequency_band Create_v4l2_frequency_band() => new v4l2_frequency_band();

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

        [StructLayout(LayoutKind.Explicit, Size=48)]
        private new unsafe struct v4l2_hw_freq_seek : NativeMethods_V4L2_Interop.v4l2_hw_freq_seek
        {
            [FieldOffset(0)] private uint tuner_;
            public uint tuner
            {
                get => this.tuner_;
                set => this.tuner_ = (uint)value;
            }

            [FieldOffset(4)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(8)] private uint seek_upward_;
            public uint seek_upward
            {
                get => this.seek_upward_;
                set => this.seek_upward_ = (uint)value;
            }

            [FieldOffset(12)] private uint wrap_around_;
            public uint wrap_around
            {
                get => this.wrap_around_;
                set => this.wrap_around_ = (uint)value;
            }

            [FieldOffset(16)] private uint spacing_;
            public uint spacing
            {
                get => this.spacing_;
                set => this.spacing_ = (uint)value;
            }

            [FieldOffset(20)] private uint rangelow_;
            public uint rangelow
            {
                get => this.rangelow_;
                set => this.rangelow_ = (uint)value;
            }

            [FieldOffset(24)] private uint rangehigh_;
            public uint rangehigh
            {
                get => this.rangehigh_;
                set => this.rangehigh_ = (uint)value;
            }

            [FieldOffset(28)] private fixed uint reserved_[5];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 5); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 5); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_hw_freq_seek Create_v4l2_hw_freq_seek() => new v4l2_hw_freq_seek();

        [StructLayout(LayoutKind.Explicit, Size=80)]
        private new unsafe struct v4l2_input : NativeMethods_V4L2_Interop.v4l2_input
        {
            [FieldOffset(0)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(4)] private fixed byte name_[32];
            public byte[] name
            {
                get { fixed (byte* p = this.name_) { return get(p, 32); } }
                set { fixed (byte* p = this.name_) { set(p, value, 32); } }
            }

            [FieldOffset(36)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(40)] private uint audioset_;
            public uint audioset
            {
                get => this.audioset_;
                set => this.audioset_ = (uint)value;
            }

            [FieldOffset(44)] private uint tuner_;
            public uint tuner
            {
                get => this.tuner_;
                set => this.tuner_ = (uint)value;
            }

            [FieldOffset(48)] private ulong std_;
            public ulong std
            {
                get => this.std_;
                set => this.std_ = (ulong)value;
            }

            [FieldOffset(56)] private uint status_;
            public uint status
            {
                get => this.status_;
                set => this.status_ = (uint)value;
            }

            [FieldOffset(60)] private uint capabilities_;
            public uint capabilities
            {
                get => this.capabilities_;
                set => this.capabilities_ = (uint)value;
            }

            [FieldOffset(64)] private fixed uint reserved_[3];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 3); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 3); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_input Create_v4l2_input() => new v4l2_input();

        [StructLayout(LayoutKind.Explicit, Size=140)]
        private new unsafe struct v4l2_jpegcompression : NativeMethods_V4L2_Interop.v4l2_jpegcompression
        {
            [FieldOffset(0)] private int quality_;
            public int quality
            {
                get => this.quality_;
                set => this.quality_ = (int)value;
            }

            [FieldOffset(4)] private int APPn_;
            public int APPn
            {
                get => this.APPn_;
                set => this.APPn_ = (int)value;
            }

            [FieldOffset(8)] private int APP_len_;
            public int APP_len
            {
                get => this.APP_len_;
                set => this.APP_len_ = (int)value;
            }

            [FieldOffset(12)] private fixed byte APP_data_[60];   // char
            public byte[] APP_data
            {
                get { fixed (byte* p = this.APP_data_) { return get(p, 60); } }
                set { fixed (byte* p = this.APP_data_) { set(p, value, 60); } }
            }

            [FieldOffset(72)] private int COM_len_;
            public int COM_len
            {
                get => this.COM_len_;
                set => this.COM_len_ = (int)value;
            }

            [FieldOffset(76)] private fixed byte COM_data_[60];   // char
            public byte[] COM_data
            {
                get { fixed (byte* p = this.COM_data_) { return get(p, 60); } }
                set { fixed (byte* p = this.COM_data_) { set(p, value, 60); } }
            }

            [FieldOffset(136)] private uint jpeg_markers_;
            public uint jpeg_markers
            {
                get => this.jpeg_markers_;
                set => this.jpeg_markers_ = (uint)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_jpegcompression Create_v4l2_jpegcompression() => new v4l2_jpegcompression();

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

        [StructLayout(LayoutKind.Explicit, Size=68)]
        private new unsafe struct v4l2_modulator : NativeMethods_V4L2_Interop.v4l2_modulator
        {
            [FieldOffset(0)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(4)] private fixed byte name_[32];
            public byte[] name
            {
                get { fixed (byte* p = this.name_) { return get(p, 32); } }
                set { fixed (byte* p = this.name_) { set(p, value, 32); } }
            }

            [FieldOffset(36)] private uint capability_;
            public uint capability
            {
                get => this.capability_;
                set => this.capability_ = (uint)value;
            }

            [FieldOffset(40)] private uint rangelow_;
            public uint rangelow
            {
                get => this.rangelow_;
                set => this.rangelow_ = (uint)value;
            }

            [FieldOffset(44)] private uint rangehigh_;
            public uint rangehigh
            {
                get => this.rangehigh_;
                set => this.rangehigh_ = (uint)value;
            }

            [FieldOffset(48)] private uint txsubchans_;
            public uint txsubchans
            {
                get => this.txsubchans_;
                set => this.txsubchans_ = (uint)value;
            }

            [FieldOffset(52)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(56)] private fixed uint reserved_[3];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 3); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 3); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_modulator Create_v4l2_modulator() => new v4l2_modulator();

        [StructLayout(LayoutKind.Explicit, Size=1552)]
        private new unsafe struct v4l2_mpeg_vbi_fmt_ivtv : NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_fmt_ivtv
        {
            [FieldOffset(0)] private fixed byte magic_[4];
            public byte[] magic
            {
                get { fixed (byte* p = this.magic_) { return get(p, 4); } }
                set { fixed (byte* p = this.magic_) { set(p, value, 4); } }
            }

            [FieldOffset(4)] private v4l2_mpeg_vbi_itv0 itv0_;
            public NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_itv0 itv0
            {
                get => this.itv0_;
                set => this.itv0_ = (v4l2_mpeg_vbi_itv0)value;
            }

            [FieldOffset(4)] private v4l2_mpeg_vbi_ITV0 ITV0_;
            public NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_ITV0 ITV0
            {
                get => this.ITV0_;
                set => this.ITV0_ = (v4l2_mpeg_vbi_ITV0)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_fmt_ivtv Create_v4l2_mpeg_vbi_fmt_ivtv() => new v4l2_mpeg_vbi_fmt_ivtv();

        [StructLayout(LayoutKind.Explicit, Size=1513)]
        private new unsafe struct v4l2_mpeg_vbi_itv0 : NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_itv0
        {
            [FieldOffset(0)] private fixed uint linemask_[2];
            public uint[] linemask
            {
                get { fixed (uint* p = this.linemask_) { return get(p, 2); } }
                set { fixed (uint* p = this.linemask_) { set(p, value, 2); } }
            }

            [FieldOffset(8)] private fixed byte line_[43 * 35];   // sizeof(v4l2_mpeg_vbi_itv0_line): 43
            public NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_itv0_line[] line
            {
                get { fixed (byte* p = this.line_) { return get<v4l2_mpeg_vbi_itv0_line, NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_itv0_line>(p, 43, 35); } }
                set { fixed (byte* p = this.line_) { set<v4l2_mpeg_vbi_itv0_line, NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_itv0_line>(p, value, 43, 35); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_itv0 Create_v4l2_mpeg_vbi_itv0() => new v4l2_mpeg_vbi_itv0();

        [StructLayout(LayoutKind.Explicit, Size=1548)]
        private new unsafe struct v4l2_mpeg_vbi_ITV0 : NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_ITV0
        {
            [FieldOffset(0)] private fixed byte line_[43 * 36];   // sizeof(v4l2_mpeg_vbi_itv0_line): 43
            public NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_itv0_line[] line
            {
                get { fixed (byte* p = this.line_) { return get<v4l2_mpeg_vbi_itv0_line, NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_itv0_line>(p, 43, 36); } }
                set { fixed (byte* p = this.line_) { set<v4l2_mpeg_vbi_itv0_line, NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_itv0_line>(p, value, 43, 36); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_ITV0 Create_v4l2_mpeg_vbi_ITV0() => new v4l2_mpeg_vbi_ITV0();

        [StructLayout(LayoutKind.Explicit, Size=43)]
        private new unsafe struct v4l2_mpeg_vbi_itv0_line : NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_itv0_line
        {
            [FieldOffset(0)] private byte id_;
            public byte id
            {
                get => this.id_;
                set => this.id_ = (byte)value;
            }

            [FieldOffset(1)] private fixed byte data_[42];
            public byte[] data
            {
                get { fixed (byte* p = this.data_) { return get(p, 42); } }
                set { fixed (byte* p = this.data_) { set(p, value, 42); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_mpeg_vbi_itv0_line Create_v4l2_mpeg_vbi_itv0_line() => new v4l2_mpeg_vbi_itv0_line();

        [StructLayout(LayoutKind.Explicit, Size=72)]
        private new unsafe struct v4l2_output : NativeMethods_V4L2_Interop.v4l2_output
        {
            [FieldOffset(0)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(4)] private fixed byte name_[32];
            public byte[] name
            {
                get { fixed (byte* p = this.name_) { return get(p, 32); } }
                set { fixed (byte* p = this.name_) { set(p, value, 32); } }
            }

            [FieldOffset(36)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(40)] private uint audioset_;
            public uint audioset
            {
                get => this.audioset_;
                set => this.audioset_ = (uint)value;
            }

            [FieldOffset(44)] private uint modulator_;
            public uint modulator
            {
                get => this.modulator_;
                set => this.modulator_ = (uint)value;
            }

            [FieldOffset(48)] private ulong std_;
            public ulong std
            {
                get => this.std_;
                set => this.std_ = (ulong)value;
            }

            [FieldOffset(56)] private uint capabilities_;
            public uint capabilities
            {
                get => this.capabilities_;
                set => this.capabilities_ = (uint)value;
            }

            [FieldOffset(60)] private fixed uint reserved_[3];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 3); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 3); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_output Create_v4l2_output() => new v4l2_output();

        [StructLayout(LayoutKind.Explicit, Size=40)]
        private new unsafe struct v4l2_outputparm : NativeMethods_V4L2_Interop.v4l2_outputparm
        {
            [FieldOffset(0)] private uint capability_;
            public uint capability
            {
                get => this.capability_;
                set => this.capability_ = (uint)value;
            }

            [FieldOffset(4)] private uint outputmode_;
            public uint outputmode
            {
                get => this.outputmode_;
                set => this.outputmode_ = (uint)value;
            }

            [FieldOffset(8)] private v4l2_fract timeperframe_;
            public NativeMethods_V4L2_Interop.v4l2_fract timeperframe
            {
                get => this.timeperframe_;
                set => this.timeperframe_ = (v4l2_fract)value;
            }

            [FieldOffset(16)] private uint extendedmode_;
            public uint extendedmode
            {
                get => this.extendedmode_;
                set => this.extendedmode_ = (uint)value;
            }

            [FieldOffset(20)] private uint writebuffers_;
            public uint writebuffers
            {
                get => this.writebuffers_;
                set => this.writebuffers_ = (uint)value;
            }

            [FieldOffset(24)] private fixed uint reserved_[4];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 4); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 4); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_outputparm Create_v4l2_outputparm() => new v4l2_outputparm();

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

        [StructLayout(LayoutKind.Explicit, Size=232)]
        private new unsafe struct v4l2_query_ext_ctrl : NativeMethods_V4L2_Interop.v4l2_query_ext_ctrl
        {
            [FieldOffset(0)] private uint id_;
            public uint id
            {
                get => this.id_;
                set => this.id_ = (uint)value;
            }

            [FieldOffset(4)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(8)] private fixed byte name_[32];   // char
            public byte[] name
            {
                get { fixed (byte* p = this.name_) { return get(p, 32); } }
                set { fixed (byte* p = this.name_) { set(p, value, 32); } }
            }

            [FieldOffset(40)] private long minimum_;
            public long minimum
            {
                get => this.minimum_;
                set => this.minimum_ = (long)value;
            }

            [FieldOffset(48)] private long maximum_;
            public long maximum
            {
                get => this.maximum_;
                set => this.maximum_ = (long)value;
            }

            [FieldOffset(56)] private ulong step_;
            public ulong step
            {
                get => this.step_;
                set => this.step_ = (ulong)value;
            }

            [FieldOffset(64)] private long default_value_;
            public long default_value
            {
                get => this.default_value_;
                set => this.default_value_ = (long)value;
            }

            [FieldOffset(72)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(76)] private uint elem_size_;
            public uint elem_size
            {
                get => this.elem_size_;
                set => this.elem_size_ = (uint)value;
            }

            [FieldOffset(80)] private uint elems_;
            public uint elems
            {
                get => this.elems_;
                set => this.elems_ = (uint)value;
            }

            [FieldOffset(84)] private uint nr_of_dims_;
            public uint nr_of_dims
            {
                get => this.nr_of_dims_;
                set => this.nr_of_dims_ = (uint)value;
            }

            [FieldOffset(88)] private fixed uint dims_[4];
            public uint[] dims
            {
                get { fixed (uint* p = this.dims_) { return get(p, 4); } }
                set { fixed (uint* p = this.dims_) { set(p, value, 4); } }
            }

            [FieldOffset(104)] private fixed uint reserved_[32];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 32); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 32); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_query_ext_ctrl Create_v4l2_query_ext_ctrl() => new v4l2_query_ext_ctrl();

        [StructLayout(LayoutKind.Explicit, Size=68)]
        private new unsafe struct v4l2_queryctrl : NativeMethods_V4L2_Interop.v4l2_queryctrl
        {
            [FieldOffset(0)] private uint id_;
            public uint id
            {
                get => this.id_;
                set => this.id_ = (uint)value;
            }

            [FieldOffset(4)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(8)] private fixed byte name_[32];
            public byte[] name
            {
                get { fixed (byte* p = this.name_) { return get(p, 32); } }
                set { fixed (byte* p = this.name_) { set(p, value, 32); } }
            }

            [FieldOffset(40)] private int minimum_;
            public int minimum
            {
                get => this.minimum_;
                set => this.minimum_ = (int)value;
            }

            [FieldOffset(44)] private int maximum_;
            public int maximum
            {
                get => this.maximum_;
                set => this.maximum_ = (int)value;
            }

            [FieldOffset(48)] private int step_;
            public int step
            {
                get => this.step_;
                set => this.step_ = (int)value;
            }

            [FieldOffset(52)] private int default_value_;
            public int default_value
            {
                get => this.default_value_;
                set => this.default_value_ = (int)value;
            }

            [FieldOffset(56)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(60)] private fixed uint reserved_[2];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 2); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 2); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_queryctrl Create_v4l2_queryctrl() => new v4l2_queryctrl();

        [StructLayout(LayoutKind.Explicit, Size=44)]
        private new unsafe struct v4l2_querymenu : NativeMethods_V4L2_Interop.v4l2_querymenu
        {
            [FieldOffset(0)] private uint id_;
            public uint id
            {
                get => this.id_;
                set => this.id_ = (uint)value;
            }

            [FieldOffset(4)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(8)] private fixed byte name_[32];
            public byte[] name
            {
                get { fixed (byte* p = this.name_) { return get(p, 32); } }
                set { fixed (byte* p = this.name_) { set(p, value, 32); } }
            }

            [FieldOffset(8)] private long value_;
            public long value
            {
                get => this.value_;
                set => this.value_ = (long)value;
            }

            [FieldOffset(40)] private uint reserved_;
            public uint reserved
            {
                get => this.reserved_;
                set => this.reserved_ = (uint)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_querymenu Create_v4l2_querymenu() => new v4l2_querymenu();

        [StructLayout(LayoutKind.Explicit, Size=3)]
        private new unsafe struct v4l2_rds_data : NativeMethods_V4L2_Interop.v4l2_rds_data
        {
            [FieldOffset(0)] private byte lsb_;
            public byte lsb
            {
                get => this.lsb_;
                set => this.lsb_ = (byte)value;
            }

            [FieldOffset(1)] private byte msb_;
            public byte msb
            {
                get => this.msb_;
                set => this.msb_ = (byte)value;
            }

            [FieldOffset(2)] private byte block_;
            public byte block
            {
                get => this.block_;
                set => this.block_ = (byte)value;
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_rds_data Create_v4l2_rds_data() => new v4l2_rds_data();

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

        [StructLayout(LayoutKind.Explicit, Size=64)]
        private new unsafe struct v4l2_selection : NativeMethods_V4L2_Interop.v4l2_selection
        {
            [FieldOffset(0)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(4)] private uint target_;
            public uint target
            {
                get => this.target_;
                set => this.target_ = (uint)value;
            }

            [FieldOffset(8)] private uint flags_;
            public uint flags
            {
                get => this.flags_;
                set => this.flags_ = (uint)value;
            }

            [FieldOffset(12)] private v4l2_rect r_;
            public NativeMethods_V4L2_Interop.v4l2_rect r
            {
                get => this.r_;
                set => this.r_ = (v4l2_rect)value;
            }

            [FieldOffset(28)] private fixed uint reserved_[9];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 9); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 9); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_selection Create_v4l2_selection() => new v4l2_selection();

        [StructLayout(LayoutKind.Explicit, Size=116)]
        private new unsafe struct v4l2_sliced_vbi_cap : NativeMethods_V4L2_Interop.v4l2_sliced_vbi_cap
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

            [FieldOffset(100)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(104)] private fixed uint reserved_[3];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 3); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 3); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_sliced_vbi_cap Create_v4l2_sliced_vbi_cap() => new v4l2_sliced_vbi_cap();

        [StructLayout(LayoutKind.Explicit, Size=64)]
        private new unsafe struct v4l2_sliced_vbi_data : NativeMethods_V4L2_Interop.v4l2_sliced_vbi_data
        {
            [FieldOffset(0)] private uint id_;
            public uint id
            {
                get => this.id_;
                set => this.id_ = (uint)value;
            }

            [FieldOffset(4)] private uint field_;
            public uint field
            {
                get => this.field_;
                set => this.field_ = (uint)value;
            }

            [FieldOffset(8)] private uint line_;
            public uint line
            {
                get => this.line_;
                set => this.line_ = (uint)value;
            }

            [FieldOffset(12)] private uint reserved_;
            public uint reserved
            {
                get => this.reserved_;
                set => this.reserved_ = (uint)value;
            }

            [FieldOffset(16)] private fixed byte data_[48];
            public byte[] data
            {
                get { fixed (byte* p = this.data_) { return get(p, 48); } }
                set { fixed (byte* p = this.data_) { set(p, value, 48); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_sliced_vbi_data Create_v4l2_sliced_vbi_data() => new v4l2_sliced_vbi_data();

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

        [StructLayout(LayoutKind.Explicit, Size=72)]
        private new unsafe struct v4l2_standard : NativeMethods_V4L2_Interop.v4l2_standard
        {
            [FieldOffset(0)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(8)] private ulong id_;
            public ulong id
            {
                get => this.id_;
                set => this.id_ = (ulong)value;
            }

            [FieldOffset(16)] private fixed byte name_[24];
            public byte[] name
            {
                get { fixed (byte* p = this.name_) { return get(p, 24); } }
                set { fixed (byte* p = this.name_) { set(p, value, 24); } }
            }

            [FieldOffset(40)] private v4l2_fract frameperiod_;
            public NativeMethods_V4L2_Interop.v4l2_fract frameperiod
            {
                get => this.frameperiod_;
                set => this.frameperiod_ = (v4l2_fract)value;
            }

            [FieldOffset(48)] private uint framelines_;
            public uint framelines
            {
                get => this.framelines_;
                set => this.framelines_ = (uint)value;
            }

            [FieldOffset(52)] private fixed uint reserved_[4];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 4); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 4); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_standard Create_v4l2_standard() => new v4l2_standard();

        [StructLayout(LayoutKind.Explicit, Size=204)]
        private new unsafe struct v4l2_streamparm : NativeMethods_V4L2_Interop.v4l2_streamparm
        {
            [FieldOffset(0)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(4)] private v4l2_captureparm parm_capture_;
            public NativeMethods_V4L2_Interop.v4l2_captureparm parm_capture
            {
                get => this.parm_capture_;
                set => this.parm_capture_ = (v4l2_captureparm)value;
            }

            [FieldOffset(4)] private v4l2_outputparm parm_output_;
            public NativeMethods_V4L2_Interop.v4l2_outputparm parm_output
            {
                get => this.parm_output_;
                set => this.parm_output_ = (v4l2_outputparm)value;
            }

            [FieldOffset(4)] private fixed byte parm_raw_data_[200];
            public byte[] parm_raw_data
            {
                get { fixed (byte* p = this.parm_raw_data_) { return get(p, 200); } }
                set { fixed (byte* p = this.parm_raw_data_) { set(p, value, 200); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_streamparm Create_v4l2_streamparm() => new v4l2_streamparm();

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

        [StructLayout(LayoutKind.Explicit, Size=84)]
        private new unsafe struct v4l2_tuner : NativeMethods_V4L2_Interop.v4l2_tuner
        {
            [FieldOffset(0)] private uint index_;
            public uint index
            {
                get => this.index_;
                set => this.index_ = (uint)value;
            }

            [FieldOffset(4)] private fixed byte name_[32];
            public byte[] name
            {
                get { fixed (byte* p = this.name_) { return get(p, 32); } }
                set { fixed (byte* p = this.name_) { set(p, value, 32); } }
            }

            [FieldOffset(36)] private uint type_;
            public uint type
            {
                get => this.type_;
                set => this.type_ = (uint)value;
            }

            [FieldOffset(40)] private uint capability_;
            public uint capability
            {
                get => this.capability_;
                set => this.capability_ = (uint)value;
            }

            [FieldOffset(44)] private uint rangelow_;
            public uint rangelow
            {
                get => this.rangelow_;
                set => this.rangelow_ = (uint)value;
            }

            [FieldOffset(48)] private uint rangehigh_;
            public uint rangehigh
            {
                get => this.rangehigh_;
                set => this.rangehigh_ = (uint)value;
            }

            [FieldOffset(52)] private uint rxsubchans_;
            public uint rxsubchans
            {
                get => this.rxsubchans_;
                set => this.rxsubchans_ = (uint)value;
            }

            [FieldOffset(56)] private uint audmode_;
            public uint audmode
            {
                get => this.audmode_;
                set => this.audmode_ = (uint)value;
            }

            [FieldOffset(60)] private int signal_;
            public int signal
            {
                get => this.signal_;
                set => this.signal_ = (int)value;
            }

            [FieldOffset(64)] private int afc_;
            public int afc
            {
                get => this.afc_;
                set => this.afc_ = (int)value;
            }

            [FieldOffset(68)] private fixed uint reserved_[4];
            public uint[] reserved
            {
                get { fixed (uint* p = this.reserved_) { return get(p, 4); } }
                set { fixed (uint* p = this.reserved_) { set(p, value, 4); } }
            }

        }
        public override NativeMethods_V4L2_Interop.v4l2_tuner Create_v4l2_tuner() => new v4l2_tuner();

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

