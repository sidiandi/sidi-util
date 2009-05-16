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
using System.Text;

namespace Sidi.Util
{
    public class Interval : IEnumerable<int>
    {
        int m_begin;
        int m_end;

        public int End
        {
            set { m_end = value; }
            get { return m_end; }
        }

        public int Begin
        {
            set { m_begin = value; }
            get { return m_begin; }
        }

        public bool Contains(int x)
        {
            return m_begin <= x && x < m_end;
        }

        public int Length
        {
            get { return m_end - m_begin; } 
        }
        
        public int Clip(int x)
        {
            if (x < m_begin)
            {
                x = m_begin;
            }

            if (m_end <= x)
            {
                x = m_end - 1;
            }
            return x;
        }

        public Interval(int begin, int end)
        {
            m_begin = begin;
            m_end = end;
        }

        public static Interval FromClosedInterval(int x1, int x2)
        {
            if (x1 <= x2)
            {
                return new Interval(x1, x2 + 1);
            }
            else
            {
                return new Interval(x2, x1 + 1);
            }
        }

        public Interval Intersect(Interval r)
        {
            return new Interval(Math.Max(m_begin, r.m_begin), Math.Min(m_end, r.m_end));
        }

        #region IEnumerable<int> Members

        public IEnumerator<int> GetEnumerator()
        {
            int iEnd = End;
            for (int i = Begin; i < iEnd; ++i)
            {
                yield return i;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            int iEnd = End;
            for (int i = Begin;  i < iEnd; ++i)
            {
                yield return i;
            }
        }

        #endregion
    }
}
