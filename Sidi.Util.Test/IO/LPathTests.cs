using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.IO;
using NUnit.Framework;
using Sidi.Test;

namespace Sidi.IO.Tests
{
    [TestFixture]
    public class LPathTests : TestBase
    {
        [Test]
        public void EnsureNotExistsTest()
        {
            var root = TestFile("EnsureNotExistsTest");

            var lp = root.CatDir(
                Enumerable.Range(0, 100)
                    .Select(x => String.Format("PathPart{0}", x)));

            lp.EnsureParentDirectoryExists();
            lp.WriteAllText("hello");
            Assert.IsTrue(lp.IsFile);
            
            root.EnsureNotExists();
            Assert.IsFalse(root.Exists);
        }

        [Test]
        public void EnsureNotExists_removes_an_empty_directory_tree()
        {
            var root = TestFile("EnsureNotExists_removes_an_empty_directory_tree");

            var lp = root.CatDir(
                Enumerable.Range(0, 100)
                    .Select(x => String.Format("PathPart{0}", x)));

            lp.EnsureDirectoryExists();
            Assert.IsTrue(root.Exists);
            
            root.EnsureNotExists();
            Assert.IsFalse(root.Exists);
        }
    }
}
