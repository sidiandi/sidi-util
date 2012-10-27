using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using System.Globalization;

namespace Sidi.Util
{
    [TestFixture]
    public class BinaryPrefixTest
    {
        public void Binary()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var b = new BinaryPrefix();
            Assert.AreEqual("1.00 Ki", String.Format(b, "{0:B}", 1024));
            Assert.AreEqual("1.46 Ki", String.Format(b, "{0:B}", 1500));
            Assert.AreEqual("1.00 Mi", String.Format(b, "{0:B}", 1 << 20));
        }
    }
}
