using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sidi.Util
{
    [TestFixture]
    public class TimeIntervalTest
    {
        [Test]
        public void Intersect()
        {
            var t = new DateTime(2012, 10, 1);
            var i1 = new TimeInterval(t, t.AddDays(1));
            var i2 = new TimeInterval(t, t);
            Assert.IsFalse(i1.Intersects(i2));
            Assert.IsFalse(i2.Intersects(i1));

            Assert.IsTrue(i1.Includes(i2));
            Assert.IsFalse(i2.Includes(i1));
        }

        [Test]
        public void Parse()
        {
            var t = TimeInterval.Month(DateTime.Now);
            Assert.AreEqual(t, TimeInterval.Parse(t.ToString()));
        }
    }
}
