using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sidi.IO
{
    [TestFixture]
    public class OperationTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Copy()
        {
            var op = new Operation();

            var rootDir = this.TestFile("OperationTest");
            var sourceRoot = rootDir.CatDir("source");
            var destRoot = rootDir.CatDir("dest");
            var relChild = LPath.Join(Enumerable.Range(0, 30).Select(x => String.Format("directory_{0}", x)).ToArray());
            op.Delete(rootDir);
            Assert.IsFalse(rootDir.IsDirectory);

            var source = sourceRoot.CatDir(relChild);
            var dest = destRoot.CatDir(relChild);
            log.Info(source);
            Assert.IsFalse(source.IsFile);

            op.EnsureDirectoryExists(source);
            LFile.WriteAllText(source, "hello");
            Assert.IsTrue(source.IsFile);

            op.Copy(sourceRoot, destRoot);
            Assert.IsTrue(destRoot.CatDir(relChild).IsFile);
            Assert.IsTrue(dest.IsFile);
            op.Delete(dest);
            Assert.IsFalse(dest.Exists);
            Assert.IsTrue(dest.Parent.IsDirectory);
            op.DeleteEmptyDirectories(destRoot);
            Assert.IsFalse(destRoot.Exists);

            op.Move(sourceRoot, destRoot);
            Assert.IsTrue(destRoot.IsDirectory);
            Assert.IsFalse(sourceRoot.Exists);
            Assert.IsTrue(destRoot.CatDir(relChild).IsFile);
            
            op.Move(destRoot, sourceRoot);
            Assert.IsTrue(sourceRoot.Exists);

            op.Move(source, dest);
            Assert.IsTrue(dest.IsFile);
            Assert.IsFalse(source.Exists);
            op.Move(dest, source);
            Assert.IsFalse(dest.IsFile);
            Assert.IsTrue(source.Exists);

            op.Simulate = true;
            op.Move(source, dest);
            Assert.IsFalse(dest.IsFile);
            Assert.IsTrue(source.Exists);
            op.Simulate = false;

            op.Link(source, dest);

            log.Info(source.GetPathRoot());
            log.Info(relChild.GetPathRoot());
            Assert.IsTrue(LPath.IsSameFileSystem(sourceRoot, destRoot));
        }
    }
}
