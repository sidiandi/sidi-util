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
    public class ConcatStreamTests
    {
        [Test()]
        public void ConcatStreamTest()
        {
            int n = 100;
            var streams = Enumerable.Range(0, n)
                .Select(i => new MemoryStream(new byte[] { (byte)i }));

            var c = new ConcatStream(streams);
            var buffer = new byte[n];
            var readData = c.ReadToEnd();
            Assert.AreEqual(n, readData.Length);
        }
    }
}