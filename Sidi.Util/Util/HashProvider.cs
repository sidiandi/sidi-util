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
        readonly Func<HashAlgorithm> algorithmProvider;

        public HashProvider(Func<HashAlgorithm> algorithmProvider)
        {
            if (algorithmProvider == null)
            {
                throw new ArgumentNullException("algorithmProvider");
            }
            this.algorithmProvider = algorithmProvider;

        }

        public Hash Get(IFileSystemInfo file)
        {
            return file.FullName.Read(Get);
        }

        public Hash Get(Stream stream)
        {
            return new Hash(algorithmProvider().ComputeHash(stream));
        }

        /// <summary>
        /// Default hash provider that uses SHA1 to calculate hashes
        /// </summary>
        /// <returns></returns>
        public static IHashProvider GetDefault()
        {
            return new HashProvider(SHA1CryptoServiceProvider.Create);
        }

        public HashStream GetStream()
        {
            return new HashStream(algorithmProvider());
        }
    }
}
