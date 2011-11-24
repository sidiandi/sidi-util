using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<KeyValuePair<int, T>> Counted<T>(this IEnumerable<T> e)
        {
            int index = 0;
            return e.Select(i => new KeyValuePair<int, T>(index++, i));
        }

        /// <summary>
        /// like select, but silently ignores exceptions in f
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
