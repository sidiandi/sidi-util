using NUnit.Framework;
using Sidi.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace Sidi.IO.Tests
{
    [TestFixture()]
    public class CopyStreamTests
    {
        [Test()]
        public void CopyStreamTest()
        {
            //! [Usage]
            var data = ASCIIEncoding.ASCII.GetBytes("Hello, world!");
            var source = new MemoryStream(data);
            var copy = new MemoryStream();

            var fork = new CopyStream(source, copy);
            Assert.AreEqual(data, fork.ReadToEnd());
            copy.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(data, copy.ReadToEnd());
            //! [Usage]
        }
    }
}