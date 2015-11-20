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
using Sidi.Extensions;

namespace Sidi.Util.Tests
{
    [TestFixture()]
    public class XmlSerializerObjectHashProviderTests : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Print(Hash a, Hash b)
        {
            Console.WriteLine("            Assert.AreEqual(new Hash({0})", b.ToString().Quote());
        }

        [Test()]
        public void Get()
        {
            var hp = new XmlSerializerObjectHashProvider(HashProvider.GetDefault());
            Assert.AreEqual(new Hash("3a57b1e6f78f5adbf6f23c0af317d84175e15972"), hp.Get(1));
            Assert.AreEqual(new Hash("f94e81dff8a42c4877c093efd1533177999b7ddc"), hp.Get("Hello"));
            Assert.AreEqual(new Hash("da39a3ee5e6b4b0d3255bfef95601890afd80709"), hp.Get(null));
            Assert.AreEqual(new Hash("ac164e379e469c897bd01f029a64ed79df8ef6ed"), hp.Get(MethodBase.GetCurrentMethod()));

            var fv = new FileVersion("a", 1, new DateTime(2015, 1, 1, 1, 1, 1));
            Assert.AreEqual(new Hash("3f5b2ecc1b46f80e204ff999dd33c384e5b36ce4"), hp.Get(fv));
        }
    }
}
