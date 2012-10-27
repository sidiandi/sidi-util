using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Extensions;

namespace Sidi.Extensions
{
    [TestFixture]
    public class IEnumerableExtensionsTest
    {
        [Test]
        public void Best()
        {
            var r = Enumerable.Range(0, 25).Select(x => x.ToString());
            Assert.AreEqual("24", r.Best(x => Int32.Parse(x)));
        }

        [Test]
        public void SafeSelect()
        {
            var x = Enumerable.Range(0, 10).ToList();
            var y = x.SafeSelect(i => 100 / i);
            Assert.AreEqual(x.Count() - 1, y.Count());
        }
    }
}
