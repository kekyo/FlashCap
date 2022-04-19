////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System.Collections;
using FlashCap.Internal;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#if NET20
namespace System.Runtime.CompilerServices
{
    internal sealed class ExtensionAttribute : Attribute
    {
    }
}

namespace System
{
    internal delegate void Action();
    internal delegate void Action<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2);
    internal delegate TR Func<T0, TR>(T0 arg0);
}

namespace System.Collections.Generic
{
    internal sealed class HashSet<T> : IEnumerable<T>
    {
        private readonly Dictionary<T, bool> inner = new();

        public void Add(T value) =>
            this.inner.Add(value, true);

        public IEnumerator<T> GetEnumerator() =>
            this.inner.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            this.inner.Keys.GetEnumerator();
    }
}

namespace System.Linq
{
    internal static partial class Enumerable
    {
        public static IEnumerable<T> Cast<T>(this IEnumerable enumerable)
        {
            foreach (var value in enumerable)
            {
                yield return (T)value;
            }
        }
        
        public static IEnumerable<int> Range(int begin, int count)
        {
            for (var index = 0; index < count; index++)
            {
                yield return begin + index;
            }
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, IEnumerable<T> next)
        {
            foreach (var item in enumerable)
            {
                yield return item;
            }
            foreach (var item in next)
            {
                yield return item;
            }
        }

        public static IEnumerable<T> Where<T>(
            this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            foreach (var value in enumerable)
            {
                if (predicate(value))
                {
                    yield return value;
                }
            }
        }

        public static IEnumerable<U> Select<T, U>(
            this IEnumerable<T> enumerable, Func<T, U> selector)
        {
            foreach (var value in enumerable)
            {
                yield return selector(value);
            }
        }

        public static IEnumerable<U> SelectMany<T, U>(
            this IEnumerable<T> enumerable, Func<T, IEnumerable<U>> binder)
        {
            foreach (var value in enumerable)
            {
                foreach (var innerValue in binder(value))
                {
                    yield return innerValue;
                }
            }
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> enumerable)
        {
            foreach (var value in enumerable)
            {
                return value;
            }

            return default(T)!;
        }

        public static T[] ToArray<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is ICollection<T> coll)
            {
                var arr = new T[coll.Count];
                coll.CopyTo(arr, 0);
                return arr;
            }
            else
            {
                var list = new List<T>();
                foreach (var item in enumerable)
                {
                    list.Add(item);
                }
                return list.ToArray();
            }
        }

        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> enumerable)
        {
            var dict = new Dictionary<T, T>();
            foreach (var item in enumerable)
            {
                if (!dict.ContainsKey(item))
                {
                    dict.Add(item, item);
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> OrderByDescending<T, U>(
            this IEnumerable<T> enumerable, Func<T, U> keySelector)
            where U : IComparable<U>
        {
            var list = new List<T>(enumerable);
            list.Sort((a, b) => 0 - keySelector(a).CompareTo(keySelector(b)));
            return list;
        }

        public static int Max(this IEnumerable<int> enumerable)
        {
            var max = default(int?);
            foreach (var item in enumerable)
            {
                if (item > (max ?? int.MinValue))
                {
                    max = item;
                }
            }
            throw new InvalidOperationException();
        }
    }
}
#endif

#if NET20 || NET35 || NET40 || NET45
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
            if (NativeMethods.CurrentPlatform == NativeMethods.Platforms.Windows)
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

#if !(NET20 || NET35 || NET40)
namespace System.Threading.Tasks
{
    internal static class TaskEx
    {
#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Task<T> FromResult<T>(T value) =>
            Task.FromResult(value);
    }
}
#endif

#if NET20 || NET35 || NET40
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

#if NET20
namespace System.Threading
{
    internal sealed class ManualResetEventSlim : IDisposable
    {
        private readonly ManualResetEvent mre;

        public ManualResetEventSlim(bool initialState) =>
            this.mre = new(initialState);

        public void Dispose() =>
            this.mre.Close();

        public WaitHandle WaitHandle =>
            this.mre;

        public void Set() =>
            this.mre.Set();

        public void Reset() =>
            this.mre.Reset();

        public void Wait() =>
            this.mre.WaitOne();
        public void Wait(TimeSpan timeout) =>
            this.mre.WaitOne(timeout);
    }
}
#endif

#if NET20 || NETSTANDARD1_3
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
