using NUnit.Framework;
using Sidi.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Collections.Tests
{
    [TestFixture()]
    public class DefaultValueDictionaryTests
    {
        [Test()]
        public void BehavesLikeANormalDictionary()
        {
            var d = new DefaultValueDictionary<int, int>();
            d[1] = 2;
            Assert.AreEqual(1, d.Count);
            Assert.AreEqual(2, d[1]);
        }

        [Test()]
        public void LookingUpUnknownKeysReturnsTheDefaultValue()
        {
            var d = new DefaultValueDictionary<int, int>(_ => _+1);
            Assert.AreEqual(0, d.Count);
            Assert.AreEqual(2, d[1]);
        }

        [Test()]
        public void TheComputedDefaultValuesCanBeStoredForLaterLookups()
        {
            var d = new DefaultValueDictionary<int, int>(_ => _ + 1)
            { StoreDefaults = true};

            Assert.AreEqual(0, d.Count);
            Assert.AreEqual(2, d[1]);
            Assert.AreEqual(1, d.Count);
        }
    }
}