using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Collections
{
    public class EnumList<T> : IList<T>
    {
        public EnumList(IEnumerator<T> e)
        {
            this.e = e;
        }

        IEnumerator<T> e;
        List<T> buffer = new List<T>();

        bool FillBuffer(int count)
        {
            while (buffer.Count < count)
            {
                if (!e.MoveNext())
                {
                    return false;
                }
                buffer.Add(e.Current);
            }
            return true;
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            FillBuffer(index);
            buffer.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            FillBuffer(index + 1);
            buffer.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                FillBuffer(index + 1);
                return buffer[index];
            }
            set
            {
                FillBuffer(index + 1);
                buffer[index] = value;
            }
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            buffer.Clear();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return Int32.MaxValue; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; ; ++i)
            {
                if (!FillBuffer(i + 1))
                {
                    break;
                }
                yield return buffer[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
