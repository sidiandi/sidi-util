using Sidi.IO;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Caching
{
    /// <summary>
    /// Pre-computes values and stores them as flat files. The computation results will be persistent between runs 
    /// of the program.
    /// </summary>
    public class CacheBase : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly IHashAddressableStorage store;
        readonly IHashProvider hashProvider;

        public CacheBase(IHashAddressableStorage store, IHashProvider hashProvider)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (hashProvider == null)
            {
                throw new ArgumentNullException("hashProvider");
            }

            this.store = store;
            this.hashProvider = hashProvider;
            this.MaxAge = TimeSpan.MaxValue;
            this.RememberExceptions = false;
        }

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
        public static void SerializeToStream(Stream stream, object t)
        {
            if (object.Equals(t, default(object)))
            {
            }
            else
            {
                var b = new BinaryFormatter();
                b.Serialize(stream, t);
            }
        }

        /// <summary>
        /// Use binary serialization to read an object from a file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static object DeSerializeFromStream(Stream stream)
        {
            var b = new BinaryFormatter();
            var cacheContent = b.Deserialize(stream);
            return cacheContent;
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
        public bool IsValid(StorageItemInfo storageItemInfo, object key)
        {
            if (!Valid(storageItemInfo, key))
            {
                return false;
            }

            return (DateTime.UtcNow - storageItemInfo.LastWriteTimeUtc) < MaxAge;
        }

        public static Func<StorageItemInfo, object, bool> Valid = (p, k) => true;

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
            return Cache.Local(calculation.Method).GetCached(new object[] { input1, input2 }, _ => calculation(input1, input2));
        }

        /// <summary>
        /// Caches a read file operation
        /// </summary>
        /// Uses a local cache with an ID derived from readFileOperation.
        /// <typeparam name="TOutput">Result of the read file operation</typeparam>
        /// <param name="file">Path to the file or directory to be read</param>
        /// <param name="readFileOperation">Function that reads a file/directory and returns an TOutput value</param>
        /// <returns>Result of the readFileOperation function call.</returns>
        public static TOutput ReadFile<TOutput>(LPath file, Func<LPath, TOutput> readFileOperation)
        {
            return Cache.Local(readFileOperation.Method).Read(file, readFileOperation);
        }

        public TResult GetCached<TKey, TResult>(TKey key, Func<TKey, TResult> calculation)
        {
            return GetCached(key, calculation, DeSerializeFromStream, SerializeToStream);
        }

        public TResult GetCached<TKey, TResult>(TKey key, Func<TKey, TResult> provider, Func<Stream, object> reader, Action<Stream, object> writer)
        {
            var hash = GetHash(key);
            StorageItemInfo info;

            if (store.TryGetInfo(hash, out info) && IsValid(info, key))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Cache hit: {0} was cached under {1} at {2}", key, hash, info.LastWriteTimeUtc);
                }

                object cachedValue = default(TResult);
                try
                {
                    using (var readStream = store.Read(hash))
                    {
                        cachedValue = reader(readStream);
                    }
                }
                catch (Exception ex)
                {
                    log.Warn(key, ex);
                }

                if (cachedValue is Exception && RememberExceptions)
                {
                    throw (Exception)cachedValue;
                }

                return (TResult)cachedValue;
            }

            {
                try
                {
                    var result = provider(key);
                    using (var writeStream = store.Write(hash))
                    {
                        writer(writeStream, result);
                    }
                    return result;
                }
                catch (Exception e)
                {
                    if (RememberExceptions)
                    {
                        using (var writeStream = store.Write(hash))
                        {
                            writer(writeStream, e);
                        }
                    }
                    throw;
                }
            }
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
            store.Clear();
        }

        public void Clear(object id)
        {
            log.DebugFormat("Clear {0} from cache", id);
            store.Remove(GetHash(id));
        }

        public bool IsCached(object key)
        {
            return store.Contains(GetHash(key));
        }

        Hash GetHash(object x)
        {
            if (x is MethodBase)
            {
                return GetHash(x.ToString());
            }
            else
            {
                return hashProvider.GetObjectHash(x);
            }
        }

        public TimeSpan MaxAge { set; get; }
        public bool RememberExceptions { set; get; }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeHelper.Dispose(store);
            }
        }
    }
}
