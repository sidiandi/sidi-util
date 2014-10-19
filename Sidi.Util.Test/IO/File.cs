// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NUnit.Framework;
using Sidi.Util;
using Sidi.Test;

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

            var e = new LFileSystemInfo(root).GetChildren();
            Assert.IsTrue(e.Count() >= 10);
        }

        public void CreateSampleFile(LPath lp)
        {
                using (var o = lp.WriteText())
                {
                    o.WriteLine("Hello");
                }
        }

        [Test]
        public void Copy()
        {
            CreateSampleFile(lp);
            var lpCopy = lp.CatName(".copy");
            LFile.Copy(lp, lpCopy);
            Assert.IsTrue(LFile.Exists(lpCopy));
            log.Info(lp.Parent.Children);
            log.Info(lp);
        }

        [Test]
        public void CopyProgress()
        {
            var bigSampleFile = root.CatDir("big");

            using (var f = bigSampleFile.OpenWrite())
            {
                var b = new byte[1024 * 1024];
                for (int i = 0; i < 100; ++i)
                {
                    f.Write(b, 0, b.Length);
                }
            }

            log.Info(bigSampleFile.Info.Length);
            var lpCopy = bigSampleFile.CatName(".copy");
            LFile.Copy(bigSampleFile, lpCopy, true, (p) =>
            {
                log.Info(p);
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
            log.Info(lp.Parent.Children);
            log.Info(lp);
            Assert.IsTrue(LFile.EqualByTimeAndLength(lp, lpCopy));
        }

        [Test]
        public void Junction()
        {
            CreateSampleFile(lp);
            var d = lp.Parent;
            var j = d.CatName("-link");
            Assert.IsFalse(j.Exists);
            JunctionPoint.Create(j, d);
            Assert.IsTrue(JunctionPoint.Exists(j));
            Assert.AreEqual(d, JunctionPoint.GetTarget(j));
            Assert.IsTrue(j.CatDir(lp.FileName).IsFile);
            j.EnsureNotExists();
            Assert.IsTrue(lp.IsFile);
        }

        [Test]
        public void JunctionExists()
        {
            lp.EnsureNotExists();
            lp.EnsureDirectoryExists();
            Assert.IsFalse(JunctionPoint.Exists(lp));
            lp.EnsureNotExists();
            Assert.IsFalse(JunctionPoint.Exists(lp));
            var target = lp.Sibling("target");
            target.EnsureDirectoryExists();
            JunctionPoint.Create(lp, target);
            Assert.IsTrue(JunctionPoint.Exists(lp));
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
            var info = new LFileSystemInfo(lp);
            Assert.IsFalse(info.IsReadOnly);
            info.IsReadOnly = true;
            Assert.IsTrue(info.IsReadOnly);
            lp.DeleteFile();
        }

        [Test]
        public void Equal()
        {
            var f1 = lp;
            CreateSampleFile(f1);
            var f2 = f1.CatName(".copy");
            LFile.Copy(f1, f2);
            Assert.IsTrue(LFile.EqualByContent(f1, f2));

            using (var s = f2.OpenWrite())
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
            p.WriteAllText("hello");
            Assert.AreEqual("hello", p.ReadAllText());
            d.EnsureNotExists();
        }

        [Test]
        public void Open()
        {
            var text = "world";
            var d = this.TestFile("Long").CatDir("234234", "23443", "blablabla");
            using (var s = d.WriteText())
            {
                s.WriteLine("hello");
            }
            using (var s = d.WriteText())
            {
                s.Write(text);
            }
            using (var s = d.ReadText())
            {
                Assert.AreEqual(text, s.ReadToEnd());
            }
            Assert.AreEqual(text, d.ReadAllText());
        }

        [Test]
        public void Reopen()
        {
            var random = new Random();
            var relPath = new LPath(Enumerable.Range(0, 100)
                .Select(x => random.String(10)));
            var rootDir = TestFile("opentest");
            rootDir.EnsureNotExists();
            try
            {
                var f = rootDir.CatDir(relPath);
                var text = "Hello, world!";
                using (var w = f.WriteText())
                {
                    w.WriteLine(text);
                    w.Flush();
                    using (var r = f.ReadText())
                    {
                        Assert.AreEqual(text, r.ReadLine());
                    }
                }
                f.EnsureNotExists();
                log.Info(f);
            }
            finally
            {
                rootDir.EnsureNotExists();
            }
        }

        [Test]
        public void CannotDelete()
        {
            var p = TestFile("delete_me");
            using (var w = p.WriteText())
            {
                try
                {
                    p.EnsureNotExists();
                    Assert.IsTrue(false);
                }
                catch (System.IO.IOException ex)
                {
                    log.Info(ex.ToString());
                    var w32 = (Win32Exception)ex.InnerException;
                    Assert.IsFalse(w32.Message.Contains("The operation completed successfully"));
                }
            }
        }

        [Test]
        public void EnsureNotExistsDoesNotFollowJunctionPoints()
        {
            var root = TestFile("EnsureNotExistsDoesNotFollowJunctionPoints");
            root.EnsureNotExists();
            var a = root.CatDir("a");
            a.EnsureDirectoryExists();
            var b = root.CatDir("b");
            var c = b.CatDir("c");
            c.WriteAllText("hello");
            JunctionPoint.Create(a.CatDir("b"), b);
            a.EnsureNotExists();
            Assert.IsTrue(b.IsDirectory);
            Assert.IsTrue(c.IsFile);
        }
    }
}
