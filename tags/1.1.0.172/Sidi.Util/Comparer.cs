using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Util
{
    public class Comparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _comparer;

        public Comparer(Func<T, T, bool> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            _comparer = comparer;
        }

        public bool Equals(T x, T y)
        {
            return _comparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.ToString().ToLower().GetHashCode();
        }
    }

    public class PropertyComparer<T, F> : IEqualityComparer<T>, IComparer<T>
    {
        private readonly Func<T, F> _fieldgetter;

        public PropertyComparer(Func<T, F> fieldgetter)
        {
            if (fieldgetter == null)
                throw new ArgumentNullException("comparer");

            _fieldgetter = fieldgetter;
        }

        public bool Equals(T x, T y)
        {
            return _fieldgetter(x).Equals(_fieldgetter(y));
        }

        public int GetHashCode(T obj)
        {
            return _fieldgetter(obj).GetHashCode();
        }

        #region IComparer<T> Members

        public int Compare(T x, T y)
        {
            return ((IComparable)_fieldgetter(x)).CompareTo(_fieldgetter(y));
        }

        #endregion
    }

    public static class ComparerEx
    {
        public static PropertyComparer<T, F> Comparer<T, F>(this IEnumerable<T> e, Func<T, F> f)
        {
            return new PropertyComparer<T, F>(f);
        }
    }
}
