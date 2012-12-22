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
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace Sidi.Collections
{
    public interface IState<T>
    {
        void Save(T x);
        void Restore(T x);
    }
    
    public class History<T, State> where State : IState<T>, new()
    {
        T m_object;
        
        public History(T x)
        {
            m_object = x;
        }

        List<State> m_list = new List<State>();
        int m_index = 0;
        bool m_restoring = false;

        public void Save()
        {
            if (m_restoring)
            {
                return;
            }

            m_list.RemoveRange(m_index, m_list.Count - m_index);
            State state = new State();
            state.Save(m_object);
            m_list.Add(state);
            m_index = m_list.Count;
        }

        public bool Back()
        {
            if (m_index <= 0)
            {
                return false;
            }

            if (m_index >= m_list.Count)
            {
                State state = new State();
                state.Save(m_object);
                m_list.Add(state);
            }
            
            --m_index;
            Restore(m_list[m_index]);
            return true;
        }

        public bool Forward()
        {
            if (m_index < m_list.Count-1)
            {
                ++m_index;
                Restore(m_list[m_index]);
                return true;
            }
            else
            {
                return false;
            }
        }

        void Restore(State state)
        {
            m_restoring = true;
            try
            {
                state.Restore(m_object);
            }
            finally
            {
                m_restoring = false;
            }
        }
    }

    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class LimitedQueue<T> : List<T>
    {
        private int m_maxCount = 100;

        public LimitedQueue(int maxCount)
        {
            m_maxCount = maxCount;
        }

        public new void Add(T t)
        {
            base.Add(t);
            if (Count > m_maxCount)
            {
                while (Count > m_maxCount - m_maxCount / 10)
                {
                    RemoveAt(0);
                }
            }
        }
    }
}
