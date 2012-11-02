﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NUnit.Framework;
using Sidi.Util;

namespace Sidi.IO
{
        [TestFixture]
        public class FileTest : TestBase
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            public LPath root;
            public LPath lp;

            [SetUp]
            public void Setup()
            {
                root = Paths.BinDir.CatDir(@"test\LongNameTest");

                root.EnsureNotExists();

                lp = root.CatDir(
                    Enumerable.Range(0, 100)
                        .Select(x => String.Format("PathPart{0}", x)));

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

            public void CreateSampleFile(LPath lp)
            {
                using (var f = LFile.Open(lp, System.IO.FileMode.Create))
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
                var lpCopy = lp.CatName(".copy");
                LFile.Copy(lp, lpCopy);
                Assert.IsTrue(LFile.Exists(lpCopy));

                log.Info(LDirectory.GetChilds(lp.Parent));

                log.Info(lp);
            }

            [Test]
            public void CopyProgress()
            {
                var bigSampleFile = root.CatDir("big");
                
                using (var f = LFile.Open(bigSampleFile, System.IO.FileMode.Create))
                {
                    var b = new byte[1024*1024];
                    for (int i = 0; i < 100; ++i)
                    {
                        f.Write(b, 0, b.Length);
                    }
                }

                log.Info(bigSampleFile.Info.Length);
                var lpCopy = bigSampleFile.CatName(".copy");
                LFile.Copy(bigSampleFile, lpCopy, true, (p) =>
                {
                    log.Info(p.Message);
                });
                Assert.IsTrue(LFile.Exists(lpCopy));
                lpCopy.EnsureNotExists();
                bigSampleFile.EnsureNotExists();
            }

            [Test]
            public void HardLink()
            {
                CreateSampleFile(lp);
                var lpCopy = lp.CatName(".link");
                LFile.CreateHardLink(lpCopy, lp);
                Assert.IsTrue(LFile.Exists(lpCopy));
                log.Info(LDirectory.GetChilds(lp.Parent));
                log.Info(lp);
                Assert.IsTrue(LFile.EqualByTime(lp, lpCopy));
            }

            [Test]
            public void Move()
            {
                CreateSampleFile(lp);
                var m = new LPath(lp.Parts.Take(20));
                var dest = root.CatDir("moved");
                LDirectory.Move(m, dest);
                Assert.IsTrue(LDirectory.Exists(dest));
                Assert.IsFalse(LFile.Exists(lp));
                Assert.IsTrue(LDirectory.Exists(root));
            }

            [Test]
            public void DeleteReadOnlyFiles()
            {
                CreateSampleFile(lp);
                var info = new FileSystemInfo(lp);
                Assert.IsFalse(info.IsReadOnly);
                info.IsReadOnly = true;
                Assert.IsTrue(info.IsReadOnly);
                LFile.Delete(lp);
            }

            [Test]
            public void Equal()
            {
                var f1 = lp;
                CreateSampleFile(f1);
                var f2 = f1.CatName(".copy");
                LFile.Copy(f1, f2);
                Assert.IsTrue(LFile.EqualByContent(f1, f2));

                using (var s = LFile.Open(f2, System.IO.FileMode.Create))
                {
                    s.WriteByte(0);
                }

                Assert.IsFalse(LFile.EqualByContent(f1, f2));
            }

            [Test]
            public void WriteAllText()
            {
                var d = this.TestFile("Long").CatDir("blablabla");
                d.EnsureNotExists();
                var p = d.CatDir(".moviesidi");
                LFile.WriteAllText(p, "hello");
                Assert.AreEqual("hello", LFile.ReadAllText(p));
                d.EnsureNotExists();
            }

            [Test]
            public void Open()
            {
                var text = "world";
                var d = this.TestFile("Long").CatDir("234234", "23443", "blablabla");
                using (var s = LFile.StreamWriter(d))
                {
                    s.WriteLine("hello");
                }
                using (var s = LFile.StreamWriter(d))
                {
                    s.Write(text);
                }
                using (var s = LFile.StreamReader(d))
                {
                    Assert.AreEqual(text, s.ReadToEnd());
                }
                Assert.AreEqual(text, LFile.ReadAllText(d));
            }
        }
}
