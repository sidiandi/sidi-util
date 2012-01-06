using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;

namespace Sidi.Collections
{
    [TestFixture]
    public class CacheTest
    {
        [Test]
        public void DoCache()
        {
            var c = Cache.Local(MethodBase.GetCurrentMethod());
            c.Clear();
            
            for (int i = 0; i < 10; ++i)
            {
                var result = c.GetCached(i, () => i * i);
                Assert.AreEqual(i * i, result);
            }

            c = Cache.Local(MethodBase.GetCurrentMethod());
            for (int i = 0; i < 10; ++i)
            {
                Assert.IsTrue(c.IsCached(i));
            }

            c.Clear();
            for (int i = 0; i < 10; ++i)
            {
                Assert.IsFalse(c.IsCached(i));
            }
        }

        [Test]
        public void Exception()
        {
            var c = Cache.Local(MethodBase.GetCurrentMethod());
            c.Clear();

            var d = 0;
            try
            {
                var r = c.GetCached(d, () => 1 / d);
                Assert.IsTrue(false);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is System.DivideByZeroException);
            }

            try
            {
                var r = c.GetCached(d, () => 1 / d);
                Assert.IsTrue(false);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(System.DivideByZeroException), e.GetType());
            }
        }
    }
}
