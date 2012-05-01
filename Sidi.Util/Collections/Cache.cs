﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;
using Sidi.Util;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Sidi.IO.Long;
using Sidi.IO.Long.Extensions;

namespace Sidi.Collections
{
    public class Cache
    {
        public Cache(string storeDirectory)
        {
            this.storeDirectory = storeDirectory.Long();
        }

        Path storeDirectory;

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
                    using (var stream = File.OpenRead(path))
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
                    using (var stream = File.OpenWrite(path))
                    {
                        if (t != null)
                        {
                            var b = new BinaryFormatter();
                            b.Serialize(stream, t);
                        }
                    }
                });
        }

        public T GetCached<T>(object key, Func<T> provider, Func<Path, object> reader, Action<Path, object> writer)
        {
            var p = CachePath(key);
            if (p.Exists)
            {
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
                    throw e;
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

        Path CachePath(object key)
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

        static Cache s_Local;
        public static Cache Local(object id)
        {
            return new Cache(typeof(Cache).UserSetting("cache").CatDir(Digest(id)));
        }
    }
}
