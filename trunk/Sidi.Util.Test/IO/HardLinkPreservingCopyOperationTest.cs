using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sidi.IO
{
    [TestFixture]
    public class HardLinkPreservingCopyOperationTest : TestBase
    {
        [Test]
        public void Test()
        {
            var count = 10;
            var sourceDir = TestFile("copy-hardlink-test");
            sourceDir.EnsureNotExists();
            sourceDir.EnsureDirectoryExists();
            var f = sourceDir.CatDir("orig");
            LFile.WriteAllText(f, "hello");
            for (int i = 0; i < count; ++i)
            {
                LFile.CreateHardLink(f.CatName(i.ToString()), f);
            }
            Assert.AreEqual(count + 1, f.Info.FileLinkCount);

            var destinationDir = new LPath(@"E:\temp\copy-hardlink-test");
            destinationDir.EnsureNotExists();
            destinationDir.EnsureDirectoryExists();

            var co = new HardLinkPreservingCopyOperation();
            co.Copy(sourceDir, destinationDir);
            var c = destinationDir.Children;
            Assert.AreEqual(count + 1, c.Count);
            var g = c.GroupBy(x => x.Info.FileIndex);

            Assert.AreEqual(1, g.Count());
        }
    }
}
