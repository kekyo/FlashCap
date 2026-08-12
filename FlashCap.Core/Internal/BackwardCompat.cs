////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using FlashCap.Internal;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET35 || NET40 || NET45
namespace System
{
    internal static class ArrayEx
    {
        private static class EmptyArray<T>
        {
            public static readonly T[] Empty = new T[0];
        }

#if NET45
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T[] Empty<T>() =>
            EmptyArray<T>.Empty;
    }

    namespace Text
    {
        internal static class EncodingEx
        {
            public static unsafe string GetString(
                this Encoding encoding, byte* bytes, int byteCount)
            {
                var stringData = new byte[byteCount];
                Marshal.Copy((IntPtr)bytes, stringData, 0, stringData.Length);
                return Encoding.UTF8.GetString(stringData);
            }
        }
    }

    namespace Runtime.InteropServices
    {
        internal static class MarshalEx
        {
            public static int SizeOf<T>() =>
                Marshal.SizeOf(typeof(T));
        }
    }
}
#else
namespace System
{
    internal static class ArrayEx
    {
#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T[] Empty<T>() =>
            Array.Empty<T>();
    }

    namespace Runtime.InteropServices
    {
        internal static class MarshalEx
        {
            public static int SizeOf<T>() =>
                Marshal.SizeOf<T>();
        }
    }
}
#endif

#if NETFRAMEWORK || NETSTANDARD1_3
namespace System
{
    internal readonly struct ValueTuple<T1, T2>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;

        public ValueTuple(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }
    }
}
#endif

namespace System.Linq
{
    internal static partial class EnumerableExtension
    {
        public static IEnumerable<U> Collect<T, U>(
            this IEnumerable<T> enumerable, Func<T, U?> selector)
        {
            foreach (var value in enumerable)
            {
                if (selector(value) is { } mapped)
                {
                    yield return mapped;
                }
            }
        }
        
        public static IEnumerable<U> CollectWhile<T, U>(
            this IEnumerable<T> enumerable, Func<T, U?> selector)
        {
            foreach (var value in enumerable)
            {
                if (selector(value) is { } mapped)
                {
                    yield return mapped;
                }
                else
                {
                    break;
                }
            }
        }
    }
}

#if NETSTANDARD1_3
namespace System.Security
{
    // HACK: dummy
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method)]
    internal sealed class SuppressUnmanagedCodeSecurityAttribute : Attribute
    {
    }
}

namespace System.Diagnostics
{
    internal static class Trace
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLine(object? obj) =>
            Debug.WriteLine(obj);
    }
}

namespace System.Threading
{
    internal enum ApartmentState
    {
        STA,
        MTA,
        Unknown,
    }

    internal delegate void ThreadStart();

    internal sealed class Thread
    {
        private readonly ThreadStart entryPoint;
        private ApartmentState state = ApartmentState.Unknown;
        private Tasks.Task? task;

        public Thread(ThreadStart entryPoint) =>
            this.entryPoint = entryPoint;

        public bool IsBackground { get; set; }

        public void SetApartmentState(ApartmentState state) =>
            this.state = state;

        private void EntryPoint()
        {
            if (NativeMethods.IsWindows())
            {
                switch (this.state)
                {
                    case ApartmentState.STA:
                        NativeMethods.CoUninitialize();   // DIRTY
                        NativeMethods.CoInitializeEx(
                            IntPtr.Zero, NativeMethods.COINIT.APARTMENTTHREADED);
                        break;
                    case ApartmentState.MTA:
                        NativeMethods.CoUninitialize();   // DIRTY
                        NativeMethods.CoInitializeEx(
                            IntPtr.Zero, NativeMethods.COINIT.MULTITHREADED);
                        break;
                }
            }
            try
            {
                this.entryPoint();
            }
            finally
            {
                NativeMethods.CoUninitialize();   // DIRTY
            }
        }

        public void Start() =>
            this.task = Tasks.Task.Factory.StartNew(
                this.EntryPoint,
                Tasks.TaskCreationOptions.LongRunning);

        public void Join() =>
            this.task?.Wait();

        public void Join(TimeSpan timeout) =>
            this.task?.Wait(timeout);
    }

    internal delegate void WaitCallback(object? parameter);

    internal static class ThreadPool
    {
        public static bool QueueUserWorkItem(WaitCallback workItem, object? parameter)
        {
            Tasks.Task.Factory.StartNew(p => workItem(p), parameter);
            return true;
        }
    }
}
#endif

#if !(NET35 || NET40)
namespace System.Threading.Tasks
{
    internal static class TaskCompat
    {
#if NET45
        public static Task CompletedTask =>
            Task.FromResult(0);
#else
        public static Task CompletedTask =>
            Task.CompletedTask;
#endif

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Task<Task> WhenAny(params Task[] tasks) =>
            Task.WhenAny(tasks);

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Task<T> FromResult<T>(T value) =>
            Task.FromResult(value);
    }
}
#endif

#if NET35 || NET40
namespace System.Threading.Tasks
{
    internal static class TaskCompat
    {
        public static Task CompletedTask =>
            TaskEx.FromResult(true);
        public static Task<T> FromResult<T>(T value) =>
            TaskEx.FromResult(value);

        public static Task<Task> WhenAny(params Task[] tasks) =>
            TaskEx.WhenAny(tasks);
    }
}

namespace System.Runtime.ExceptionServices
{
    internal sealed class ExceptionDispatchInfo
    {
        private readonly Exception ex;
        private readonly StackTrace stackTrace;

        private ExceptionDispatchInfo(Exception ex)
        {
            this.ex = ex;
            this.stackTrace = new StackTrace(ex);
        }

        public void Throw() =>
            throw this.ex;     // IGNORED: Will lost stack information.

        public static ExceptionDispatchInfo Capture(Exception ex) =>
            new ExceptionDispatchInfo(ex);
    }
}
#endif

#if NETSTANDARD1_3
namespace System.Threading.Tasks
{
    internal static class Parallel
    {
        public static void For(int fromInclusive, int toExclusive, Action<int> body)
        {
            using var waiter = new ManualResetEventSlim(false);
            var running = 1;

            var trampoline = new WaitCallback(parameter =>
            {
                try
                {
                    body((int)parameter!);
                }
                finally
                {
                    if (Interlocked.Decrement(ref running) <= 0)
                    {
                        waiter.Set();
                    }
                }
            });

            for (var index = fromInclusive; index < toExclusive; index++)
            {
                Interlocked.Increment(ref running);
                ThreadPool.QueueUserWorkItem(trampoline, index);
            }

            if (Interlocked.Decrement(ref running) >= 1)
            {
                waiter.Wait();
            }
        }
    }
}
#endif
#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}

namespace System.Runtime.Versioning
{
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Class |
        AttributeTargets.Constructor |
        AttributeTargets.Delegate |
        AttributeTargets.Enum |
        AttributeTargets.Event |
        AttributeTargets.Field |
        AttributeTargets.Interface |
        AttributeTargets.Method |
        AttributeTargets.Module |
        AttributeTargets.Property |
        AttributeTargets.Struct,
        AllowMultiple = true,
        Inherited = false)]
    internal sealed class SupportedOSPlatformAttribute : Attribute
    {
        public SupportedOSPlatformAttribute(string platformName)
        {
            PlatformName = platformName;
        }
        public string PlatformName { get; }
    }
}
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
    internal sealed class RequiresUnreferencedCodeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresUnreferencedCodeAttribute"/> class
        /// with the specified message.
        /// </summary>
        /// <param name="message">
        /// A message that contains information about the usage of unreferenced code.
        /// </param>
        public RequiresUnreferencedCodeAttribute(string message)
        {
            Message = message;
        }

        /// <summary>
        /// When set to true, indicates that the annotation should not apply to static members.
        /// </summary>
        public bool ExcludeStatics { get; set; }

        /// <summary>
        /// Gets a message that contains information about the usage of unreferenced code.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets or sets an optional URL that contains more information about the method,
        /// why it requires unreferenced code, and what options a consumer has to deal with it.
        /// </summary>
        public string? Url { get; set; }
    }
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    internal sealed class UnconditionalSuppressMessageAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnconditionalSuppressMessageAttribute"/>
        /// class, specifying the category of the tool and the identifier for an analysis rule.
        /// </summary>
        /// <param name="category">The category for the attribute.</param>
        /// <param name="checkId">The identifier of the analysis rule the attribute applies to.</param>
        public UnconditionalSuppressMessageAttribute(string category, string checkId)
        {
            Category = category;
            CheckId = checkId;
        }

        /// <summary>
        /// Gets the category identifying the classification of the attribute.
        /// </summary>
        /// <remarks>
        /// The <see cref="Category"/> property describes the tool or tool analysis category
        /// for which a message suppression attribute applies.
        /// </remarks>
        public string Category { get; }

        /// <summary>
        /// Gets the identifier of the analysis tool rule to be suppressed.
        /// </summary>
        /// <remarks>
        /// Concatenated together, the <see cref="Category"/> and <see cref="CheckId"/>
        /// properties form a unique check identifier.
        /// </remarks>
        public string CheckId { get; }

        /// <summary>
        /// Gets or sets the scope of the code that is relevant for the attribute.
        /// </summary>
        /// <remarks>
        /// The Scope property is an optional argument that specifies the metadata scope for which
        /// the attribute is relevant.
        /// </remarks>
        public string? Scope { get; set; }

        /// <summary>
        /// Gets or sets a fully qualified path that represents the target of the attribute.
        /// </summary>
        /// <remarks>
        /// The <see cref="Target"/> property is an optional argument identifying the analysis target
        /// of the attribute. An example value is "System.IO.Stream.ctor():System.Void".
        /// Because it is fully qualified, it can be long, particularly for targets such as parameters.
        /// The analysis tool user interface should be capable of automatically formatting the parameter.
        /// </remarks>
        public string? Target { get; set; }

        /// <summary>
        /// Gets or sets an optional argument expanding on exclusion criteria.
        /// </summary>
        /// <remarks>
        /// The <see cref="MessageId "/> property is an optional argument that specifies additional
        /// exclusion where the literal metadata target is not sufficiently precise. For example,
        /// the <see cref="UnconditionalSuppressMessageAttribute"/> cannot be applied within a method,
        /// and it may be desirable to suppress a violation against a statement in the method that will
        /// give a rule violation, but not against all statements in the method.
        /// </remarks>
        public string? MessageId { get; set; }

        /// <summary>
        /// Gets or sets the justification for suppressing the code analysis message.
        /// </summary>
        public string? Justification { get; set; }
    }
}
#endif

#if !NET6_0_OR_GREATER
namespace System.Runtime.Versioning
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    internal sealed class SupportedOSPlatformGuardAttribute : Attribute
    {
        public SupportedOSPlatformGuardAttribute(string platformName)
        {
            PlatformName = platformName;
        }
        public string PlatformName { get; }
    }
}
#endif

#if !NET7_0_OR_GREATER
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Indicates that the specified method requires the ability to generate new code at runtime,
    /// for example through <see cref="Reflection"/>.
    /// </summary>
    /// <remarks>
    /// This allows tools to understand which methods are unsafe to call when compiling ahead of time.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
    internal sealed class RequiresDynamicCodeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresDynamicCodeAttribute"/> class
        /// with the specified message.
        /// </summary>
        /// <param name="message">
        /// A message that contains information about the usage of dynamic code.
        /// </param>
        public RequiresDynamicCodeAttribute(string message)
        {
            Message = message;
        }

        /// <summary>
        /// When set to true, indicates that the annotation should not apply to static members.
        /// </summary>
        public bool ExcludeStatics { get; set; }

        /// <summary>
        /// Gets a message that contains information about the usage of dynamic code.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets or sets an optional URL that contains more information about the method,
        /// why it requires dynamic code, and what options a consumer has to deal with it.
        /// </summary>
        public string? Url { get; set; }
    }
}
#endif
