using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sidi.IO.Long
{
    [TestFixture]
    public class FileListTest
    {
        [Test]
        public void Parse()
        {
            var text = @"C:\temp;D:\docs;E:\something";
            var fl = FileList.Parse(text);
            Assert.AreEqual(3, fl.Count);
            Assert.AreEqual(new Path(@"C:\temp"), fl[0]);
        }
    }
}
