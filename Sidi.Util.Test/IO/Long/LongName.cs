using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using NUnit.Framework;

namespace Sidi.IO.Long
{
    [TestFixture]
    public static class LongNameExTest
    {
    }

    [TestFixture]
    public class LongNameTest
    {
        [Test, ExpectedException(ExpectedException = typeof(System.IO.PathTooLongException))]
        public void Check()
        {
            var ln = new LongName(Enumerable.Range(0, 4000).Select(x => "0000000000").Join(new string(System.IO.Path.DirectorySeparatorChar, 1)));
        }

        [Test, ExpectedException(ExpectedException = typeof(System.IO.PathTooLongException))]
        public void Check2()
        {
            var ln = new LongName(new string('0', 256));
        }

        [Test]
        public void Parts()
        {
            var pCount = 40;
            var part = "0000000000";
            var ln = new LongName(Enumerable.Range(0, pCount).Select(x => part).Join(new string(System.IO.Path.DirectorySeparatorChar, 1)));
            var p = ln.Parts;
            Assert.AreEqual(pCount, p.Count());
            Assert.AreEqual(part, p[0]);
        }
    }
}
