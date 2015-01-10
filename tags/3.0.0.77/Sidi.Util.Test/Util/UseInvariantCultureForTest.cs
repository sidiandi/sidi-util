using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sidi.Util
{
    [SetUpFixture]
    public class UseInvariantCultureForTest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        CultureInfo initialCulture;

        [SetUp]
        public void Setup()
        {
            initialCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        [TearDown]
        public void TearDown()
        {
            Thread.CurrentThread.CurrentCulture = initialCulture;
        }
    }
}
