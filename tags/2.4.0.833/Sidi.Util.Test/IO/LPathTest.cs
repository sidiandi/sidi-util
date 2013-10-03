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
using Sidi.Util;
using NUnit.Framework;
using System.Xml.Serialization;
using Sidi.Extensions;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Sidi.Test;

namespace Sidi.IO
{
    [TestFixture]
    public class LPathTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test, ExpectedException(ExpectedException = typeof(System.IO.PathTooLongException))]
        public void Check()
        {
            var ln = new LPath(Enumerable.Range(0, 4000).Select(x => "0000000000").Join(new string(System.IO.Path.DirectorySeparatorChar, 1)));
        }

        [Test, ExpectedException(ExpectedException = typeof(System.IO.PathTooLongException))]
        public void Check2()
        {
            var ln = new LPath(new string('0', 256));
        }

        [Test]
        public void UseAsString()
        {
            var cd = new LPath(System.Environment.CurrentDirectory);
            Assert.AreEqual(cd.ToString(), new System.IO.DirectoryInfo(cd).FullName);
        }

        [Test]
        public void Serialize()
        {
            var p = Paths.BinDir;
            var b = new BinaryFormatter();
            var m = new MemoryStream();
            b.Serialize(m, p);
            m.Seek(0, SeekOrigin.Begin);
            var p1 = (LPath)b.Deserialize(m);
            Assert.AreEqual(p, p1);
        }

        [Test]
        public void FullPath()
        {
            var ln = new LPath(Enumerable.Range(0, 100).Select(x => "0000000000"));
            var cd = new LPath(System.Environment.CurrentDirectory);
            Assert.AreEqual(cd.CatDir(ln), ln.GetFullPath());

            ln = new LPath(@"\" + ln.NoPrefix);
            Assert.AreEqual(
                cd.GetPathRoot().CatName(ln.ToString()), 
                ln.GetFullPath());

            ln = new LPath(".");
            Assert.AreEqual(LDirectory.Current, ln.GetFullPath());
        }

        [Test]
        public void Parts()
        {
            var pCount = 40;
            var part = "0000000000";
            var ln = new LPath(Enumerable.Range(0, pCount).Select(x => part).Join(new string(System.IO.Path.DirectorySeparatorChar, 1)));
            var p = ln.Parts;
            Assert.AreEqual(pCount, p.Count());
            Assert.AreEqual(part, p[0]);
        }

        [Test]
        public void SpecialPaths()
        {
            for (Sidi.IO.LPath p = System.Environment.SystemDirectory; p != null; p = p.Parent)
            {
                Console.WriteLine(p);
                Assert.IsTrue(LDirectory.Exists(p));
                Console.WriteLine(p.Children.Join());
            }
        }

        [Test]
        public void UncPaths()
        {
            var tempDir = LPath.GetTempPath();
            Assert.IsTrue(tempDir.IsDirectory);

            log.Info(tempDir.DriveLetter);
            var unc = @"\\" + System.Environment.MachineName + @"\" + tempDir.DriveLetter + "$";
            Assert.IsTrue(System.IO.Directory.Exists(unc));

            var longNameUnc = new Sidi.IO.LPath(unc);
            Assert.IsTrue(longNameUnc.IsDirectory);
            Assert.IsTrue(longNameUnc.IsUnc);

            tempDir = longNameUnc.NoPrefix;
            Assert.IsTrue(System.IO.Directory.Exists(tempDir));

            for (var i = longNameUnc; i != null; i = i.Parent)
            {
                Console.WriteLine(i);
                Console.WriteLine(i.Parts.Join("|"));
                Console.WriteLine(i.Children.Join());
                Assert.IsTrue(i.IsDirectory);
            }
        }

        [Test]
        public void MakeFileName()
        {
            var validName = "I am a valid filename";
            var invalidName = System.IO.Path.GetInvalidFileNameChars().Join(" ");

            Assert.IsTrue(LPath.IsValidFilename(validName));
            Assert.IsFalse(LPath.IsValidFilename(invalidName));

            var f = LPath.GetValidFilename(invalidName);
            Assert.IsTrue(LPath.IsValidFilename(f));
            Assert.AreNotEqual(invalidName, f);
            Assert.AreEqual(System.IO.Path.GetInvalidFileNameChars().Select(c => "_").Join(" "), f);
            Assert.AreEqual(validName, LPath.GetValidFilename(validName));
            Assert.AreEqual("someName_", LPath.GetValidFilename("someName "));
            Assert.AreEqual("someName___", LPath.GetValidFilename("someName..."));
        }

        [Test]
        public void PathRoot()
        {
            var n = Sidi.IO.LPath.GetTempPath();
            Assert.AreEqual(System.IO.Path.GetPathRoot(n.NoPrefix), n.GetPathRoot().NoPrefix + @"\");
        }

        [Test]
        public void Relative()
        {
            var n = new LPath(@"C:\temp\abc.txt");
            var root = new LPath(@"C:\Temp");
            Assert.AreEqual(new LPath("abc.txt"), n.RelativeTo(root));
            Assert.AreEqual(new LPath(), n.RelativeTo(n));
        }

        [Test]
        public void IsValid()
        {
            Assert.IsTrue(LPath.IsValid(@"C:\temp"));
        }

        [Test]
        public void XmlSerialize()
        {
            var n = Sidi.IO.LPath.GetTempPath();
            var s = new XmlSerializer(typeof(LPath));
            var t = new System.IO.StringWriter();
            s.Serialize(t, n);
            log.Info(t.ToString());
            var n1 = s.Deserialize(new System.IO.StringReader(t.ToString()));
            Assert.AreEqual(n, n1);
        }

        [Test]
        public void StringCast()
        {
            var stringPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            LPath p = stringPath;
            Assert.AreEqual(new LPath(stringPath), p);
        }

        [Test]
        public void ParentChildren()
        {
            LPath p = TestFile(".");
            p = p.Canonic;
            Assert.IsTrue(p.Children.Any());
            foreach (var c in p.Children)
            {
                Assert.AreEqual(p, c.Parent);
            }
        }

        [Test]
        public void Sibling()
        {
            Assert.IsTrue(new Sidi.IO.LPath(@"a\b").Sibling("c").ToString().EndsWith(@"a\c"));
        }

        [Test]
        public void CatDir()
        {
            int someNum = 123;
            var p = LPath.Join(@"C:\temp", someNum);
            Assert.AreEqual(new LPath(@"C:\temp\123"), p);
            Assert.AreEqual(new LPath(@"C:\temp\123\dir"), LPath.Join(p, "dir"));
        }

        [Test]
        public void GetRelativePath()
        {
            Assert.AreEqual(new LPath("a.txt"), new LPath(@"d:\temp\a.txt").GetRelative(@"d:\temp"));
            Assert.AreEqual(new LPath(".."), new LPath(@"d:\temp").GetRelative(@"d:\temp\a.txt"));
        }

        [Test]
        public void HasExtension()
        {
            var p = new LPath("a.txt");
            Assert.IsTrue(p.HasExtension);
            p = p.ChangeExtension(null);
            Assert.IsFalse(p.HasExtension);
        }

        [Test]
        public void Absolute()
        {
            var rel = LPath.Join("a", "b", "c");
            Assert.IsFalse(rel.IsAbsolute);
            var abs = new LPath(@"C:\temp\something.txt");
            Assert.IsTrue(abs.IsAbsolute);

            var unc = new LPath(@"\\server\share\somedir\somefile");
            Assert.IsTrue(unc.IsAbsolute);
            Assert.IsTrue(unc.IsUnc);
            Assert.AreEqual(new LPath(@"\\server\share"), unc.GetPathRoot());
        }

        [Test]
        public void EnsureNotExists()
        {
            var tf = NewTestFile("file-to-delete");
            LFile.WriteAllText(tf, "hello");
            tf.EnsureFileNotExists();
            Assert.IsFalse(tf.Exists);
        }

        [Test, ExpectedException]
        public void EnsureNotExistsCannotDeleteDirectory()
        {
            var tf = NewTestFile("directory-to-delete");
            tf.EnsureDirectoryExists();
            tf.EnsureFileNotExists();
        }

        [Test]
        public void UniqueFileName()
        {
            var p = TestFile("someFile.jpg");

            var files = p.Parent.GetChildren(LPath.JoinFileName(p.FileNameWithoutExtension, "*"));
            log.Info(files.Join());
            foreach (var i in files)
            {
                i.EnsureNotExists();
            }

            Assert.AreEqual(p, p.Parent.CatDir(LPath.JoinFileName(p.FileNameParts)));
            p.EnsureNotExists();
            LFile.WriteAllText(p, "hello");
            for (int i = 0; i < 10; ++i)
            {
                var p1 = p.UniqueFileName();
                log.Info(p1);
                Assert.IsFalse(p1.Exists);
                LFile.WriteAllText(p1, "hello");
                Assert.IsTrue(p1.IsFile);
            }
        }

        [Test]
        public void Compare()
        {
            var p1 = new LPath("a");
            var p2 = new LPath("b");
            Assert.AreEqual(-1, p1.CompareTo(p2));
        }

        public class TestData
        {
            public TestData()
            {
                Path = new LPath("bla");
            }

            public LPath Path { set; get; }
            public bool Test { set; get; }
        }

        [Test]
        public void XmlSerialization()
        {
            var d = new TestData() { Path = Paths.BinDir, Test = true };
            var s = new XmlSerializer(typeof(TestData));
            var o = new StringWriter();
            s.Serialize(o, d);
            log.Info(o.ToString());
            var p1 = (TestData)s.Deserialize(new StringReader(o.ToString()));
            Assert.AreEqual(d.Path, p1.Path);
            Assert.AreEqual(d.Test, p1.Test);
        }

        [Test]
        public void Parse()
        {
            var p = new LPath(@"C:\temp\somefile");
            Assert.AreEqual(p, LPath.Parse(@"C:\temp\somefile"));
            Assert.AreEqual(p, LPath.Parse(@"C:/temp/somefile"));
            Assert.AreEqual(new LPath(@"a\b\c"), LPath.Parse("a/b/c"));
        }

        [Test]
        public void VolumePath()
        {
            var t = TestFile("test");
            log.Info(t.VolumePath);
        }
    }
}