using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.IO
{
    [TestFixture]
    public class PathListTest : Sidi.Test.TestBase
    {
        [Test]
        public void Parse()
        {
            var t = @"C:\temp\1;C:\temp\2";
            var pl = PathList.Parse(t);
            Assert.AreEqual(2, pl.Count);
        }
    }
}
