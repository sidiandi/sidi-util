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
using System.Threading;

namespace Sidi.Collections
{

    public class LruCacheBackground<Key, Value> : IDisposable
    {
        public class EntryUpdatedEventArgs : EventArgs
        {
            public Key key;
        }

        public delegate void EntryUpdatedHandler(object sender, EntryUpdatedEventArgs arg);
        
        /// <summary>
        /// Fires when an entry in the cache was updated.
        /// </summary>
        /// Warning: this event will be fired by a background thread.
        public event EntryUpdatedHandler EntryUpdated;

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
            public LruCache<Key, CacheEntry> m_cache;
            public Value m_defaultValueWhileLoading = default(Value);
            public bool m_workerStarted = false;
            public List<LruCacheBackground<Key, Value>> m_instances = new List<LruCacheBackground<Key, Value>>();

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
                                    catch (Exception)
                                    {
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
                    m_workerStarted = false;
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

        ~LruCacheBackground()
        {
            lock (this)
            {
                m_provideValueRequestQueue = null;
            }
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
                return m_shared.m_cache.IsCached(key);
            }
        }

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

                if (m_shared.m_workerStarted == false)
                {
                    m_shared.m_workerStarted = true;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(m_shared.Worker));
                }
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

        public void Dispose()
        {
            lock (this)
            {
                m_provideValueRequestQueue = null;
            }
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
