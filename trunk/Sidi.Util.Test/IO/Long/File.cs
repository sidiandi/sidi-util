using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NUnit.Framework;
using Sidi.Util;
using Sidi.IO.Long.Extensions;

namespace Sidi.IO.Long
{
        [TestFixture]
        public class FileTest : TestBase
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            public Path root;
            public Path lp;

            [SetUp]
            public void Setup()
            {
                root = Sidi.IO.FileUtil.BinFile(@"test\LongNameTest").Long();

                root.EnsureNotExists();

                lp = root.CatDir(
                    Enumerable.Range(0, 100)
                        .Select(x => String.Format("PathPart{0}", x))
                        .ToArray());

                lp.EnsureParentDirectoryExists();
            }

            [TearDown]
            public void TearDown()
            {
                root.EnsureNotExists();
                Assert.IsFalse(root.Exists);
            }

            [Test]
            public void SetupAndTearDown()
            {
                log.Info("do nothing");
            }

            [Test]
            public void Enum()
            {
                // create 10 files
                foreach (var i in Enumerable.Range(0, 10))
                {
                    CreateSampleFile(root.CatDir(i.ToString()));
                }

                var e = new FileSystemInfo(root).GetFileSystemInfos();
                Assert.IsTrue(e.Count() >= 10);
            }

            public void CreateSampleFile(Path lp)
            {
                using (var f = File.Open(lp, System.IO.FileMode.Create))
                {
                    using (var o = new System.IO.StreamWriter(f))
                    {
                        o.WriteLine("Hello");
                    }
                }
            }

            [Test]
            public void Copy()
            {
                CreateSampleFile(lp);
                var lpCopy = (lp.ToString() + ".copy").Long();
                File.Copy(lp, lpCopy);
                Assert.IsTrue(File.Exists(lpCopy));

                log.Info(Directory.GetChilds(lp.ParentDirectory));

                log.Info(lp);
            }

            [Test]
            public void HardLink()
            {
                CreateSampleFile(lp);
                var lpCopy = (lp.ToString() + ".link").Long();
                File.CreateHardLink(lpCopy, lp);
                Assert.IsTrue(File.Exists(lpCopy));
                log.Info(Directory.GetChilds(lp.ParentDirectory));
                log.Info(lp);
                Assert.IsTrue(File.EqualByTime(lp, lpCopy));
            }

            [Test]
            public void Move()
            {
                CreateSampleFile(lp);
                var m = new Path(lp.Parts.Take(20));
                var dest = root.CatDir("moved");
                Directory.Move(m, dest);
                Assert.IsTrue(Directory.Exists(dest));
                Assert.IsFalse(File.Exists(lp));
                Assert.IsTrue(Directory.Exists(root));
            }

            [Test]
            public void DeleteReadOnlyFiles()
            {
                CreateSampleFile(lp);
                var info = new FileSystemInfo(lp);
                Assert.IsFalse(info.IsReadOnly);
                info.IsReadOnly = true;
                Assert.IsTrue(info.IsReadOnly);
                File.Delete(lp);
            }

            [Test]
            public void Equal()
            {
                var f1 = lp;
                CreateSampleFile(f1);
                var f2 = f1.CatName(".copy");
                File.Copy(f1, f2);
                Assert.IsTrue(File.EqualByContent(f1, f2));

                using (var s = File.Open(f2, System.IO.FileMode.Create))
                {
                    s.WriteByte(0);
                }

                Assert.IsFalse(File.EqualByContent(f1, f2));
            }
        }
}
