using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.IO;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Sidi.Util
{
    /// <summary>
    /// Wrapper for a byte[] hash that was created by a cryptographic hash function
    /// </summary>
    [Serializable]
    public class Hash
    {
        public IReadOnlyCollection<byte> Value { get { return Array.AsReadOnly(hash);  } }
        readonly byte[] hash;

        public Hash(byte[] hash)
        {
            this.hash = hash;
        }

        public override bool Equals(object obj)
        {
            var r = obj as Hash;
            if (r == null)
            {
                return false;
            }

            return hash.SequenceEqual(r.hash);
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(hash, 0);
        }

        public override string ToString()
        {
            return hash.HexString();
        }
    }

    public interface IHashProvider
    {
        Hash Get(LPath file);
        Hash Get(Stream stream);
    }

    public static class IHashProviderExtension
    {
        public static Hash GetObjectHash(this IHashProvider hashProvider, object x)
        {
            using (var m = new MemoryStream())
            {
                if (x != null)
                {
                    var ser = new BinaryFormatter();
                    ser.Serialize(m, x);
                }
                m.Seek(0, SeekOrigin.Begin);
                return hashProvider.Get(m);
            }
        }
    }

    public class HashProvider : IHashProvider
    {
        HashAlgorithm algorithm;
        
        public HashProvider(HashAlgorithm algorithm = null)
        {
            if (algorithm == null)
            {
                algorithm = new SHA1CryptoServiceProvider();
            }
            this.algorithm = algorithm;
        }

        public Hash Get(LPath file)
        {
            return file.Read(Get);
        }

        public Hash Get(Stream stream)
        {
            return new Hash(algorithm.ComputeHash(stream));
        }

        /// <summary>
        /// Default hash provider that uses SHA1 to calculate hashes
        /// </summary>
        /// <returns></returns>
        public static IHashProvider GetDefault()
        {
            return new HashProvider(new SHA1CryptoServiceProvider());
        }
    }

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
