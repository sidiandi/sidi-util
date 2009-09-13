using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.IO;
using System.IO;

namespace Sidi.IO
{
    [TestFixture]
    public class PathExTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void GetRelativePath()
        {
            Assert.AreEqual("a.txt", PathEx.GetRelativePath(@"d:\temp\a.txt", @"d:\temp"));
            Assert.AreEqual("..", PathEx.GetRelativePath(@"d:\temp", @"d:\temp\a.txt"));
        }

        [Test]
        public void Find()
        {
            foreach (var i in PathEx.Find(TestFile(".")))
            {
                log.Info(i);
                Assert.IsTrue(File.Exists(i) || Directory.Exists(i));
            }
        }
    }
}
