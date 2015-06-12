using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Util;
using NUnit.Framework;
using System.IO;
using Sidi.Test;
using Sidi.Extensions;

namespace Sidi.Util.Tests
{
    [TestFixture()]
    public class HashTests : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test()]
        public void Serialize()
        {
            var hash = HashProvider.GetDefault().Get(new MemoryStream());
            log.Info(() => hash);    
            TestSerialize(hash);
            TestXmlSerialize(hash);
        }
    }
}
