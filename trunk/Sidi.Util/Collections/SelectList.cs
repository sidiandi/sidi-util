// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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

namespace Sidi.Collections
{
    public class SelectList<X, Y> : IList<Y>
    {
        public SelectList(IList<X> x, Func<X, Y> f)
        {
            this.x = x;
            this.f = f;
        }

        IList<X> x;
        Func<X, Y> f;

        public int IndexOf(Y item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, Y item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            x.RemoveAt(index);
        }

        public Y this[int index]
        {
            get
            {
                return f(x[index]);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(Y item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            x.Clear();
        }

        public bool Contains(Y item)
        {
            return this.Any(i => i.Equals(item));
        }

        public void CopyTo(Y[] array, int arrayIndex)
        {
            int j=0;
            for (int i = arrayIndex; i < Count; ++i)
            {
                array[i] = this[j++];
            }
        }

        public int Count
        {
            get { return x.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(Y item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Y> GetEnumerator()
        {
            return x.Select(i => f(i)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return x.Select(i => f(i)).GetEnumerator();
        }
    }
}
