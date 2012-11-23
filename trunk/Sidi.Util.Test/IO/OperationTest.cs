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

        public OperationTest()
        {
            rootDir = this.TestFile("OperationTest");
            sourceRoot = rootDir.CatDir("source");
            destRoot = rootDir.CatDir("dest");
            relChild = LPath.Join(Enumerable.Range(0, 30).Select(x => String.Format("directory_{0}", x)).ToArray());
            source = sourceRoot.CatDir(relChild);
            dest = destRoot.CatDir(relChild);
        }

        [SetUp]
        public void Setup()
        {
            var op = new Operation();

            op.Delete(rootDir);
            Assert.IsFalse(rootDir.Exists);

            log.Info(source);
            Assert.IsFalse(source.IsFile);

            op.EnsureDirectoryExists(source);
            LFile.WriteAllText(source, "hello");
            Assert.IsTrue(source.IsFile);
        }

        [TearDown]
        public void TearDown()
        {
            new Operation().Delete(rootDir);
        }

        LPath rootDir;
        LPath sourceRoot;
        LPath destRoot;
        LPath relChild;
        LPath source;
        LPath dest;

        [Test]
        public void Copy()
        {
            var op = new Operation();

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
            Assert.IsTrue(LPath.IsSameFileSystem(sourceRoot, destRoot));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void PathRoot()
        {
            log.Info(relChild.GetPathRoot());
        }

        [Test, ExpectedException(typeof(System.IO.IOException))]
        public void NoOverwrite()
        {
            var op = new Operation();
            op.Copy(sourceRoot, destRoot);
            op.Copy(sourceRoot, destRoot);
        }

        [Test]
        public void Overwrite()
        {
            var op = new Operation() { Overwrite = true };
            op.Copy(sourceRoot, destRoot);
            op.Copy(sourceRoot, destRoot);
        }

        [Test]
        public void Overwrite2()
        {
            var op = new Operation() { Overwrite = true };
            op.Copy(sourceRoot, destRoot);
            op.Link(sourceRoot, destRoot);
        }

        [Test]
        public void Overwrite3()
        {
            var op = new Operation() { Overwrite = true };
            op.Copy(sourceRoot, destRoot);
            op.Move(sourceRoot, destRoot);
        }
    }
}
