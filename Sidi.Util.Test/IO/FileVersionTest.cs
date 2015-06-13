using NUnit.Framework;
using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    [TestFixture]
    public class FileVersionTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Equal()
        {
            var f0 = TestFile("hello");
            f0.WriteAllText("hello");

            var v0 = FileVersion.Get(f0);
            var d0 = FileVersion.Get(f0.Parent);
            var v1 = FileVersion.Get(f0);
            var d1 = FileVersion.Get(f0.Parent);
            Assert.That(v0, Is.EqualTo(v1));
            Assert.That(d0, Is.EqualTo(d1));
            log.Info(d1);    

            f0.WriteAllText("hello");
            v1 = FileVersion.Get(f0);
            d1 = FileVersion.Get(f0.Parent);
            Assert.That(v0, Is.Not.EqualTo(v1));
            Assert.That(d0, Is.Not.EqualTo(d1));
            log.Info(d1);
        }
    }
}
