using Sidi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sidi.Util.Tests
{
    [TestFixture]
    public class TimeIntervalTests
    {
        [Test]
        public void Parse()
        {
            var t = TimeInterval.Month(DateTime.Now);

            Assert.AreEqual(t, TimeInterval.Parse(t.ToString()));

            t = new TimeInterval(t.Begin, t.End, true, true);
            Assert.AreEqual(t, TimeInterval.Parse(t.ToString()));
        }

        [Test]
        public void Contains()
        {
            var ti = new TimeInterval(new DateTime(2014, 1, 1), new DateTime(2015, 1, 1));
            Assert.IsTrue(ti.Contains(new DateTime(2014, 6, 1)));
            Assert.IsTrue(ti.Contains(ti.Begin));
            Assert.IsFalse(ti.Contains(ti.End));

            Assert.IsFalse(ti.Contains(DateTime.MinValue));
            Assert.IsFalse(ti.Contains(DateTime.MaxValue));
        }

        [Test]
        public void ContainsAndIntersects()
        {
            var a = new TimeInterval(new DateTime(2014, 1, 1), new DateTime(2015, 1, 1));
            var aBeginEdge = new TimeInterval(a.Begin, a.Begin, true, true);
            var aEndEdge = new TimeInterval(a.End, a.End, true, true);
            var b = new TimeInterval(new DateTime(2014, 2, 1), new DateTime(2014, 3, 1));

            Assert.IsTrue(a.Contains(a));
            Assert.IsTrue(a.Contains(b));
            Assert.IsFalse(b.Contains(a));
            Assert.IsTrue(TimeInterval.MaxValue.Contains(a));
            Assert.IsFalse(a.Contains(TimeInterval.MaxValue));
            Assert.IsTrue(a.Contains(aBeginEdge));
            Assert.IsFalse(a.Contains(aEndEdge));

            Assert.IsTrue(a.Intersects(a));
            Assert.IsTrue(a.Intersects(b));
            Assert.IsTrue(b.Intersects(a));
            Assert.IsTrue(TimeInterval.MaxValue.Intersects(a));
            Assert.IsTrue(a.Intersects(TimeInterval.MaxValue));
            Assert.IsTrue(a.Intersects(aBeginEdge));
            Assert.IsFalse(a.Intersects(aEndEdge));

            Assert.AreEqual(a, a.Intersect(a));
            Assert.AreEqual(aBeginEdge, a.Intersect(aBeginEdge));
            Assert.Throws<ArgumentOutOfRangeException>(() => a.Intersect(aEndEdge));
            Assert.AreEqual(aBeginEdge, aBeginEdge.Intersect(a));
            Assert.AreEqual(a, a.Intersect(TimeInterval.MaxValue));
        }

        [Test()]
        public void TimeIntervalTest()
        {
            //! [TimeIntervalTestCtor]
            var t = DateTime.UtcNow;

            var zeroLengthTimeInterval = new TimeInterval(t, t);
            Assert.IsTrue(zeroLengthTimeInterval.Contains(zeroLengthTimeInterval.Begin));
            Assert.IsTrue(zeroLengthTimeInterval.Contains(zeroLengthTimeInterval.End));
            Assert.AreEqual(zeroLengthTimeInterval.Begin, zeroLengthTimeInterval.End);

            var timeInterval = new TimeInterval(t, t.AddSeconds(1));
            Assert.IsTrue(timeInterval.Contains(timeInterval.Begin));
            Assert.IsFalse(timeInterval.Contains(timeInterval.End));
            //! [TimeIntervalTestCtor]
        }
    }
}
