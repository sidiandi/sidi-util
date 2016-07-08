using NUnit.Framework;
using Sidi.CredentialManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.CredentialManagement.Tests
{
    [TestFixture()]
    public class FactoryTests
    {
        [Test(), Explicit("interactive")]
        public void GetCredentialProviderTest()
        {
            var p = Sidi.CredentialManagement.Factory.GetCredentialProvider("Enter User: u, Password: p");
            p.Reset();
            p.SetInteractive();

            var p1 =  Sidi.CredentialManagement.Factory.GetCredentialProvider("Enter User: u, Password: p");
            var c = p1.GetCredential();
            Assert.AreEqual("u", c.UserName);
            Assert.AreEqual("p", c.Password);
        }
    }
}