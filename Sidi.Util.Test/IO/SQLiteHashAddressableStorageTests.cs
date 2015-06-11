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
using System.Diagnostics;

namespace Sidi.IO.Tests
{
    [TestFixture()]
    public class SQLiteHashAddressableStorageTests : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test()]
        public void ReadWrite()
        {
            var sqliteDatabase = TestFile("SQLiteHashAddressableStorage.sqlite");
            log.Info(() => sqliteDatabase);
            sqliteDatabase.EnsureFileNotExists();
            using (var s = new SQLiteHashAddressableStorage(sqliteDatabase))
            {
                HashAddressableStorageTests.TestIHashAddressableStorage(s);
            }
        }

        [Test]
        public void Performance()
        {
            var keys = Enumerable.Range(0, 1000)
                .Select(x => HashProvider.GetDefault().GetObjectHash(x))
                .ToList();

            var data = ASCIIEncoding.ASCII.GetBytes("Hello, world.");

            var sqliteDatabase = TestFile("SQLiteHashAddressableStorage.sqlite");
            log.Info(() => sqliteDatabase);
            sqliteDatabase.EnsureFileNotExists();

            var stopWatch = Stopwatch.StartNew();
            using (var s = new SQLiteHashAddressableStorage(sqliteDatabase))
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
            using (var s = new SQLiteHashAddressableStorage(sqliteDatabase))
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
            using (var s = new SQLiteHashAddressableStorage(sqliteDatabase))
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
