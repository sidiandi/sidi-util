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
            var hp = ObjectHashProvider.GetDefault();
            hp.Get(1);
            hp.Get("Hello");
            hp.Get(null);
            hp.Get(MethodBase.GetCurrentMethod());
        }
    }
}
