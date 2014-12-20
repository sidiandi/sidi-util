using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Test;

namespace Sidi.IO
{
    [TestFixture]
    public class PathsTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Type()
        {
            var p = Paths.Get(GetType());
            log.Info(p);
        }

        [Test]
        public void GetDrives()
        {
            foreach (var drive in Paths.GetDrives())
            {
                if (new System.IO.DriveInfo(drive).DriveType != System.IO.DriveType.CDRom)
                {
                    Assert.IsTrue(drive.IsDirectory, drive.ToString());
                }
            }
        }

        [Test]
        public void FreeDrives()
        {
            foreach (var d in Paths.GetFreeDrives())
            {
                Assert.IsFalse(d.Exists);
                Assert.IsFalse(d.IsDirectory);
            }
        }
    }
}
