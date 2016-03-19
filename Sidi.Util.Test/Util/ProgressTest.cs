using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Globalization;
using System.Threading;

namespace Sidi.Util
{
    [TestFixture, SetCulture("en-US")]
    public class ProgressTest : Sidi.Test.TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void ToStringTest()
        {
            var p = new Progress
            {
                Total = 1e6,
                Done = 5e5,
            };
            p.End = p.Begin.AddSeconds(10);

            var s = p.ToString();
            log.Info(s);
            Assert.IsTrue(p.ToString().StartsWith("50.0% (500k/1.00M, rate=50.0k/s, rem=00:00:10, eta="));
        }

        [Test]
        public void ExtremeValues()
        {
            var p = new Progress
            {
                Total = 1e99,
                Done = 1,
            };
            p.End = p.Begin.AddYears(1);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var s = p.ToString();
            Assert.AreEqual("0.0% (1.00/1E+99, rate=31.7n/s, rem=10675199.02:48:05.4775807, eta=12/31/9999 23:59:59)", s);
        }

        [Test]
        public void Ctor()
        {
            var p = new Progress();
            var expected = "0.0% (0/0, rate=0/s, rem=00:00:00, eta=";
            Assert.AreEqual(expected, p.ToString().Substring(0, expected.Length));
        }

        [Test]
        public void Update()
        {
            var p = new Progress(log.Info) { Total = 1e6 };
            p.Update(5e5);
        }

        [Test]
        public void SafeDiv()
        {
            Assert.AreEqual(0, Progress.SafeDiv(0, 0));
            Assert.AreEqual(0, Progress.SafeDiv(1, 0));
            Assert.AreEqual(0, Progress.SafeDiv(-1, 0));
            Assert.AreEqual(2, Progress.SafeDiv(4,2));
        }
    }
}
