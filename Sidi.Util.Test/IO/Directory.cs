using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace Sidi.IO
{
    [TestFixture]
    public class DirectoryTest
    {
        [Test]
        public void Exists()
        {
            var p = new Path(String.Format(@"\\{0}\C$", System.Environment.MachineName));
            Assert.IsTrue(System.IO.Directory.Exists(p.NoPrefix)); 
            Assert.IsTrue(Directory.Exists(p));
        }
    }
}
