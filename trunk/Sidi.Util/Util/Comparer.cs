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
                throw new ArgumentNullException("fieldgetter");

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

    public static class ComparerExtensions
    {
        public static PropertyComparer<T, F> Comparer<T, F>(this IEnumerable<T> e, Func<T, F> f)
        {
            return new PropertyComparer<T, F>(f);
        }
    }
}
