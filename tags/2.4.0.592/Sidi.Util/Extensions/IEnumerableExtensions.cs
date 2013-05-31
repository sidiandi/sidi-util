// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace Sidi.Extensions
{
    public static class IEnumerableExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Returns a counted (0-based) enumeration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IEnumerable<KeyValuePair<int, T>> Counted<T>(this IEnumerable<T> e)
        {
            int index = 0;
            return e.Select(i => new KeyValuePair<int, T>(index++, i));
        }

        public static IEnumerable<T> Chain<T>(T i, Func<T, T> transform)
        {
            for (; i != null; i = transform(i))
            {
                yield return i;
            }
        }

        public static IEnumerable<Y> AggregateSelect<Y, X>(this IEnumerable<X> sequence, Y initialResult, Func<Y, X, Y> selector)
        {
            foreach (var i in sequence)
            {
                initialResult = selector(initialResult, i);
                yield return initialResult;
            }
        }

        public static IEnumerable<string> JoinSelect<X>(this IEnumerable<X> sequence, string separator)
        {
            var iEnd = sequence.Count();
            for (int i=0; i<iEnd; ++i)
            {
                yield return sequence.Take(i).Join(separator);
            }
        }

        /// <summary>
        /// Like Select, but silently ignores exceptions in f
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <param name="x"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static IEnumerable<Y> SafeSelect<X, Y>(this IEnumerable<X> x, Func<X, Y> f)
        {
            foreach (var i in x)
            {
                Y y = default(Y);
                bool ok = false;
                try
                {
                    y = f(i);
                    ok = true;
                }
                catch (Exception e)
                {
                    log.Warn(Sidi.Extensions.StringExtensions.SafeToString(i), e);
                }

                if (ok)
                {
                    yield return y;
                }
            }
        }

        /// <summary>
        /// Returns the maximum element as defined by f
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="x"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static T Best<T, S>(this IEnumerable<T> x, Func<T, S> f) where S : IComparable<S>
        {
            var e = x.GetEnumerator();
            e.MoveNext();
            S max = f(e.Current);
            T best = e.Current;
            for (; e.MoveNext(); )
            {
                var s = f(e.Current);
                if (max.CompareTo(s) < 0)
                {
                    max = s;
                    best = e.Current;
                }
            }
            return best;
        }

        public static IEnumerable<KeyValuePair<K, V>> Combine<K, V>(this IEnumerable<K> keys, IEnumerable<V> values)
        {
            var v = values.GetEnumerator();
            foreach (var k in keys)
            {
                if (!v.MoveNext())
                {
                    break;
                }
                yield return new KeyValuePair<K, V>(k, v.Current);
            }
        }
    }
}