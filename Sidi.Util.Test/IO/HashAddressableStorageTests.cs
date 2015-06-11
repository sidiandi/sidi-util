using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.IO;
using NUnit.Framework;
using Sidi.Test;
using Sidi.Util;
using System.IO;
using Sidi.Extensions;

namespace Sidi.IO.Tests
{
    [TestFixture()]
    public class HashAddressableStorageTests : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test()]
        public void ReadWrite()
        {
            var root = TestFile("HashAddressableStorage");
            root.EnsureNotExists();
            var s = new HashAddressableStorage(root);
            TestIHashAddressableStorage(s);
        }

        public static void TestIHashAddressableStorage(IHashAddressableStorage s)
        {
            var hp = HashProvider.GetDefault();
            var key = hp.GetObjectHash("hello");

            Assert.IsFalse(s.Contains(key));

            using (var w = s.Write(key))
            {
                using (var tw = new StreamWriter(w))
                {
                    tw.Write("world");
                }
            }

            Assert.IsTrue(s.Contains(key));

            using (var r = s.Read(key))
            {
                using (var tr = new StreamReader(r))
                {
                    var text = tr.ReadToEnd();
                    Assert.AreEqual("world", text);
                }
            }

            StorageItemInfo info;
            Assert.IsTrue(s.TryGetInfo(key, out info));
            log.Info(() => info);

            s.Remove(key);

            Assert.IsFalse(s.Contains(key));

            using (var w = s.Write(key))
            {
                using (var tw = new StreamWriter(w))
                {
                    tw.Write("other");
                }
            }

            Assert.IsTrue(s.Contains(key));

            using (var r = s.Read(key))
            {
                using (var tr = new StreamReader(r))
                {
                    var text = tr.ReadToEnd();
                    Assert.AreEqual("other", text);
                }
            }

            s.Clear();

            Assert.IsFalse(s.Contains(key));

        }
    }
}
