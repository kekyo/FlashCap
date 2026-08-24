////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using FlashCap.Devices;
using NUnit.Framework;
using System;
using System.Linq;

namespace FlashCap.Core.Tests;

#pragma warning disable CA1416 // This test verifies the non-Windows guard itself.

[TestFixture]
public sealed class NativeMethodsPlatformTests
{
    [TestCase(5, 1, 6, 1, false)]
    [TestCase(6, 0, 6, 1, false)]
    [TestCase(6, 1, 6, 1, true)]
    [TestCase(6, 2, 6, 1, true)]
    [TestCase(10, 0, 6, 1, true)]
    [TestCase(7, 0, 6, 1, true)]
    [TestCase(6, int.MaxValue, 7, 0, false)]
    [TestCase(7, 0, 6, int.MaxValue, true)]
    public void IsVersionAtLeastComparesMajorThenMinor(
        int versionMajor,
        int versionMinor,
        int requiredMajor,
        int requiredMinor,
        bool expected)
    {
        var actual = NativeMethods.IsVersionAtLeast(
            new Version(versionMajor, versionMinor),
            requiredMajor,
            requiredMinor);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void MediaFoundationEnumerationRejectsNonWindowsBeforeNativeLoading()
    {
        if (NativeMethods.IsWindows())
        {
            return;
        }

        _ = Assert.Throws<PlatformNotSupportedException>(() =>
            new MediaFoundationDevices().InternalEnumerateDescriptors().ToArray());
    }
}
