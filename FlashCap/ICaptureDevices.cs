////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace FlashCap
{
    public interface ICaptureDevices
    {
        IEnumerable<CaptureDeviceDescription> Descriptions { get; }

        ICaptureDevice Open(CaptureDeviceDescription description, bool holdRawData = false);
    }
}
