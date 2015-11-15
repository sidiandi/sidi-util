using NUnit.Framework;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace Sidi.Util.Tests
{
    [TestFixture()]
    public class HashStreamTests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test()]
        public void GetHashTest()
        {
            Func<HashAlgorithm> hashProvider = SHA1CryptoServiceProvider.Create;

            var emptyHash = HashProvider.GetDefault().Get(new MemoryStream());

            {
                using (var s = new HashStream(hashProvider()))
                {
                    Assert.AreEqual(emptyHash, s.GetHash());
                }
            }

            {
                using (var s = new HashStream(hashProvider()))
                {
                    s.Write(ASCIIEncoding.ASCII.GetBytes("Hello, world"));
                    log.Info(() => s.GetHash());
                }
            }
        }
    }
}