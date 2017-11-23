using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.CommandLine.GetOptInternal.Test
{
    [TestFixture]
    public class GetOptOptionTest
    {
        [Test]
        public void CSharpIdentifierToLongOption()
        {
            Assert.AreEqual("say-hello", Sidi.CommandLine.GetOptInternal.Option.CSharpIdentifierToLongOption("SayHello"));
            Assert.AreEqual("get-wlan", Sidi.CommandLine.GetOptInternal.Option.CSharpIdentifierToLongOption("GetWLAN"));
        }
    }
}
