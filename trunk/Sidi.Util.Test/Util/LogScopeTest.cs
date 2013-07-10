using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Test;

namespace Sidi.Util
{
    [TestFixture]
    public class LogScopeTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Log()
        {
            using (new LogScope(log.Info, "test with params={0},{1},{2}", 1, 2, 3))
            {
            }
        }
    }
}
