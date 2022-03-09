////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

namespace System.Threading.Tasks
{
#if !NET35
    // Interoperability implementation for TaskEx.
    internal static class TaskEx
    {
        public static Task Delay(int milliseconds) =>
            // Simply bypassed.
            Task.Delay(milliseconds);
    }
#endif
}
