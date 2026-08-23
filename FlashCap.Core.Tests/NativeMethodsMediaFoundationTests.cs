////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MediaFoundation = FlashCap.Internal.NativeMethods_MediaFoundation;

#pragma warning disable CA1416 // The fake vtables do not invoke Windows APIs.

namespace FlashCap.Core.Tests;

[TestFixture]
[NonParallelizable]
public sealed unsafe class NativeMethodsMediaFoundationTests
{
    private const int VtableLength = 64;
    private const int ExpectedHResult = unchecked((int)0x81234567);
    private const int UnexpectedHResult = unchecked((int)0x80004005);
    private const uint ExpectedReleaseResult = 0xfedcba98;
    private const uint UnexpectedReleaseResult = 0;

    private static readonly Guid InputGuid =
        new("11223344-5566-7788-99aa-bbccddeeff00");
    private static readonly Guid SecondInputGuid =
        new("ffeeddcc-bbaa-9988-7766-554433221100");
    private static readonly Guid OutputGuid =
        new("13572468-2468-1357-8642-abcdefabcdef");

    private static readonly nint InputPointer = (nint)0x12345678;
    private static readonly nint OutputPointer = (nint)0x76543210;
    private static readonly nint OutputStringPointer = (nint)0x24681357;

    private static nint expectedSelf;
    private static nint actualSelf;
    private static int expectedCallCount;
    private static int unexpectedCallCount;
    private static Guid actualGuid;
    private static Guid actualSecondGuid;
    private static nint actualPointer;
    private static nint actualReservedPointer;
    private static nint actualStreamIndexPointer;
    private static nint actualStreamFlagsPointer;
    private static nint actualTimestampPointer;
    private static nint actualSamplePointer;
    private static uint actualUInt32;
    private static uint actualSecondUInt32;
    private static int actualBoolean;

    private struct FakeComObject
    {
        internal void** Vtable;
    }

    [Test]
    public void IUnknownReleaseUsesZeroBasedSlot2()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, uint>)&UnexpectedRelease,
            2,
            (void*)(delegate* unmanaged[Stdcall]<void*, uint>)&Release,
            &instance);

        var result = ((MediaFoundation.IUnknown*)&instance)->Release();

        Assert.That(result, Is.EqualTo(ExpectedReleaseResult));
        AssertExpectedCall();
    }

    [Test]
    public void IMFAttributesGetUINT32UsesZeroBasedSlot7()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, uint*, int>)&UnexpectedGuidUInt32Out,
            7,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, uint*, int>)&GetUInt32,
            &instance);
        var key = InputGuid;

        var result = ((MediaFoundation.IMFAttributes*)&instance)->GetUINT32(in key, out var value);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualGuid, Is.EqualTo(InputGuid));
        Assert.That(value, Is.EqualTo(0x89abcdefu));
        AssertExpectedCall();
    }

    [Test]
    public void IMFAttributesGetUINT64UsesZeroBasedSlot8()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, ulong*, int>)&UnexpectedGuidUInt64Out,
            8,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, ulong*, int>)&GetUInt64,
            &instance);
        var key = InputGuid;

        var result = ((MediaFoundation.IMFAttributes*)&instance)->GetUINT64(in key, out var value);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualGuid, Is.EqualTo(InputGuid));
        Assert.That(value, Is.EqualTo(0x0123456789abcdeful));
        AssertExpectedCall();
    }

    [Test]
    public void IMFAttributesGetGUIDUsesZeroBasedSlot10()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, Guid*, int>)&UnexpectedGuidGuidOut,
            10,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, Guid*, int>)&GetGuid,
            &instance);
        var key = InputGuid;

        var result = ((MediaFoundation.IMFAttributes*)&instance)->GetGUID(in key, out var value);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualGuid, Is.EqualTo(InputGuid));
        Assert.That(value, Is.EqualTo(OutputGuid));
        AssertExpectedCall();
    }

    [Test]
    public void IMFAttributesGetAllocatedStringUsesZeroBasedSlot13()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, nint*, uint*, int>)&UnexpectedAllocatedString,
            13,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, nint*, uint*, int>)&GetAllocatedString,
            &instance);
        var key = InputGuid;

        var result = ((MediaFoundation.IMFAttributes*)&instance)->GetAllocatedString(
            in key,
            out var value,
            out var length);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualGuid, Is.EqualTo(InputGuid));
        Assert.That(value, Is.EqualTo(OutputStringPointer));
        Assert.That(length, Is.EqualTo(37u));
        AssertExpectedCall();
    }

    [Test]
    public void IMFActivateGetAllocatedStringUsesZeroBasedSlot13()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, nint*, uint*, int>)&UnexpectedAllocatedString,
            13,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, nint*, uint*, int>)&GetAllocatedString,
            &instance);
        var key = InputGuid;

        var result = ((MediaFoundation.IMFActivate*)&instance)->GetAllocatedString(
            in key,
            out var value,
            out var length);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualGuid, Is.EqualTo(InputGuid));
        Assert.That(value, Is.EqualTo(OutputStringPointer));
        Assert.That(length, Is.EqualTo(37u));
        AssertExpectedCall();
    }

    [Test]
    public void IMFMediaTypeGetUINT32UsesZeroBasedSlot7()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, uint*, int>)&UnexpectedGuidUInt32Out,
            7,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, uint*, int>)&GetUInt32,
            &instance);
        var key = InputGuid;

        var result = ((MediaFoundation.IMFMediaType*)&instance)->GetUINT32(in key, out var value);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualGuid, Is.EqualTo(InputGuid));
        Assert.That(value, Is.EqualTo(0x89abcdefu));
        AssertExpectedCall();
    }

    [Test]
    public void IMFMediaTypeGetUINT64UsesZeroBasedSlot8()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, ulong*, int>)&UnexpectedGuidUInt64Out,
            8,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, ulong*, int>)&GetUInt64,
            &instance);
        var key = InputGuid;

        var result = ((MediaFoundation.IMFMediaType*)&instance)->GetUINT64(in key, out var value);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualGuid, Is.EqualTo(InputGuid));
        Assert.That(value, Is.EqualTo(0x0123456789abcdeful));
        AssertExpectedCall();
    }

    [Test]
    public void IMFMediaTypeGetGUIDUsesZeroBasedSlot10()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, Guid*, int>)&UnexpectedGuidGuidOut,
            10,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, Guid*, int>)&GetGuid,
            &instance);
        var key = InputGuid;

        var result = ((MediaFoundation.IMFMediaType*)&instance)->GetGUID(in key, out var value);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualGuid, Is.EqualTo(InputGuid));
        Assert.That(value, Is.EqualTo(OutputGuid));
        AssertExpectedCall();
    }

    [Test]
    public void IMFAttributesSetGUIDUsesZeroBasedSlot24()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, Guid*, int>)&UnexpectedGuidGuidIn,
            24,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, Guid*, int>)&SetGuid,
            &instance);
        var key = InputGuid;
        var value = SecondInputGuid;

        var result = ((MediaFoundation.IMFAttributes*)&instance)->SetGUID(in key, in value);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualGuid, Is.EqualTo(InputGuid));
        Assert.That(actualSecondGuid, Is.EqualTo(SecondInputGuid));
        AssertExpectedCall();
    }

    [Test]
    public void IMFAttributesSetUnknownUsesZeroBasedSlot27()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, void*, int>)&UnexpectedGuidPointerIn,
            27,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, void*, int>)&SetUnknown,
            &instance);
        var key = InputGuid;
        var value = (MediaFoundation.IUnknown*)InputPointer;

        var result = ((MediaFoundation.IMFAttributes*)&instance)->SetUnknown(in key, value);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualGuid, Is.EqualTo(InputGuid));
        Assert.That(actualPointer, Is.EqualTo(InputPointer));
        AssertExpectedCall();
    }

    [Test]
    public void IMFActivateActivateObjectUsesZeroBasedSlot33()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, void**, int>)&UnexpectedGuidPointerOut,
            33,
            (void*)(delegate* unmanaged[Stdcall]<void*, Guid*, void**, int>)&ActivateObject,
            &instance);
        var iid = InputGuid;

        var result = ((MediaFoundation.IMFActivate*)&instance)->ActivateObject(
            in iid,
            out var value);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualGuid, Is.EqualTo(InputGuid));
        Assert.That((nint)value, Is.EqualTo(OutputPointer));
        AssertExpectedCall();
    }

    [Test]
    public void IMFActivateShutdownObjectUsesZeroBasedSlot34()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        PrepareNoArguments(vtable, 34, &instance);

        var result = ((MediaFoundation.IMFActivate*)&instance)->ShutdownObject();

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        AssertExpectedCall();
    }

    [Test]
    public void IMFMediaBufferLockUsesZeroBasedSlot3()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, byte**, uint*, uint*, int>)&UnexpectedLock,
            3,
            (void*)(delegate* unmanaged[Stdcall]<void*, byte**, uint*, uint*, int>)&Lock,
            &instance);

        var result = ((MediaFoundation.IMFMediaBuffer*)&instance)->Lock(
            out var data,
            out var maximumLength,
            out var currentLength);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That((nint)data, Is.EqualTo(OutputPointer));
        Assert.That(maximumLength, Is.EqualTo(4096u));
        Assert.That(currentLength, Is.EqualTo(3072u));
        AssertExpectedCall();
    }

    [Test]
    public void IMFMediaBufferUnlockUsesZeroBasedSlot4()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        PrepareNoArguments(vtable, 4, &instance);

        var result = ((MediaFoundation.IMFMediaBuffer*)&instance)->Unlock();

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        AssertExpectedCall();
    }

    [Test]
    public void IMFMediaSourceShutdownUsesZeroBasedSlot12()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        PrepareNoArguments(vtable, 12, &instance);

        var result = ((MediaFoundation.IMFMediaSource*)&instance)->Shutdown();

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        AssertExpectedCall();
    }

    [Test]
    public void IMFSampleConvertToContiguousBufferUsesZeroBasedSlot41()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, void**, int>)&UnexpectedPointerOut,
            41,
            (void*)(delegate* unmanaged[Stdcall]<void*, void**, int>)&ConvertToContiguousBuffer,
            &instance);

        var result = ((MediaFoundation.IMFSample*)&instance)->ConvertToContiguousBuffer(
            out var buffer);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That((nint)buffer, Is.EqualTo(OutputPointer));
        AssertExpectedCall();
    }

    [Test]
    public void IMFSourceReaderSetStreamSelectionUsesZeroBasedSlot4AndFourByteBool()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, uint, int, int>)&UnexpectedUInt32Boolean,
            4,
            (void*)(delegate* unmanaged[Stdcall]<void*, uint, int, int>)&SetStreamSelection,
            &instance);
        var reader = (MediaFoundation.IMFSourceReader*)&instance;

        var result = reader->SetStreamSelection(0x10203040, true);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualUInt32, Is.EqualTo(0x10203040u));
        Assert.That(actualBoolean, Is.EqualTo(1));
        AssertExpectedCall();

        result = reader->SetStreamSelection(0x50607080, false);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualUInt32, Is.EqualTo(0x50607080u));
        Assert.That(actualBoolean, Is.EqualTo(0));
        AssertExpectedCall(2);
    }

    [Test]
    public void IMFSourceReaderGetNativeMediaTypeUsesZeroBasedSlot5()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, uint, uint, void**, int>)&UnexpectedTwoUInt32PointerOut,
            5,
            (void*)(delegate* unmanaged[Stdcall]<void*, uint, uint, void**, int>)&GetNativeMediaType,
            &instance);

        var result = ((MediaFoundation.IMFSourceReader*)&instance)->GetNativeMediaType(
            0x10203040,
            0x50607080,
            out var mediaType);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualUInt32, Is.EqualTo(0x10203040u));
        Assert.That(actualSecondUInt32, Is.EqualTo(0x50607080u));
        Assert.That((nint)mediaType, Is.EqualTo(OutputPointer));
        AssertExpectedCall();
    }

    [Test]
    public void IMFSourceReaderSetCurrentMediaTypeUsesZeroBasedSlot7AndNullReservedPointer()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, uint, void*, void*, int>)&UnexpectedSetCurrentMediaType,
            7,
            (void*)(delegate* unmanaged[Stdcall]<void*, uint, void*, void*, int>)&SetCurrentMediaType,
            &instance);
        var mediaType = (MediaFoundation.IMFMediaType*)InputPointer;

        var result = ((MediaFoundation.IMFSourceReader*)&instance)->SetCurrentMediaType(
            0x10203040,
            mediaType);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualUInt32, Is.EqualTo(0x10203040u));
        Assert.That(actualReservedPointer, Is.EqualTo(nint.Zero));
        Assert.That(actualPointer, Is.EqualTo(InputPointer));
        AssertExpectedCall();
    }

    [Test]
    public void IMFSourceReaderReadSampleUsesZeroBasedSlot9()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, uint, uint, uint*, uint*, long*, void**, int>)&UnexpectedReadSample,
            9,
            (void*)(delegate* unmanaged[Stdcall]<void*, uint, uint, uint*, uint*, long*, void**, int>)&ReadSample,
            &instance);

        var reader = (MediaFoundation.IMFSourceReader*)&instance;
        uint actualStreamIndex = 0;
        uint streamFlags = 0;
        long timestamp = 0;
        MediaFoundation.IMFSample* sample = null;

        var result = reader->ReadSample(
            0x10203040,
            0x50607080,
            &actualStreamIndex,
            &streamFlags,
            &timestamp,
            &sample);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualUInt32, Is.EqualTo(0x10203040u));
        Assert.That(actualSecondUInt32, Is.EqualTo(0x50607080u));
        Assert.That(actualStreamIndexPointer, Is.EqualTo((nint)(&actualStreamIndex)));
        Assert.That(actualStreamFlagsPointer, Is.EqualTo((nint)(&streamFlags)));
        Assert.That(actualTimestampPointer, Is.EqualTo((nint)(&timestamp)));
        Assert.That(actualSamplePointer, Is.EqualTo((nint)(&sample)));
        Assert.That(actualStreamIndex, Is.EqualTo(0x90a0b0c0u));
        Assert.That(streamFlags, Is.EqualTo(0xd0e0f000u));
        Assert.That(timestamp, Is.EqualTo(0x0123456789abcdefL));
        Assert.That((nint)sample, Is.EqualTo(OutputPointer));
        AssertExpectedCall();

        result = reader->ReadSample(
            0x11223344,
            0x55667788,
            null,
            null,
            null,
            null);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualUInt32, Is.EqualTo(0x11223344u));
        Assert.That(actualSecondUInt32, Is.EqualTo(0x55667788u));
        Assert.That(actualStreamIndexPointer, Is.EqualTo(nint.Zero));
        Assert.That(actualStreamFlagsPointer, Is.EqualTo(nint.Zero));
        Assert.That(actualTimestampPointer, Is.EqualTo(nint.Zero));
        Assert.That(actualSamplePointer, Is.EqualTo(nint.Zero));
        AssertExpectedCall(2);
    }

    [Test]
    public void IMFSourceReaderFlushUsesZeroBasedSlot10()
    {
        void** vtable = stackalloc void*[VtableLength];
        var instance = new FakeComObject { Vtable = vtable };
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, uint, int>)&UnexpectedUInt32,
            10,
            (void*)(delegate* unmanaged[Stdcall]<void*, uint, int>)&Flush,
            &instance);

        var result = ((MediaFoundation.IMFSourceReader*)&instance)->Flush(0x10203040);

        Assert.That(result, Is.EqualTo(ExpectedHResult));
        Assert.That(actualUInt32, Is.EqualTo(0x10203040u));
        AssertExpectedCall();
    }

    private static void Prepare(
        void** vtable,
        void* unexpected,
        int expectedSlot,
        void* expected,
        FakeComObject* instance)
    {
        for (var slot = 0; slot < VtableLength; slot++)
        {
            vtable[slot] = unexpected;
        }
        vtable[expectedSlot] = expected;
        Reset(instance);
    }

    private static void PrepareNoArguments(
        void** vtable,
        int expectedSlot,
        FakeComObject* instance) =>
        Prepare(
            vtable,
            (void*)(delegate* unmanaged[Stdcall]<void*, int>)&UnexpectedNoArguments,
            expectedSlot,
            (void*)(delegate* unmanaged[Stdcall]<void*, int>)&NoArguments,
            instance);

    private static void Reset(FakeComObject* instance)
    {
        expectedSelf = (nint)instance;
        actualSelf = nint.Zero;
        expectedCallCount = 0;
        unexpectedCallCount = 0;
        actualGuid = Guid.Empty;
        actualSecondGuid = Guid.Empty;
        actualPointer = nint.Zero;
        actualReservedPointer = nint.Zero;
        actualStreamIndexPointer = nint.Zero;
        actualStreamFlagsPointer = nint.Zero;
        actualTimestampPointer = nint.Zero;
        actualSamplePointer = nint.Zero;
        actualUInt32 = 0;
        actualSecondUInt32 = 0;
        actualBoolean = 0;
    }

    private static void RecordExpected(void* self)
    {
        actualSelf = (nint)self;
        expectedCallCount++;
    }

    private static void RecordUnexpected(void* self)
    {
        actualSelf = (nint)self;
        unexpectedCallCount++;
    }

    private static void AssertExpectedCall(int expectedCount = 1)
    {
        Assert.That(actualSelf, Is.EqualTo(expectedSelf));
        Assert.That(expectedCallCount, Is.EqualTo(expectedCount));
        Assert.That(unexpectedCallCount, Is.EqualTo(0));
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static uint Release(void* self)
    {
        RecordExpected(self);
        return ExpectedReleaseResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static uint UnexpectedRelease(void* self)
    {
        RecordUnexpected(self);
        return UnexpectedReleaseResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int GetUInt32(void* self, Guid* key, uint* value)
    {
        RecordExpected(self);
        actualGuid = *key;
        *value = 0x89abcdef;
        return ExpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int UnexpectedGuidUInt32Out(void* self, Guid* key, uint* value)
    {
        RecordUnexpected(self);
        *value = 0;
        return UnexpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int GetUInt64(void* self, Guid* key, ulong* value)
    {
        RecordExpected(self);
        actualGuid = *key;
        *value = 0x0123456789abcdef;
        return ExpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int UnexpectedGuidUInt64Out(void* self, Guid* key, ulong* value)
    {
        RecordUnexpected(self);
        *value = 0;
        return UnexpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int GetGuid(void* self, Guid* key, Guid* value)
    {
        RecordExpected(self);
        actualGuid = *key;
        *value = OutputGuid;
        return ExpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int UnexpectedGuidGuidOut(void* self, Guid* key, Guid* value)
    {
        RecordUnexpected(self);
        *value = Guid.Empty;
        return UnexpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int GetAllocatedString(void* self, Guid* key, nint* value, uint* length)
    {
        RecordExpected(self);
        actualGuid = *key;
        *value = OutputStringPointer;
        *length = 37;
        return ExpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int UnexpectedAllocatedString(void* self, Guid* key, nint* value, uint* length)
    {
        RecordUnexpected(self);
        *value = nint.Zero;
        *length = 0;
        return UnexpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int SetGuid(void* self, Guid* key, Guid* value)
    {
        RecordExpected(self);
        actualGuid = *key;
        actualSecondGuid = *value;
        return ExpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int UnexpectedGuidGuidIn(void* self, Guid* key, Guid* value)
    {
        RecordUnexpected(self);
        return UnexpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int SetUnknown(void* self, Guid* key, void* value)
    {
        RecordExpected(self);
        actualGuid = *key;
        actualPointer = (nint)value;
        return ExpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int UnexpectedGuidPointerIn(void* self, Guid* key, void* value)
    {
        RecordUnexpected(self);
        return UnexpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int ActivateObject(void* self, Guid* iid, void** value)
    {
        RecordExpected(self);
        actualGuid = *iid;
        *value = (void*)OutputPointer;
        return ExpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int UnexpectedGuidPointerOut(void* self, Guid* iid, void** value)
    {
        RecordUnexpected(self);
        *value = null;
        return UnexpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int NoArguments(void* self)
    {
        RecordExpected(self);
        return ExpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int UnexpectedNoArguments(void* self)
    {
        RecordUnexpected(self);
        return UnexpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int Lock(void* self, byte** data, uint* maximumLength, uint* currentLength)
    {
        RecordExpected(self);
        *data = (byte*)OutputPointer;
        *maximumLength = 4096;
        *currentLength = 3072;
        return ExpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int UnexpectedLock(
        void* self,
        byte** data,
        uint* maximumLength,
        uint* currentLength)
    {
        RecordUnexpected(self);
        *data = null;
        *maximumLength = 0;
        *currentLength = 0;
        return UnexpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int ConvertToContiguousBuffer(void* self, void** buffer)
    {
        RecordExpected(self);
        *buffer = (void*)OutputPointer;
        return ExpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int UnexpectedPointerOut(void* self, void** value)
    {
        RecordUnexpected(self);
        *value = null;
        return UnexpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int SetStreamSelection(void* self, uint streamIndex, int selected)
    {
        RecordExpected(self);
        actualUInt32 = streamIndex;
        actualBoolean = selected;
        return ExpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int UnexpectedUInt32Boolean(void* self, uint streamIndex, int selected)
    {
        RecordUnexpected(self);
        return UnexpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int GetNativeMediaType(
        void* self,
        uint streamIndex,
        uint mediaTypeIndex,
        void** mediaType)
    {
        RecordExpected(self);
        actualUInt32 = streamIndex;
        actualSecondUInt32 = mediaTypeIndex;
        *mediaType = (void*)OutputPointer;
        return ExpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int UnexpectedTwoUInt32PointerOut(
        void* self,
        uint first,
        uint second,
        void** value)
    {
        RecordUnexpected(self);
        *value = null;
        return UnexpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int SetCurrentMediaType(
        void* self,
        uint streamIndex,
        void* reserved,
        void* mediaType)
    {
        RecordExpected(self);
        actualUInt32 = streamIndex;
        actualReservedPointer = (nint)reserved;
        actualPointer = (nint)mediaType;
        return ExpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int UnexpectedSetCurrentMediaType(
        void* self,
        uint streamIndex,
        void* reserved,
        void* mediaType)
    {
        RecordUnexpected(self);
        return UnexpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int ReadSample(
        void* self,
        uint streamIndex,
        uint controlFlags,
        uint* actualStreamIndex,
        uint* streamFlags,
        long* timestamp,
        void** sample)
    {
        RecordExpected(self);
        actualUInt32 = streamIndex;
        actualSecondUInt32 = controlFlags;
        actualStreamIndexPointer = (nint)actualStreamIndex;
        actualStreamFlagsPointer = (nint)streamFlags;
        actualTimestampPointer = (nint)timestamp;
        actualSamplePointer = (nint)sample;
        if (actualStreamIndex is not null)
        {
            *actualStreamIndex = 0x90a0b0c0;
        }
        if (streamFlags is not null)
        {
            *streamFlags = 0xd0e0f000;
        }
        if (timestamp is not null)
        {
            *timestamp = 0x0123456789abcdef;
        }
        if (sample is not null)
        {
            *sample = (void*)OutputPointer;
        }
        return ExpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int UnexpectedReadSample(
        void* self,
        uint streamIndex,
        uint controlFlags,
        uint* actualStreamIndex,
        uint* streamFlags,
        long* timestamp,
        void** sample)
    {
        RecordUnexpected(self);
        if (actualStreamIndex is not null)
        {
            *actualStreamIndex = 0;
        }
        if (streamFlags is not null)
        {
            *streamFlags = 0;
        }
        if (timestamp is not null)
        {
            *timestamp = 0;
        }
        if (sample is not null)
        {
            *sample = null;
        }
        return UnexpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int Flush(void* self, uint streamIndex)
    {
        RecordExpected(self);
        actualUInt32 = streamIndex;
        return ExpectedHResult;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int UnexpectedUInt32(void* self, uint value)
    {
        RecordUnexpected(self);
        return UnexpectedHResult;
    }
}

#pragma warning restore CA1416
