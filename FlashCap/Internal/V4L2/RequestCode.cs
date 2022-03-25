////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;

namespace FlashCap.Internal.V4L2
{
    internal abstract class RequestCode
    {
        // https://jcs.org/2022/02/16/ioctl

        protected RequestCode()
        {
        }

        public abstract UIntPtr VIDIOC_QUERYCAP { get; }
        public abstract UIntPtr VIDIOC_ENUMINPUT { get; }
        public abstract UIntPtr VIDIOC_ENUM_FMT { get; }
        public abstract UIntPtr VIDIOC_ENUM_FRAMESIZES { get; }
        public abstract UIntPtr VIDIOC_ENUM_FRAMEINTERVALS { get; }
        public abstract UIntPtr VIDIOC_S_FMT { get; }
        public abstract UIntPtr VIDIOC_G_FMT { get; }
        public abstract UIntPtr VIDIOC_REQBUFS { get; }
        public abstract UIntPtr VIDIOC_QUERYBUF { get; }
        public abstract UIntPtr VIDIOC_QBUF { get; }
        public abstract UIntPtr VIDIOC_DQBUF { get; }
        public abstract UIntPtr VIDIOC_STREAMON { get; }
        public abstract UIntPtr VIDIOC_STREAMOFF { get; }
    }
}
