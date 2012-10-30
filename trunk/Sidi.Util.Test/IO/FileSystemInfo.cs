using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Util;
using Sidi.Extensions;

namespace Sidi.IO
{
    [TestFixture]
    public class FileSystemInfoTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        [Test]
        public void DriveRoot()
        {
            var drive = Directory.Current.PathRoot;
            Assert.IsTrue(drive.IsRoot);
            var c = new FileSystemInfo(drive);
            Assert.IsTrue(c.Exists);
            c.DumpProperties(Console.Out);
            log.Info(c.GetFileSystemInfos().Join());
        }

        [Test]
        public void DotHandling()
        {
            var dot = new Path(".");
            Assert.IsTrue(dot.Info.Exists);
        }

        [Test]
        public void Fullname()
        {
            var currentDirectory = new Path(System.Environment.CurrentDirectory);
            var dot = new Path(".");
            dot.Info.DumpProperties(Console.Out);
            Assert.AreEqual(currentDirectory, dot.Info.FullName);
        }
    }
}
