using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NUnit.Framework;
using Sidi.Util;

namespace Sidi.IO.Long
{
        [TestFixture]
        public class FileTest : TestBase
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            public LongName root;
            public LongName lp;

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

                var e = new FileSystemInfo(root).GetChilds();
                Assert.IsTrue(e.Count() >= 10);
            }

            public void CreateSampleFile(LongName lp)
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
                var m = new LongName(lp.Parts.Take(20));
                var dest = root.CatDir("moved");
                Directory.Move(m, dest);
                Assert.IsTrue(Directory.Exists(dest));
                Assert.IsFalse(File.Exists(lp));
                Assert.IsTrue(Directory.Exists(root));
            }
        }
}
