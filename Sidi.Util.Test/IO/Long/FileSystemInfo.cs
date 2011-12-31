using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Util;
using Sidi.Extensions;
using Sidi.IO.Long.Extensions;

namespace Sidi.IO.Long
{
    [TestFixture]
    public class FileSystemInfoTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        [Test]
        public void DriveRoot()
        {
            var c = new FileSystemInfo(@"C:".Long());
            c.DumpProperties(Console.Out);
            log.Info(c.GetFileSystemInfos().Join());
            Assert.IsTrue(c.Exists);
        }
    }
}
