using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace Sidi.IMAP
{
    [TestFixture]
    public class Rfc822MessageTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Parse()
        {
            string testMessage = TestFile(@"mail\message-1-1456.eml");

            var m = Rfc822Message.Parse(File.OpenText(testMessage));
            Assert.AreEqual(28, m.Headers.Count);
            log.Info(m.Subject);
            log.Info(m.Date);
        }
    }
}
