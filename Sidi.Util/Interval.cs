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
