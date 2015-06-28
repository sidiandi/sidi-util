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
using Sidi.Extensions;

#pragma warning disable 618

namespace Sidi.IO
{
    [TestFixture]
    public class FileTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public LPath root;
        public LPath longPath;

        [SetUp]
        public void Setup()
        {
            root = Paths.BinDir.CatDir(@"test\LongNameTest");
            root.EnsureNotExists();
            longPath = root.CatDir(Enumerable.Range(0, 100).Select(x => String.Format("PathPart{0}", x)));
            longPath.EnsureParentDirectoryExists();
            log.Info(() => longPath.StringRepresentation.Length);
        }

        [TearDown]
        public void TearDown()
        {
            root.EnsureNotExists();
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
            var root = TestFile("FileTestEnum");
            foreach (var i in Enumerable.Range(0, 10))
            {
                CreateSampleFile(root.CatDir(i.ToString()));
            }
            var e = root.GetChildren();
            Assert.IsTrue(e.Count() >= 10);
            root.EnsureNotExists();
        }

        public void CreateSampleFile(LPath lp)
        {
            lp.WriteAllText("Hello");
        }

        [Test]
        public void Copy()
        {
            CreateSampleFile(longPath);
            var lpCopy = longPath.CatName(".copy");
            LFile.Copy(longPath, lpCopy);
            Assert.IsTrue(lpCopy.IsFile);
        }

        [Test]
        public void CopyProgress()
        {
            var bigSampleFile = root.CatDir("big");

            using (var f = LFile.Open(bigSampleFile, System.IO.FileMode.Create))
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
            Assert.IsTrue(lpCopy.IsFile);
            lpCopy.EnsureNotExists();
            bigSampleFile.EnsureNotExists();
        }

        [Test]
        public void HardLink()
        {
            CreateSampleFile(longPath);
            var lpCopy = longPath.CatName(".link");
            LFile.CreateHardLink(lpCopy, longPath);
            Assert.IsTrue(lpCopy.IsFile);
            log.Info(longPath.Parent.Children);
            log.Info(longPath);
            Assert.IsTrue(LFile.EqualByTimeAndLength(longPath, lpCopy));
        }

        [Test]
        public void Move()
        {
            CreateSampleFile(longPath);
            var m = longPath.Root.CatDir(longPath.Parts.Take(20));
            var dest = root.CatDir("moved");
            LDirectory.Move(m, dest);
            Assert.IsTrue(LDirectory.Exists(dest));
            Assert.IsFalse(longPath.IsFile);
            Assert.IsTrue(LDirectory.Exists(root));
        }

        [Test]
        public void Equal()
        {
            var f1 = longPath;
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

        [Test]
        public void Reopen()
        {
            var random = new Random();
            var relPath = LPath.CreateRelative(Enumerable.Range(0, 100).Select(x => random.String(10)));
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
            using (var w = LFile.StreamWriter(p))
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
            Assert.IsTrue(c.IsFile);

            JunctionPoint.Create(a.CatDir("b"), b);
            
            a.EnsureNotExists();
            
            Assert.IsTrue(b.IsDirectory);
            Assert.IsTrue(c.IsFile);
        }
    }
}
