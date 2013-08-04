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
using System.Threading;

namespace Sidi.Caching
{
    /// <summary>
    /// Least Recently Used Cache.
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Value"></typeparam>
    public class LruCache<Key, Value> : ICache<Key, Value>, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxCount">Maximal number of items the cache can hold.</param>
        /// <param name="provideValue">Delegate to create a cache item if it is not in the cache.</param>
        public LruCache(int maxCount, Func<Key, Value> provideValue)
        {
            this.maxCount = maxCount;
            this.provideValue = provideValue;
        }
        
        class DictionaryItem
        {
            public DictionaryItem(Key key, Value value)
            {
                this.value = value;
                this.usageEntry = new LinkedListNode<Key>(key);
                this.usageTime = DateTime.Now;
            }

            public Value value;
            public LinkedListNode<Key> usageEntry;
            public DateTime usageTime;
        }

        LinkedList<Key> usage = new LinkedList<Key>();
        Dictionary<Key, DictionaryItem> dictionary = new Dictionary<Key,DictionaryItem>();
        int maxCount;
        Func<Key, Value> provideValue;

        public void Reset(Key key)
        {
            lock (this)
            {
                if (dictionary.ContainsKey(key))
                {
                    DictionaryItem di = dictionary[key];
                    usage.Remove(di.usageEntry);
                    dictionary.Remove(key);
                }
            }
        }

        public void Clear()
        {
            ShrinkTo(0);
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
                    if (dictionary.ContainsKey(key))
                    {
                        DictionaryItem di = dictionary[key];
                        Use(di);
                        return di.value;
                    }
                    else
                    {
                        Monitor.Exit(this);
                        Value v = default(Value);
                        try
                        {
                            v = provideValue(key);
                        }
                        finally
                        {
                            Monitor.Enter(this);
                        }
                        Update(key, v);
                        return v;
                    }
                }
            }
        }

        private void Use(DictionaryItem di)
        {
            lock (this)
            {
                di.usageTime = DateTime.Now;
                usage.Remove(di.usageEntry);
                usage.AddFirst(di.usageEntry);
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
                if (Contains(key))
                {
                    dictionary[key].value = value;
                }
                else
                {
                    DictionaryItem di = new DictionaryItem(key, value);
                    dictionary[key] = di;
                    usage.AddFirst(di.usageEntry);
                    ShrinkTo(maxCount);
                }
            }
        }

        void CheckInvariant()
        {
            lock (this)
            {
                if (!(dictionary.Count == usage.Count))
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
                while (dictionary.Count > newCount)
                {
                    Key expiredKey = usage.Last.Value;
                    usage.RemoveLast();
                    if (dictionary.ContainsKey(expiredKey))
                    {
                        Value expiredValue = dictionary[expiredKey].value;
                        dictionary.Remove(expiredKey);
                        if (expiredValue is IDisposable)
                        {
                            ((IDisposable)expiredValue).Dispose();
                        }
                    }
                }
            }
        }

        public int MaxCount
        {
            get { return maxCount; }
        }

        public int Count
        {
            get { lock (this) { return dictionary.Count; } }
        }

        public DateTime OldestUsageTime
        {
            get { lock (this) { return dictionary[usage.Last.Value].usageTime; }  }
        }

        public bool Contains(Key key)
        {
            lock (this) { return dictionary.ContainsKey(key); }
        }

        public bool TryGetValue(Key key, out Value value)
        {
            lock (this)
            {
                DictionaryItem item;
                if (!dictionary.TryGetValue(key, out item))
                {
                    value = default(Value);
                    return false;
                }
                value = item.value;
                return true;
            }
        }

        private bool disposed = false;
            
        //Implement IDisposable.
        public void Dispose()
        {
          Dispose(true);
          GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
          if (!disposed)
          {
            if (disposing)
            {
                Clear();
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
            disposed = true;
          }
        }

        // Use C# destructor syntax for finalization code.
        ~LruCache()
        {
          // Simply call Dispose(false).
          Dispose(false);
        }    
    
    }
}
