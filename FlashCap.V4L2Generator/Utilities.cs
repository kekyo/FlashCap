////////////////////////////////////////////////////////////////////////////
//
// FlashCap - Independent camera capture library.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

namespace FlashCap
{
    internal static class Utilities
    {
        public static IEnumerable<T> TraverseMany<T>(
            this T value, Func<T, IEnumerable<T>> predicate)
            where T : class
        {
            yield return value;
            foreach (var ii in predicate(value).
                SelectMany(iv => TraverseMany<T>(iv, predicate)))
            {
                yield return ii;
            }
        }

        public static IEnumerable<KeyValuePair<T, string>> MakeShorterSnakeCase<T>(
            this IEnumerable<T> enumerable, Func<T, string> selector)
        {
            var buffer = new List<KeyValuePair<T, string[]>>();
            var lastElements = new string[0];
            
            foreach (var item in enumerable)
            {
                var name = selector(item);
                var splitted = name.Split('_');
                buffer.Add(new KeyValuePair<T, string[]>(item, splitted));

                if (lastElements.Length >= 1)
                {
                    var count = splitted.
                        Zip(lastElements, (s, le) => s == le).
                        TakeWhile(eq => eq).
                        Count();
                    lastElements = splitted.
                        Take(count).
                        ToArray();
                }
                else
                {
                    lastElements = splitted.
                        Take(splitted.Length - 1).
                        ToArray();
                }
            }

            return buffer.Select(entry =>
                new KeyValuePair<T, string>(
                    entry.Key,
                    string.Join("_", entry.Value.Skip(
                        Array.FindLastIndex(
                            entry.Value,
                            lastElements.Length,
                            element => !char.IsDigit(element[0]))))));
        }
    }
}
