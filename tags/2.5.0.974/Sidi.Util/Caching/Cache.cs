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

        LPath storeDirectory;

        public T GetCached<T>(object key, Func<T> provider)
        {
            return GetCached(key, provider,
                path =>
                {
                    if (path.Exists && path.Info.Length == 0)
                    {
                        return null;
                    }

                    var b = new BinaryFormatter();
                    using (var stream = LFile.OpenRead(path))
                    {
                        var cacheContent = b.Deserialize(stream);
                        return cacheContent;
                    }
                },
                (path, t) =>
                {
                    path.EnsureParentDirectoryExists();
                    using (var stream = LFile.OpenWrite(path))
                    {
                        if (t != null)
                        {
                            var b = new BinaryFormatter();
                            b.Serialize(stream, t);
                        }
                    }
                });
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

        public T GetCached<T>(object key, Func<T> provider, Func<LPath, object> reader, Action<LPath, object> writer)
        {
            var p = CachePath(key);
            if (p.IsFile && IsValid(p, key))
            {
                log.InfoFormat("Cache hit: {0} = {1}", key, p);
                object cachedValue = null;
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
                    throw (Exception)cachedValue;
                }

                return (T)cachedValue;
            }

            {
                try
                {
                    var result = provider();
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
            return GetCached(key, provider,
                (cachePath) => cachePath,
                (cachePath, path) => LFile.CopyOrHardLink((LPath)path, cachePath));
        }

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

        static SHA1 sha = new SHA1CryptoServiceProvider();

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
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
