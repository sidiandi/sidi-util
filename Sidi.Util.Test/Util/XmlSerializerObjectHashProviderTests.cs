using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Util;
using NUnit.Framework;
using Sidi.Test;
using System.Reflection;
using Sidi.IO;
namespace Sidi.Util.Tests
{
    [TestFixture()]
    public class XmlSerializerObjectHashProviderTests : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test()]
        public void Get()
        {
            var hp = new XmlSerializerObjectHashProvider(HashProvider.GetDefault());
            Assert.AreEqual(new Hash("dc57a758dfdebce53b4306ec3df857f8f5a0e971"), hp.Get(1));
            Assert.AreEqual(new Hash("07d987af2de5c3001088a81e52b0dcd235c6e56b"), hp.Get("Hello"));
            Assert.AreEqual(new Hash("da39a3ee5e6b4b0d3255bfef95601890afd80709"), hp.Get(null));
            Assert.AreEqual(new Hash("3ad5f65764c24e22c0e942a2255659495cb0b1dd"), hp.Get(MethodBase.GetCurrentMethod()));
            Assert.AreEqual(new Hash("f5fc7509647c44fd82fa40b6c94bfadc61fa6a92"), hp.Get(new FileVersion("a", 1, new DateTime(2015, 1, 1, 1, 1, 1))));
        }
    }
}
