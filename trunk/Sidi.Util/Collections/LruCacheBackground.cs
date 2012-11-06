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
using System.Threading;

namespace Sidi.Collections
{
    public class LruCacheBackground<Key, Value> : IDisposable
    {
        public class EntryUpdatedEventArgs : EventArgs
        {
            public Key key;
        }

        /// <summary>
        /// Fires when an entry in the cache was updated.
        /// </summary>
        /// Warning: this event will be fired by a background thread.
        public event EventHandler<EntryUpdatedEventArgs> EntryUpdated;

        enum State
        {
            Missing,
            Loading,
            Complete
        };

        class CacheEntry : IDisposable
        {
            public CacheEntry(Value value, State state)
            {
                m_value = value;
                m_state = state;
            }

            public Value m_value;
            public State m_state;

            #region IDisposable Members

            public void Dispose()
            {
                IDisposable disposeValue = m_value as IDisposable;
                if (disposeValue != null)
                {
                    disposeValue.Dispose();
                }
                m_value = default(Value);
            }

            #endregion
        }

        public LruCache<Key, Value>.ProvideValue ProvideValue
        {
            set
            {
                m_provideValue = value;
            }
        }

        class Shared
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            public Shared()
            {
                ThreadCount = 4;
            }

            public LruCache<Key, CacheEntry> m_cache;
            public Value m_defaultValueWhileLoading = default(Value);
            public int m_workersStarted = 0;
            public List<LruCacheBackground<Key, Value>> m_instances = new List<LruCacheBackground<Key, Value>>();
            public int ThreadCount { get; set; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
            public void Worker(object state)
            {
                lock (this)
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                    for (; ; )
                    {
                        bool itemProcessed = false;
                        foreach (LruCacheBackground<Key, Value> instance in m_instances)
                        {
                            LinkedList<ProvideValueRequest> provideValueRequestQueue = instance.m_provideValueRequestQueue;
                            if (provideValueRequestQueue != null && provideValueRequestQueue.Count > 0)
                            {
                                itemProcessed = true;
                                Key k = provideValueRequestQueue.First.Value.Key;
                                provideValueRequestQueue.RemoveFirst();

                                if (m_cache.IsCached(k) && m_cache[k].m_state != State.Complete)
                                {
                                    Monitor.Exit(this);
                                    Value v = default(Value);
                                    try
                                    {
                                        v = instance.m_provideValue(k);
                                    }
                                    catch (Exception exception)
                                    {
                                        log.Error(String.Format("Providing value for key {0} failed.", k), exception);
                                    }

                                    Monitor.Enter(this);
                                    if ((object)v == null)
                                    {
                                        m_cache.Update(k, new CacheEntry(v, State.Missing));
                                    }
                                    else
                                    {
                                        m_cache.Update(k, new CacheEntry(v, State.Complete));
                                        instance.OnEntryUpdated(k);
                                    }
                                }
                            }
                        }

                        if (!itemProcessed)
                        {
                            break;
                        }
                    }
                    --m_workersStarted;
                }
            }
        }

        Shared m_shared = new Shared();
        
        class ProvideValueRequest
        {
            Key m_key;
            DateTime m_requestTime;

            public ProvideValueRequest(Key key)
            {
                m_key = key;
                m_requestTime = DateTime.Now;
            }

            public DateTime RequestTime
            {
                get { return m_requestTime; }
            }

            public Key Key { get { return m_key; } }
        }

        LinkedList<ProvideValueRequest> m_provideValueRequestQueue = new LinkedList<ProvideValueRequest>();
        LruCache<Key, Value>.ProvideValue m_provideValue;

        public LruCacheBackground(int maxCount)
        {
            m_shared.m_cache = new LruCache<Key, CacheEntry>(maxCount, null);
            m_shared.m_instances.Add(this);
        }

        public LruCacheBackground(int maxCount, LruCache<Key, Value>.ProvideValue provideValue)
        {
            m_shared.m_cache = new LruCache<Key, CacheEntry>(maxCount, null);
            m_provideValue = provideValue;
            m_shared.m_instances.Add(this);
        }

        public LruCacheBackground(LruCacheBackground<Key, Value> shareFrom)
        {
            m_shared = shareFrom.m_shared;
            m_shared.m_instances.Add(this);
        }

        protected virtual void OnEntryUpdated(Key key)
        {
            EntryUpdatedEventArgs a = new EntryUpdatedEventArgs();
            a.key = key;

            foreach (LruCacheBackground<Key, Value> instance in m_shared.m_instances)
            {
                if (instance.EntryUpdated != null)
                {
                    instance.EntryUpdated(this, a);
                }
            }
        }

        public void Reset(Key key)
        {
            lock (this)
            {
                m_shared.m_cache.Reset(key);
                OnEntryUpdated(key);
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
                    if (!m_shared.m_cache.IsCached(key))
                    {
                        Load(key);
                        return m_shared.m_defaultValueWhileLoading;
                    }
                    else
                    {
                        CacheEntry e = m_shared.m_cache[key];
                        Value v = default(Value);
                        switch (e.m_state)
                        {
                            case State.Missing:
                                Load(key);
                                v = m_shared.m_defaultValueWhileLoading;
                                break;
                            case State.Loading:
                                Load(key);
                                v = m_shared.m_defaultValueWhileLoading;
                                break;
                            case State.Complete:
                                v = e.m_value;
                                break;
                        }
                        return v;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the item specified by key is in the cache. 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(Key key)
        {
            lock (m_shared)
            {
                if (m_shared.m_cache.IsCached(key))
                {
                    return m_shared.m_cache[key].m_state == State.Complete;
                }
                else
                {
                    return false;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void Load(Key key)
        {
            lock (m_shared)
            {
                if (!m_shared.m_cache.IsCached(key))
                {
                    m_shared.m_cache.Update(key, new CacheEntry(default(Value), State.Loading));
                }
                m_provideValueRequestQueue.AddFirst(new ProvideValueRequest(key));

                // limit request queue length
                int maxRequestQueueCount = m_shared.m_cache.MaxCount;
                while (m_provideValueRequestQueue.Count > maxRequestQueueCount)
                {
                    m_provideValueRequestQueue.RemoveLast();
                }
                if (m_shared.m_cache.Count == m_shared.m_cache.MaxCount)
                {
                    DateTime minRequestTime = m_shared.m_cache.OldestUsageTime;
                    for (; m_provideValueRequestQueue.Last.Value.RequestTime < minRequestTime;)
                    {
                        m_provideValueRequestQueue.RemoveLast();
                    }
                }

                if (m_shared.m_workersStarted <= 0)
                {
                    var threadsToStart = Math.Min(ThreadCount, m_provideValueRequestQueue.Count);
                    while (m_shared.m_workersStarted < threadsToStart)
                    {
                        m_shared.m_workersStarted++;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(m_shared.Worker));
                    }
                }
            }
        }

        public int ThreadCount
        {
            set
            {
                lock (m_shared)
                {
                    m_shared.ThreadCount = value;
                }
            }
            get
            {
                return m_shared.ThreadCount;
            }
        }

        public void ClearLoadQueue()
        {
            lock (this)
            {
                m_provideValueRequestQueue.Clear();
            }
        }

        public Value DefaultValueWhileLoading
        {
            set
            {
                lock (m_shared)
                {
                    m_shared.m_defaultValueWhileLoading = value;
                }
            }
            get
            {
                lock (m_shared)
                {
                    return m_shared.m_defaultValueWhileLoading;
                }
            }

        }

        #region IDisposable Members

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
                lock (this)
                {
                    m_provideValueRequestQueue = null;
                }
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
            disposed = true;
          }
        }

        // Use C# destructor syntax for finalization code.
        ~LruCacheBackground()
        {
          // Simply call Dispose(false).
          Dispose(false);
        }    
    

        #endregion

        public LruCacheBackground<Key, Value> CreateShared()
        {
            return new LruCacheBackground<Key, Value>(this);
        }

        public void Clear()
        {
            m_shared.m_cache.Clear();
        }
    }
}
