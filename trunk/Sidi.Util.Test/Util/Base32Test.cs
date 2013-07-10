using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Test;

namespace Sidi.Util
{
    [TestFixture]
    public class Base32Test : TestBase
    {
        [Test]
        public void Encode()
        {
            BASE32("foobar", "MZXW6YTBOI");
            BASE32("foo", "MZXW6");
            BASE32("foob", "MZXW6YQ");
            BASE32("fooba", "MZXW6YTB");
        }

        void BASE32(string input, string encoded)
        {
            Assert.AreEqual(encoded, Base32.Encode(ASCIIEncoding.ASCII.GetBytes(input)));
        }
    }
}
