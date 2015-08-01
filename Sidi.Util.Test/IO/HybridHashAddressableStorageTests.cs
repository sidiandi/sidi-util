using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.IO;
using NUnit.Framework;
using Sidi.Test;
using Sidi.Extensions;
using Sidi.Util;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Sidi.IO.Tests
{
    [TestFixture()]
    public class HybridHashAddressableStorageTests : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test()]
        public void ReadWrite()
        {
            var root = TestFile("HybridHashAddressableStorage");
            root.EnsureNotExists();
            using (var s = new HybridHashAddressableStorage(root))
            {
                HashAddressableStorageTests.TestIHashAddressableStorage(s);

                var bigDataKey = ObjectHashProvider.GetDefault().Get("big_data");
                var bigData = new byte[s.MaxInternalBlobSize * 2];
                new Random().NextBytes(bigData);

                for (int i = 0; i < 2; ++i)
                {
                    using (var w = s.Write(bigDataKey))
                    {
                        w.Write(bigData);
                        log.Info(() => Find.AllFiles(root));
                    }
                }

                StorageItemInfo info;
                Assert.IsTrue(s.TryGetInfo(bigDataKey, out info));
                Assert.AreEqual(bigData.Length, info.Length);

                log.Info(() => Find.AllFiles(root));

                using (var r = s.Read(bigDataKey))
                {
                    var data = r.ReadToEnd();
                    Assert.IsTrue(bigData.SequenceEqual(data));
                }

                s.Clear();
            }

            Assert.AreEqual(1, Find.AllFiles(root).Count());
        }

        [Test]
        public void Performance()
        {
            var root = TestFile("HybridHashAddressableStorageTests");
            log.Info(() => root);
            root.EnsureNotExists();
            using (var s = new HybridHashAddressableStorage(root))
            {
                HashAddressableStorageTests.Performance(s);
            }
        }

        [Test]
        public void PerformanceFlushAfter100()
        {
            var root = TestFile("HybridHashAddressableStorageTests");
            log.Info(() => root);
            root.EnsureNotExists();
            using (var s = new HybridHashAddressableStorage(root) { FlushAfterNWrites = 100 })
            {
                HashAddressableStorageTests.Performance(s);
            }
        }

        [Test]
        public void extensions()
        {
            var root = TestFile("HybridHashAddressableStorageTests");
            log.Info(() => root);
            root.EnsureNotExists();
            var hp = HashProvider.GetDefault();
            var key = "hello";
            var value = "world";

            var ohp = ObjectHashProvider.GetDefault();

            using (var a = new HybridHashAddressableStorage(root))
            {
                a.Write(key, ohp, value, (w, v) => new BinaryFormatter().Serialize(w, v));
                Assert.AreEqual(value, a.Read(key, ohp, r => (string) new BinaryFormatter().Deserialize(r)));
            }
        }

        [Test]
        public void do_not_lock_files()
        {
            var root = TestFile("HybridHashAddressableStorageTests");
            log.Info(() => root);
            root.EnsureNotExists();
            var hp = HashProvider.GetDefault();
            var key = "hello";
            var value = "world";

            var ohp = ObjectHashProvider.GetDefault();

            using (var a = new HybridHashAddressableStorage(root))
            {
                using (var b = new HybridHashAddressableStorage(root))
                {
                    a.Write(key, ohp, value, BinaryFormatterHelper.Serialize);
                    Assert.AreEqual(value, b.Read(key, ohp, BinaryFormatterHelper.Deserialize<string>));
                }
            }
        }
    }
}
