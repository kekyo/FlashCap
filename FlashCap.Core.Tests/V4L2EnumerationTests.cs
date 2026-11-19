////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Devices;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FlashCap.Core.Tests;

[TestFixture]
public sealed class V4L2EnumerationTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task UnavailableDeviceDirectoryReturnsNoDevices(bool inaccessible)
    {
        if (!OperatingSystem.IsLinux())
        {
            Assert.Ignore("Requires Linux device enumeration.");
            return;
        }
        var path = Path.Combine(Path.GetTempPath(), "flashcap-v4l2-" + Guid.NewGuid().ToString("N"));
        try
        {
            if (inaccessible)
            {
                Directory.CreateDirectory(path);
                File.SetUnixFileMode(path, UnixFileMode.None);
                Assert.Throws<UnauthorizedAccessException>(() => Directory.GetFiles(path));
            }
            var devices = new V4L2Devices(new DefaultBufferPool(), path);
            Assert.That(devices.InternalEnumerateDescriptors().ToArray(), Is.Empty);
            await foreach (var descriptor in devices.InternalEnumerateDescriptorsAsync(default))
                Assert.Fail($"Unexpected device: {descriptor.Name}");
        }
        finally
        {
            if (Directory.Exists(path))
            {
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
                Directory.Delete(path);
            }
        }
    }
}
