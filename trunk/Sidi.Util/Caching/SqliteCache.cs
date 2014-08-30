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
using System.Linq;
using System.Text;
using Sidi.IO;
using Sidi.Util;
using Sidi.Extensions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Security.Cryptography;
using Sidi.Persistence;
using System.Data.SQLite;

namespace Sidi.Caching
{
    /// <summary>
    /// Pre-computes values and stores them as flat files. The computation results will be persistent between runs 
    /// of the program.
    /// </summary>
    public class SqliteCache : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        SqliteCache(LPath dbPath)
        {
            log.Debug(dbPath);
            this.dbPath = dbPath;
            db = new Sidi.Persistence.Collection<Entry>(dbPath, "Cache");
            this.MaxAge = TimeSpan.MaxValue;
            this.RememberExceptions = false;
        }

        LPath dbPath;
        SQLiteTransaction transaction;
        readonly TimeSpan commitInterval = TimeSpan.FromMinutes(1);
        DateTime nextCommit = DateTime.Now;

        /// <summary>
        /// Use binary serialization to read an object from a file
        /// </summary>
        /// <param name="data">Binary data to deserialize from</param>
        /// <returns>The deserialized object</returns>
        public static object DeSerializeFromData(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var b = new BinaryFormatter();
                var cacheContent = b.Deserialize(stream);
                return cacheContent;
            }
        }

        /// <summary>
        /// Use binary serialization to write an object to a file
        /// </summary>
        /// <param name="t"></param>
        public static byte[] SerializeToData(object t)
        {
            using (var stream = new MemoryStream())
            {
                if (object.Equals(t, default(object)))
                {
                }
                else
                {
                    var b = new BinaryFormatter();
                    b.Serialize(stream, t);
                }
                return stream.GetBuffer();
            }
        }

        /// <summary>
        /// Returns true if there is a cached value for this key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(object key)
        {
            var e = GetEntry(GetCacheKey(key));
            return e != null && (DateTime.UtcNow - e.Time) < MaxAge;
        }

        public static Func<LPath, object, bool> Valid = (p, k) => true;

        public TResult GetCached<TKey, TResult>(TKey key, Func<TKey, TResult> provider)
        {
            var cacheKey = GetCacheKey(key);
            Entry entry = GetEntry(cacheKey);
            if (entry != null)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Cache hit: {0} was cached in {1} at {2}", key, cacheKey, entry.Time);
                }

                object cachedValue = default(TResult);
                try
                {
                    cachedValue = DeSerializeFromData(entry.Data);
                }
                catch (Exception ex)
                {
                    log.Warn(key, ex);
                }

                if (cachedValue is Exception && RememberExceptions)
                {
                    throw (Exception) cachedValue;
                }

                return (TResult)cachedValue;
            }

            {
                try
                {
                    var result = provider(key);
                    Add(cacheKey, result);
                    return result;
                }
                catch (Exception e)
                {
                    if (RememberExceptions)
                    {
                        Add(cacheKey, e);
                    }
                    throw;
                }
            }
        }

        void Add(string cacheKey, object result)
        {
            log.DebugFormat("add: {0}", cacheKey);
            db.Add(new Entry
            {
                Key = cacheKey,
                Time = DateTime.Now,
                Data = SerializeToData(result)
            });
            CommitIfNecessary();
        }

        /// <summary>
        /// Caches a file read. Useful when the parsing of file contents is expensive.
        /// </summary>
        /// <typeparam name="T">File content type</typeparam>
        /// <param name="path">File path</param>
        /// <param name="fileReader">Function to read a file and return its contents as type T</param>
        /// <returns></returns>
        public T ReadFile<T>(LPath path, Func<LPath, T> fileReader)
        {
            return GetCached(new LPathWriteTime(path), _ => fileReader(_.Path));
        }

        /// <summary>
        /// Clears all cached values 
        /// </summary>
        public void Clear()
        {
            log.DebugFormat("Clear cache {0}", dbPath);
            db.Dispose();
            dbPath.EnsureFileNotExists();
            db = new Sidi.Persistence.Collection<Entry>(dbPath, "Cache");
            transaction = db.BeginTransaction();
            nextCommit = DateTime.Now.AddSeconds(60);
        }

        void CommitIfNecessary()
        {
            var n = DateTime.Now;
            if (nextCommit < n)
            {
                if (transaction != null)
                {
                    transaction.Commit();
                }
                transaction = db.BeginTransaction();
                nextCommit = n + commitInterval;
            }
        }

        public void Reset(object id)
        {
            log.DebugFormat("Clear {0} from cache", id);
            var entry = GetEntry(GetCacheKey(id));
            if (entry != null)
            {
                db.Remove(entry);
            }
        }

        class Entry
        {
            [RowId]
            public long Id = 0;
            [Data, Indexed]
            public string Key;
            [Data]
            public DateTime Time;
            [Data]
            public byte[] Data;
        }

        Sidi.Persistence.Collection<Entry> db;

        public bool IsCached(object key)
        {
            return GetEntry(GetCacheKey(key)) != null;
        }

        Entry GetEntry(string key)
        {
            return db.Find("Key = {0}".F(key.Quote()));
        }

        string GetCacheKey(object key)
        {
            return Cache.Digest(key);
        }

        readonly static SHA1 sha = new SHA1CryptoServiceProvider();

        /// <summary>
        /// Returns a default instance, which stores values in local AppData
        /// Typical use:
        /// var cache = Cache.Local(MethodBase.GetCurrentMethod());
        /// </summary>
        /// <param name="id">Identifier of the cache</param>
        /// <returns>A cache object specific to id</returns>
        public static SqliteCache Local(object id)
        {
            lock (s_localInstances)
            {
                SqliteCache i;
                if (!s_localInstances.TryGetValue(id, out i))
                {
                    i = CreateLocal(id);
                    s_localInstances[id] = i;
                }
                return i;
            }
        }

        static SqliteCache CreateLocal(object id)
        {
            Type type = null;
            if (id is MethodBase)
            {
                type = ((MethodBase)id).DeclaringType;
            }
            else if (id is Type)
            {
                type = (Type)id;
            }
            else
            {
                type = id.GetType();
            }

            return new SqliteCache(
                Paths.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                .CatDir(Paths.Get(type), "cache", Cache.Digest(id) + ".sqlite"));
        }

        static System.Collections.Generic.Dictionary<object, SqliteCache> s_localInstances = new System.Collections.Generic.Dictionary<object, SqliteCache>();

        public TimeSpan MaxAge { set; get; }
        public bool RememberExceptions { set; get; }
    
        public void Dispose()
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
            }
            db.Dispose();
        }

        public static R Get<X, R>(X x, Func<X, R> uncachedFunction)
        {
            return SqliteCache.Local(uncachedFunction.Method).GetCached(x, uncachedFunction);
        }

        public static R Get<X, Y, R>(X x, Y y, Func<X, Y, R> uncachedFunction)
        {
            return SqliteCache.Local(uncachedFunction.Method).GetCached(new Tuple<X, Y>(x, y), _ => uncachedFunction(_.Item1, _.Item2)); ;
        }
    }
}
