using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Globalization;
using System.Threading;

namespace Sidi.Util.Test
{
    [TestFixture, SetCulture("en-US")]
    class MetricPrefixTest : Sidi.Test.TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string f(double x)
        {
            return String.Format(MetricPrefix.Instance, "{0:M}", x);
        }

        string fi(int x)
        {
            return String.Format(MetricPrefix.Instance, "{0:M}", x);
        }

        [Test]
        public void Format()
        {
            Assert.AreEqual("1.00k", 1000d.MetricPrefix());
            Assert.AreEqual("1.00k", String.Format(MetricPrefix.Instance, "{0:M}", 1000d));
            Assert.AreEqual("1000", String.Format(MetricPrefix.Instance, "{0}", 1000d));

            Assert.AreEqual("1.00", f(1.0d));
            Assert.AreEqual("1.00k", f(1000));
            Assert.AreEqual("-1.00k", f(-1000));
            Assert.AreEqual("990", f(990));
            Assert.AreEqual("123", f(123));
            Assert.AreEqual("12.3", f(12.3456));
            Assert.AreEqual("1.00M", f(1e6));
            Assert.AreEqual("12.3M", f(12.3456e6));
            Assert.AreEqual("1.00m", f(1e-3));
            Assert.AreEqual("1.23µ", f(1.23e-6));
            Assert.AreEqual("1.00n", f(1e-9));

            Assert.AreEqual("1.00", fi(1));
            Assert.AreEqual("1.00k", fi(1000));
            Assert.AreEqual("-1.00k", fi(-1000));
            Assert.AreEqual("990", fi(990));
            Assert.AreEqual("123", fi(123));
            Assert.AreEqual("12.0", fi(12));
            Assert.AreEqual("1.00M", fi(1000000));
        }

        [Test]
        public void ExtremeValues()
        {
            Assert.AreEqual("1.23E+99", f(1.2345678e99));
            Assert.AreEqual("1.23E-99", f(1.2345678e-99));
            Assert.AreEqual("-1.23E+99", f(-1.2345678e99));
        }
    }
}
