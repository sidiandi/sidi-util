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
            var sqliteDatabase = TestFile("SQLiteHashAddressableStorage.sqlite");
            log.Info(() => sqliteDatabase);
            sqliteDatabase.EnsureFileNotExists();
            using (var s = new SQLiteHashAddressableStorage(sqliteDatabase))
            {
                HashAddressableStorageTests.Performance(s);
            }
        }
    }
}
