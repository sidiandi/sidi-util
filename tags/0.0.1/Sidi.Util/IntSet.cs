// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;
using Sidi.Util;

namespace Sidi.Collections
{
    public interface IIntSet : IEnumerable<int>, ICloneable
    {
        void Clear();
        void Add(Interval x);
        void Remove(Interval x);
        void Set(Interval x);
        void Intersect(IntSet x);
        bool IsEmpty { get; }
        bool Contains(int x);
    }

    public class IntSet : IIntSet
    {
        List<Interval> m_intervals = new List<Interval>();

        class Comparer : IComparer<Interval>
        {
            #region IComparer<Interval> Members

            public int  Compare(Interval x, Interval y)
            {
 	            return x.Begin.CompareTo(y.Begin);
            }

            #endregion
        }

        public IntSet()
        {
        }

        public IntSet(IntSet source)
        {
            m_intervals = new List<Interval>(source.m_intervals);
        }

        public IntSet(Interval interval)
        {
            m_intervals.Add(interval);
        }

        #region IIntSet Members

        public void Clear()
        {
            m_intervals.Clear();
        }

        Comparer m_comparer = new Comparer();

        int Find(int x)
        {
            int index = m_intervals.BinarySearch(new Interval(x,x), m_comparer);
            if (index < 0)
            {
                return ~index;
            }
            else
            {
                return index;
            }
        }

        public void Add(Interval x)
        {
            int index = Find(x.Begin);
            if (index > 0 && m_intervals[index - 1].End >= x.Begin)
            {
                --index;
                m_intervals[index].End = x.End;
            }
            else
            {
                m_intervals.Insert(index, x);
            }
            while (index + 1 < m_intervals.Count && m_intervals[index+1].Begin <= m_intervals[index].End)
            {
                m_intervals[index].End = m_intervals[index+1].End;
                m_intervals.RemoveAt(index+1);
            }
        }

        public void Set(Interval x)
        {
            Clear();
            Add(x);
        }

        public void Remove(Interval x)
        {
            IntSet i = new IntSet();
            i.Add(new Interval(Int32.MinValue, x.Begin));
            i.Add(new Interval(x.End, Int32.MaxValue));
            Intersect(i);
        }

        public void Intersect(IntSet x)
        {
            List<Interval> a = m_intervals;
            List<Interval> b = x.m_intervals;
            int ia = 0;
            int ib = 0;

            List<Interval> result = new List<Interval>();

            for (ia = 0; ia < a.Count; ++ia)
            {
                ib = x.Find(a[ia].Begin) - 1;
                if (ib < 0)
                {
                    ib = 0;
                }
                for (; ib < b.Count && b[ib].Begin < a[ia].End; ++ib)
                {
                    Interval r = a[ia].Intersect(b[ib]);
                    if (r.Length > 0)
                    {
                        result.Add(r);
                    }
                }
            }
            m_intervals = result;
        }

        public bool IsEmpty
        {
            get { return m_intervals.Count == 0; }
        }

        public bool Contains(int x)
        {
            int index = m_intervals.BinarySearch(new Interval(x,x), m_comparer);
            if (index < 0)
            {
                index = ~index - 1;
                if (index >= 0)
                {
                    return m_intervals[index].Contains(x);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region IEnumerable<int> Members

        public IEnumerator<int> GetEnumerator()
        {
            foreach (Interval i in m_intervals)
            {
                for (int n = i.Begin; n < i.End; ++n)
                {
                    yield return n;
                }
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return new IntSet(this);
        }

        #endregion
    }
}
