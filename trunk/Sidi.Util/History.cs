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
