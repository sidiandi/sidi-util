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
using System.Diagnostics;

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
            var hp = ObjectHashProvider.GetDefault();
            var key = hp.Get("hello");

            Assert.IsFalse(s.ContainsKey(key));

            using (var w = s.Write(key))
            {
                using (var tw = new StreamWriter(w))
                {
                    tw.Write("world");
                }
            }

            Assert.IsTrue(s.ContainsKey(key));

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

            Assert.IsFalse(s.ContainsKey(key));

            using (var w = s.Write(key))
            {
                using (var tw = new StreamWriter(w))
                {
                    tw.Write("other");
                }
            }

            Assert.IsTrue(s.ContainsKey(key));

            using (var r = s.Read(key))
            {
                using (var tr = new StreamReader(r))
                {
                    var text = tr.ReadToEnd();
                    Assert.AreEqual("other", text);
                }
            }

            s.Clear();

            Assert.IsFalse(s.ContainsKey(key));

        }
        
        public static void Performance(IHashAddressableStorage s)
        {
            var keys = Enumerable.Range(0, 1000)
                .Select(x => ObjectHashProvider.GetDefault().Get(x))
                .ToList();

            var data = ASCIIEncoding.ASCII.GetBytes("Hello, world.");

            var stopWatch = Stopwatch.StartNew();
            {
                foreach (var i in keys)
                {
                    using (var w = s.Write(i))
                    {
                        w.Write(data, 0, data.Length);
                    }
                }
            }
            log.InfoFormat("Write: {0}", stopWatch.ElapsedMilliseconds);

            stopWatch = Stopwatch.StartNew();
            {
                foreach (var i in keys)
                {
                    using (var w = s.Read(i))
                    {
                        byte[] buf = new byte[255];
                        w.Read(buf, 0, buf.Length);
                    }
                }
            }
            log.InfoFormat("Read: {0}", stopWatch.ElapsedMilliseconds);

            stopWatch = Stopwatch.StartNew();
            {
                foreach (var i in keys)
                {
                    StorageItemInfo info;
                    Assert.IsTrue(s.TryGetInfo(i, out info));
                }
            }
            log.InfoFormat("Info: {0}", stopWatch.ElapsedMilliseconds);
        }
    }
}
