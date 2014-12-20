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

namespace Sidi.Caching
{
    /// <summary>
    /// Pre-computes values and stores them as flat files. The computation results will be persistent between runs 
    /// of the program.
    /// </summary>
    public class Cache
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Cache(LPath storeDirectory)
        {
            log.Debug(storeDirectory);
            this.storeDirectory = storeDirectory;
            this.MaxAge = TimeSpan.MaxValue;
            this.RememberExceptions = false;
        }

        readonly LPath storeDirectory;

        /// <summary>
        /// Use binary serialization to read an object from a file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static object DeSerializeFromFile(LPath path)
        {
            if (path.Exists && path.Info.Length == 0)
            {
                return null;
            }

            var b = new BinaryFormatter();
            using (var stream = path.OpenRead())
            {
                var cacheContent = b.Deserialize(stream);
                return cacheContent;
            }
        }

        /// <summary>
        /// Use binary serialization to write an object to a file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="t"></param>
        public static void SerializeToFile(LPath path, object t)
        {
            path.EnsureParentDirectoryExists();
            using (var stream = path.OpenWrite())
            {
                if (object.Equals(t, default(object)))
                {
                    path.EnsureFileNotExists();
                }
                else
                {
                    var b = new BinaryFormatter();
                    b.Serialize(stream, t);
                }
            }
        }

        /// <summary>
        /// Returns if the cacheEntry is valid for key
        /// </summary>
        /// <param name="cacheEntry"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsValid(LPath cacheEntry, object key)
        {
            if (!Valid(cacheEntry, key))
            {
                return false;
            }

            return (DateTime.UtcNow - cacheEntry.Info.LastWriteTimeUtc) < MaxAge;
        }

        public static Func<LPath, object, bool> Valid = (p, k) => true;

        /// <summary>
        /// Uses a local cache with an ID derived from calculation
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="input"></param>
        /// <param name="calculation"></param>
        /// <returns></returns>
        public static TOutput Get<TInput, TOutput>(TInput input, Func<TInput, TOutput> calculation)
        {
            return Cache.Local(calculation.Method).GetCached(input, calculation);
        }

        /// <summary>
        /// Uses a local cache with an ID derived from calculation
        /// </summary>
        /// <typeparam name="TInput1"></typeparam>
        /// <typeparam name="TInput2"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <param name="calculation"></param>
        /// <returns></returns>
        public static TOutput Get<TInput1, TInput2, TOutput>(TInput1 input1, TInput2 input2, Func<TInput1, TInput2, TOutput> calculation)
        {
            return Cache.Local(calculation.Method).GetCached(new object[]{input1, input2}, _ => calculation(input1, input2));
        }

        /// <summary>
        /// Uses a local cache with an ID derived from calculation
        /// </summary>
        /// <typeparam name="TInput1"></typeparam>
        /// <typeparam name="TInput2"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <param name="calculation"></param>
        /// <returns></returns>
        public static TOutput ReadFile<TOutput>(LPath file, Func<LPath, TOutput> readFileOperation)
        {
            return Cache.Local(readFileOperation.Method).Read(file, readFileOperation);
        }

        public TResult GetCached<TKey, TResult>(TKey key, Func<TKey, TResult> calculation)
        {
            return GetCached(key, calculation, DeSerializeFromFile, SerializeToFile);
        }

        public TResult GetCached<TKey, TResult>(TKey key, Func<TKey, TResult> provider, Func<LPath, object> reader, Action<LPath, object> writer)
        {
            var p = CachePath(key);
            if (p.IsFile && IsValid(p, key))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Cache hit: {0} was cached in {1} at {2}", key, p, p.Info.LastWriteTimeUtc);
                }

                object cachedValue = default(TResult);
                try
                {
                    cachedValue = reader(p);
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
                    p.EnsureParentDirectoryExists();
                    writer(p, result);
                    return result;
                }
                catch (Exception e)
                {
                    if (RememberExceptions)
                    {
                        writer(p, e);
                    }
                    throw;
                }
            }
        }

        public LPath GetCachedFile(object key, Func<LPath> provider)
        {
            return GetCached(key, _ => provider(),
                (cachePath) => cachePath,
                (cachePath, path) => ((LPath)path).CopyOrHardLink(cachePath));
        }

        /// <summary>
        /// Caches a file read. Useful when the parsing of file contents is expensive.
        /// </summary>
        /// <typeparam name="T">File content type</typeparam>
        /// <param name="path">File path</param>
        /// <param name="fileReader">Function to read a file and return its contents as type T</param>
        /// <returns></returns>
        public T Read<T>(LPath path, Func<LPath, T> fileReader)
        {
            return GetCached(new FileVersion(path), _ => fileReader(_.Path));
        }

        /// <summary>
        /// Clears all cached values 
        /// </summary>
        public void Clear()
        {
            storeDirectory.EnsureNotExists();
        }

        public void Clear(object id)
        {
            log.DebugFormat("Clear {0} from cache", id);
            CachePath(id).EnsureNotExists();
        }

        public bool IsCached(object key)
        {
            return CachePath(key).Exists;
        }

        LPath CachePath(object key)
        {
            return storeDirectory.CatDir(Digest(key));
        }

        readonly static SHA1 sha = new SHA1CryptoServiceProvider();

        public static string Digest(object x)
        {
            if (x is MethodBase)
            {
                return Digest(x.ToString());
            }
            else
            {
                var b = new BinaryFormatter();
                var s = new MemoryStream();
                b.Serialize(s, x);
                var hash = sha.ComputeHash(s.ToArray());
                return LPath.GetValidFilename(Base32.Encode(hash));
            }
        }

        /// <summary>
        /// Returns a default instance, which stores values in local AppData
        /// Typical use:
        /// var cache = Cache.Local(MethodBase.GetCurrentMethod());
        /// </summary>
        /// <param name="id">Identifier of the cache</param>
        /// <returns>A cache object specific to id</returns>
        public static Cache Local(object id)
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

            return new Cache(
                Paths.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                .CatDir(Paths.Get(type), "cache", Digest(id)));
        }

        public TimeSpan MaxAge { set; get; }
        public bool RememberExceptions { set; get; }
    }
}
