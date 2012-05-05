using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace Sidi.Util
{
    [TestFixture]
    public class IndentWriterTest
    {
        [Test]
        public void Indent()
        {
            var o = new StringWriter();
            var iw = new IndentWriter(o, "  ", true);
            var text = "Hello";
            iw.WriteLine(text);
            Assert.AreEqual("  Hello\r\n", o.ToString());
        }

        [Test]
        public void Indent2()
        {
            var o = new StringWriter();
            var iw = new IndentWriter(o, "  ", true);
            iw.Write('a');
            Assert.AreEqual("  a", o.ToString());
        }
    }
}
