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

namespace Sidi.Collections
{
    /// <summary>
    /// Least Recently Used Cache.
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Value"></typeparam>
    public class LruCache<Key, Value>
    {
        /// <summary>
        /// Delegate to create a cache item if it is not in the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The newly created item.</returns>
        public delegate Value ProvideValue(Key key);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxCount">Maximal number of items the cache can hold.</param>
        /// <param name="provideValue">Delegate to create a cache item if it is not in the cache.</param>
        public LruCache(int maxCount, ProvideValue provideValue)
        {
            m_maxCount = maxCount;
            m_provideValue = provideValue;
        }
        
        class DictionaryItem
        {
            public DictionaryItem(Key key, Value value)
            {
                m_value = value;
                m_usageEntry = new LinkedListNode<Key>(key);
                m_usageTime = DateTime.Now;
            }

            public Value m_value;
            public LinkedListNode<Key> m_usageEntry;
            public DateTime m_usageTime;
        }

        LinkedList<Key> m_usage = new LinkedList<Key>();
        Dictionary<Key, DictionaryItem> m_dictionary = new Dictionary<Key,DictionaryItem>();
        int m_maxCount;
        ProvideValue m_provideValue;

        public void Reset(Key key)
        {
            lock (this)
            {
                if (m_dictionary.ContainsKey(key))
                {
                    DictionaryItem di = m_dictionary[key];
                    m_usage.Remove(di.m_usageEntry);
                    m_dictionary.Remove(key);
                }
            }
        }

        public void Clear()
        {
            lock (this)
            {
                m_dictionary.Clear();
                m_usage.Clear();
            }
        }

        /// <summary>
        /// Indexer to get a cached item. 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Value this[Key key]
        {
            get
            {
                lock (this)
                {
                    if (m_dictionary.ContainsKey(key))
                    {
                        DictionaryItem di = m_dictionary[key];
                        Use(di);
                        return di.m_value;
                    }
                    else
                    {
                        if (m_provideValue != null)
                        {
                            Value v = m_provideValue(key);
                            Update(key, v);
                            return v;
                        }
                        else
                        {
                            return default(Value);
                        }
                    }
                }
            }
        }

        private void Use(DictionaryItem di)
        {
            lock (this)
            {
                di.m_usageTime = DateTime.Now;
                m_usage.Remove(di.m_usageEntry);
                m_usage.AddFirst(di.m_usageEntry);
            }
        }

        /// <summary>
        /// Updates a cache entry without changing the least-recently-used position.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Update(Key key, Value value)
        {
            lock (this)
            {
                if (IsCached(key))
                {
                    m_dictionary[key].m_value = value;
                }
                else
                {
                    DictionaryItem di = new DictionaryItem(key, value);
                    m_dictionary[key] = di;
                    m_usage.AddFirst(di.m_usageEntry);
                    ShrinkTo(m_maxCount);
                }
            }
        }

        void checkInvariant()
        {
            lock (this)
            {
                if (!(m_dictionary.Count == m_usage.Count))
                {
                    throw new Exception();
                }
            }
        }
        
        /// <summary>
        /// Removes cached items if there are more than newCount.
        /// </summary>
        /// <param name="newCount"></param>
        void ShrinkTo(int newCount)
        {
            lock (this)
            {
                while (m_dictionary.Count > newCount)
                {
                    Key expiredKey = m_usage.Last.Value;
                    m_usage.RemoveLast();
                    if (m_dictionary.ContainsKey(expiredKey))
                    {
                        Value expiredValue = m_dictionary[expiredKey].m_value;
                        m_dictionary.Remove(expiredKey);
                        if (expiredValue as IDisposable != null)
                        {
                            ((IDisposable)expiredValue).Dispose();
                        }
                    }
                }
            }
        }

        public int MaxCount
        {
            get { return m_maxCount; }
        }

        public int Count
        {
            get { lock (this) { return m_dictionary.Count; } }
        }

        public DateTime OldestUsageTime
        {
            get { lock (this) { return m_dictionary[m_usage.Last.Value].m_usageTime; }  }
        }

        public bool IsCached(Key key)
        {
            lock (this) { return m_dictionary.ContainsKey(key); }
        }
    }
}
