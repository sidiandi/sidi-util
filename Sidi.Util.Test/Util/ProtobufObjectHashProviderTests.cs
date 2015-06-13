using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Util;
using NUnit.Framework;
using Sidi.Extensions;
using System.Reflection;
using Sidi.Test;
using Sidi.IO;

namespace Sidi.Util.Tests
{
    [TestFixture()]
    public class ProtobufObjectHashProviderTests : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test()]
        public void Get()
        {
            var hp = new ProtobufObjectHashProvider(HashProvider.GetDefault());
            Assert.AreEqual(new Hash("4f8190a08041a67360ceea6c64f9be3ffb59b602"), hp.Get(1));
            Assert.AreEqual(new Hash("27bbce9ea46729635d4f3ff1715323c88f19c519"), hp.Get("Hello"));
            Assert.AreEqual(new Hash("da39a3ee5e6b4b0d3255bfef95601890afd80709"), hp.Get(null));
            Assert.AreEqual(new Hash("54d2114ad9ac93720072d72ed63f9aa92bd00cc2"), hp.Get(MethodBase.GetCurrentMethod()));
            Assert.AreEqual(new Hash("0394b1344f2131c324576aa88770257b4270dd86"), hp.Get(new FileVersion("a", 1, new DateTime(2015, 1, 1, 1, 1, 1))));
        }
    }
}
