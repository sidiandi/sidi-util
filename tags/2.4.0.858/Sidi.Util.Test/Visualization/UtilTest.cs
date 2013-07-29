using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Test;
using Sidi.Visualization;

namespace Sidi.Visualization
{
    [TestFixture]
    public class UtilTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Split()
        {
            {
                var p = new[] { 1.0, 1.0 }.Split(x => x);
                Assert.AreEqual(1, p[0].Items.Length);
                Assert.AreEqual(1, p[0].Sum);
                Assert.AreEqual(1, p[1].Sum);
            }

            {
                var s = Enumerable.Range(0, 100);
                var p = s.Split(x => (double)x);
                Assert.AreEqual(71, p[0].Items.Length);
                Assert.AreEqual(2485, p[0].Sum);
                Assert.AreEqual(2465, p[1].Sum);
            }

            {
                var s = new[] { 1.0, 0.0, 0.0, 0.0, 0.0 };
                Assert.AreEqual(1, s.Split(x => x)[0].Items.Length);
            }

            {
                var s = new[] { 0.1, 0.1, 0.1, 0.1, 1.0 };
                Assert.AreEqual(4, s.Split(x => x)[0].Items.Length);
            }
        }
    }
}
