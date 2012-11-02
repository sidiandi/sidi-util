using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;
using Sidi.Util;
using Sidi.Extensions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Sidi.Collections
{
    public class Cache
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Cache(string storeDirectory)
        {
            this.storeDirectory = storeDirectory;
            this.MaxAge = TimeSpan.MaxValue;
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
                        var cacheContent= b.Deserialize(stream);
                        if (cacheContent is Exception)
                        {
                            throw (Exception) cacheContent;
                        }
                        else
                        {
                            return (T)cacheContent;
                        }
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

        public T GetCached<T>(object key, Func<T> provider, Func<LPath, object> reader, Action<LPath, object> writer)
        {
            var p = CachePath(key);
            if (p.Exists && (DateTime.UtcNow - p.Info.LastWriteTimeUtc) < MaxAge)
            {
                log.InfoFormat("Cache hit: {0} = {1}", key, p);
                var cachedValue = reader(p);
                if (cachedValue is Exception)
                {
                    throw (Exception)cachedValue;
                }
                else
                {
                    return (T)cachedValue;
                }
            }
            else
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
                    writer(p, e);
                    throw;
                }
            }
        }

        public void Clear()
        {
            storeDirectory.EnsureNotExists();
        }

        public bool IsCached(object key)
        {
            return CachePath(key).Exists;
        }

        LPath CachePath(object key)
        {
            return storeDirectory.CatDir(Digest(key));
        }

        static string Digest(object x)
        {
            if (x is MethodBase)
            {
                return Digest(x.ToString());
            }
            else
            {
                return String.Format("{0:x}", x.GetHashCode());
            }
        }

        public static Cache Local(object id)
        {
            return new Cache(typeof(Cache).UserSetting("cache").CatDir(Digest(id)));
        }

        public TimeSpan MaxAge { set; get; }


    }
}
