using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Sidi.Util;

namespace Sidi.IMAP
{
    [TestFixture]
    public class ArgParserTest
    {
        public void GetString()
        {
            Assert.AreEqual("Hello", P("Hello").GetAtom());
            Assert.AreEqual("Hello", P("\"Hello\"").GetQuotedString());
            Assert.AreEqual("Hello", P("Hello").GetAtom());
            var o = P("(Hello 1 2 3 (more in this list))").Get();
            Console.WriteLine(((object[])o).Join());
        }

        ArgParser P(string argString)
        {
            return new ArgParser(argString, null, null);
        }

        public void List()
        {
            Assert.IsNotNull(P("(UID)").GetList());
        }

    }
}
