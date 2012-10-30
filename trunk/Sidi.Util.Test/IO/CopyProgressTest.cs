using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NUnit.Framework;
using Sidi.Util;
using System.Threading;

namespace Sidi.IO
{
    [TestFixture]
    public class CopyProgressTestTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void CopyProgressObject()
        {
            CopyProgress cp;
            var start = DateTime.Now;
            cp = new CopyProgress(null, null);
            log.Info(cp.Message);
            Thread.Sleep(10);
            cp.Update(0, 100000);
            log.Info(cp.Message);
            Thread.Sleep(10);
            cp.Update(1, 1000000000000);
            log.Info(cp.Message);
        }
    }
}
