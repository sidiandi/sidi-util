// Copyright (c) 2011, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.Linq;

namespace Sidi.Cache
{

    public class LruCacheBackground<Key, Value> : IDisposable, ICache<Key, Value>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public LruCacheBackground(int maxCount, Func<Key, Value> provideValue, int threadCount)
        : this(maxCount, threadCount)
        {
            this.provideValue = provideValue;
        }

        public LruCacheBackground(int maxCount, int threadCount)
        {
            cache = new LruCache<Key, CacheEntry>(maxCount, x =>
                {
                    return new CacheEntry(State.Missing, default(Value));
                });

            defaultValueWhileLoading = x => default(Value);

            // start worker threads
            workers = Sequence(threadCount).Select(x =>
                {
                    var t = new Thread(new ThreadStart(() => Worker()));
                    t.Start();
                    return t;
                }).ToList();
        }

        static IEnumerable<int> Sequence(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                yield return i;
            }
        }

        LruCache<Key, CacheEntry> cache;
        Func<Key, Value> provideValue;
        List<Thread> workers;

        public Func<Key, Value> ProvideValue
        {
            set
            {
                lock (this)
                {
                    Clear();
                    provideValue = value;
                }
            }
        }

        public class EntryUpdatedEventArgs : EventArgs
        {
            public EntryUpdatedEventArgs(Key key)
            {
                Key = key;
            }

            public Key Key { get; private set; } 
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
            Complete,
            Exception
        };

        class CacheEntry : IDisposable
        {
            public CacheEntry(Value value)
            {
                m_value = value;
                m_state = State.Complete;
            }

            public CacheEntry(State state, Value value)
            {
                this.m_value = value;
                this.m_state = state;
            }

            public CacheEntry(Exception ex)
            {
                m_state = State.Exception;
                exception = ex;
            }

            public CacheEntry(State state)
            {
                m_state = state;
            }

            public Value m_value;
            public State m_state;
            public Exception exception;

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

        class ProvideValueRequest
        {
            Key key;
            DateTime requestTime;

            public ProvideValueRequest(Key key)
            {
                this.key = key;
                requestTime = DateTime.Now;
            }

            public DateTime RequestTime
            {
                get { return requestTime; }
            }

            public Key Key { get { return key; } }
        }

        Stack<ProvideValueRequest> provideValueRequestQueue = new Stack<ProvideValueRequest>();

        protected virtual void OnEntryUpdated(Key key)
        {
            if (EntryUpdated != null)
            {
                EntryUpdated(this, new EntryUpdatedEventArgs(key));
            }
        }

        public void Worker()
        {
            lock (this)
            {
                while (provideValueRequestQueue != null)
                {
                    while (provideValueRequestQueue.Count > 0)
                    {
                        var k = provideValueRequestQueue.Pop();
                        var ce = cache[k.Key];
                        if (ce.m_state == State.Missing)
                        {
                            ce.m_state = State.Loading;
                            Monitor.Exit(this);
                            try
                            {
                                try
                                {
                                    var value = provideValue(k.Key);
                                    ce = new CacheEntry(value);
                                }
                                catch (Exception ex)
                                {
                                    ce = new CacheEntry(ex);
                                }

                            }
                            finally
                            {
                                Monitor.Enter(this);
                            }
                            log.InfoFormat("key={0}, queue={1}", k.Key, provideValueRequestQueue.Count);
                            cache.Update(k.Key, ce);

                            Monitor.Exit(this);
                            OnEntryUpdated(k.Key);
                            Monitor.Enter(this);
                        }
                    }

                    Monitor.Wait(this);
                }
            }
        }

        public void Reset(Key key)
        {
            lock (this)
            {
                cache.Reset(key);
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
                    var ce = cache[key];
                    if (ce.m_state == State.Missing)
                    {
                        Load(key);
                        cache.Update(key, new CacheEntry(State.Missing, defaultValueWhileLoading(key)));
                        return cache[key].m_value;
                    }
                    else
                    {
                        return ce.m_value;
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
            lock (this)
            {
                return cache.Contains(key);
            }
        }

        public void Load(Key key)
        {
            lock (this)
            {
                provideValueRequestQueue.Push(new ProvideValueRequest(key));
                Monitor.Pulse(this);
            }
        }

        public void ClearLoadQueue()
        {
            lock (this)
            {
                provideValueRequestQueue.Clear();
            }
        }

        public Func<Key, Value> DefaultValueWhileLoading
        {
            set
            {
                lock (this)
                {
                    defaultValueWhileLoading = value;
                }
            }
        }

        Func<Key, Value> defaultValueWhileLoading;

        #region IDisposable Members

        public void Dispose()
        {
            lock (this)
            {
                provideValueRequestQueue = null;
                Monitor.PulseAll(this);
            }
            workers.ForEach(x => x.Join());
            cache.Dispose();
        }

        #endregion

        public void Clear()
        {
            lock (this)
            {
                cache.Clear();
                ClearLoadQueue();
            }
        }
    }
}