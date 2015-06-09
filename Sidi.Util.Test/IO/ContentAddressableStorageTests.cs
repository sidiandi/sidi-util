using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Util;
using NUnit.Framework;
using Sidi.Test;
using System.IO;
using Sidi.Extensions;
using System.Security.Cryptography;
using Sidi.IO;

namespace Sidi.IO
{
    [TestFixture()]
    public class ContentAddressableStorageTests : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test()]
        public void ContentAddressableStorageTest()
        {
            var hashProvider = new CachedHashProvider(new HashProvider(new SHA1CryptoServiceProvider()));

            var dir = TestFile("ContentAddressableStorageTest");
            dir.EnsureNotExists();
            var s = new ContentAddressableStorage(dir, hashProvider);
            Assert.IsTrue(dir.IsDirectory);

            var emptyHash = hashProvider.Get(new MemoryStream());

            var text = "Hello, World";
            var data = ASCIIEncoding.ASCII.GetBytes(text);

            var hash = s.Write(new MemoryStream(data)).Hash;
            log.Info(() => hash);
            Assert.IsTrue(s.Contains(hash));
            
            Assert.IsFalse(s.Contains(emptyHash));
            s.Write(new MemoryStream());
            Assert.IsTrue(s.Contains(emptyHash));

            using (var reader = new StreamReader(s.Read(hash)))
            {
                Assert.AreEqual(text, reader.ReadToEnd());
            }

            log.Info(() => Find.AllFiles(dir));

        }
    }
}
