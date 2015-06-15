using Sidi.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Util
{
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

        public Hash Get(IFileSystemInfo file)
        {
            return file.FullName.Read(Get);
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
}
