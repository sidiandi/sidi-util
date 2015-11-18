using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Persistence;
using NUnit.Framework;
using Sidi.Test;
namespace Sidi.Persistence.Tests
{
    [TestFixture()]
    public class SetTests : TestBase
    {
        [Test()]
        public void SetTest()
        {
            var p = TestFile("set");
            p.EnsureFileNotExists();
            var s = new Sidi.Persistence.Set<int>(p, "setTest");

            var numberCount = 100;
            var numbers = Enumerable.Range(0, numberCount);

            using (var t = s.BeginTransaction())
            {
                foreach (var i in numbers)
                {
                    s.Add(i);
                }
                Assert.AreEqual(numberCount, s.Count);

                foreach (var i in numbers)
                {
                    Assert.IsTrue(s.Contains(i));
                }

                foreach (var i in numbers)
                {
                    s.Remove(i);
                }
                Assert.AreEqual(0, s.Count);

                foreach (var i in numbers)
                {
                    s.Add(i);
                }
                Assert.AreEqual(numberCount, s.Count);

                s.Clear();

                Assert.AreEqual(0, s.Count);

                t.Commit();
            }
        }
    }
}
