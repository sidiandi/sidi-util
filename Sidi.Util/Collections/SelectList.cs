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
