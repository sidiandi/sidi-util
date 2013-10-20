using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Test;

namespace Sidi.IO
{
    [TestFixture]
    public class PathsTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Type()
        {
            var p = Paths.Get(GetType());
            log.Info(p);
        }
    }
}
