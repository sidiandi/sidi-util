using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sidi.Util.Test
{
    [TestFixture]
    public class RegistrySerializerTest
    {
        public class Example
        {
            public string AStringVariable;
            public bool ABoolVariable;
            public int AIntVariable;
        }

        [Test]
        public void ReadWrite()
        {
            var x = new Example();
            x.AStringVariable = "Hello";
            x.ABoolVariable = true;
            x.AIntVariable = 123;

            string key = @"HKEY_CURRENT_USER\Software\sidi-util\Test\Example";
            RegistrySerializer.Write(key, x);

            var y = new Example();
            RegistrySerializer.Read(key, y);
            Assert.AreEqual(x.AStringVariable, y.AStringVariable);
            Assert.AreEqual(x.AIntVariable, y.AIntVariable);
            Assert.AreEqual(x.ABoolVariable, y.ABoolVariable);
        }
    }
}
