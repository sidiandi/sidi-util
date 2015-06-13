using Sidi.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Util
{
    /// <summary>
    /// Caches the hashes of files by (path, length, modified date)
    /// </summary>
    public class CachedHashProvider : IHashProvider
    {
        public CachedHashProvider(IHashProvider hashProvider)
        {
            this.hashProvider = hashProvider;
        }
        IHashProvider hashProvider;

        /// <summary>
        /// Calculates the hash value of file contents. Returns a cached value if the tuple (path, length, modified date) of the file were not changed.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Hash value of file contents</returns>
        public Hash Get(LPath file)
        {
            return Sidi.Caching.Cache.ReadFile(file, hashProvider.Get);
        }

        public Hash Get(Stream stream)
        {
            return hashProvider.Get(stream);
        }

        public static IHashProvider GetDefault()
        {
            return new CachedHashProvider(HashProvider.GetDefault());
        }
    }
}
