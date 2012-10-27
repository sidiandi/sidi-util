using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                    log.Warn(Sidi.Extensions.StringEx.SafeToString(i), e);
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
