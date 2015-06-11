using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Util;
using NUnit.Framework;
using System.Reflection;
namespace Sidi.Util.Tests
{
    [TestFixture()]
    public class IHashProviderExtensionTests
    {
        [Test()]
        public void GetObjectHashTest()
        {
            var hp = HashProvider.GetDefault();

            hp.GetObjectHash(1);
            hp.GetObjectHash("Hello");
            hp.GetObjectHash(null);
            hp.GetObjectHash(MethodBase.GetCurrentMethod());
        }
    }
}
