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
using System.Diagnostics;

namespace Sidi.IO
{
    [TestFixture]
    public class LPathTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Ctor()
        {
            var p = new LPath(@"\some\root\relative\path");
            Assert.AreEqual(@"\some\root\relative\path", p.ToString());

            p = new LPath(@"\\server\share\a\b\c");
            Assert.AreEqual(@"\\server\share\a\b\c", p.ToString());
        }

        [Test, ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void Check()
        {
            var ln = new LPath(Enumerable.Range(0, 4000).Select(x => "0000000000").Join(new string(System.IO.Path.DirectorySeparatorChar, 1)));
        }

        [Test, ExpectedException(ExpectedException = typeof(System.ArgumentOutOfRangeException))]
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
            TestSerialize(Paths.BinDir);
            TestSerialize<LPath>(null);
        }

        [Test]
        public void FullPath()
        {
            var ln = LPath.CreateRelative(Enumerable.Range(0, 100).Select(x => "0000000000").ToArray());
            var cd = new LPath(System.Environment.CurrentDirectory);
            Assert.AreEqual(cd.CatDir(ln), ln.GetFullPath());
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
        public void Parent()
        {
            Assert.AreEqual(new LPath(""), new LPath(@"a").Parent);
            Assert.AreEqual(new LPath(@"a"), new LPath(@"a\b").Parent);
        }

        [Test]
        public void SpecialPaths()
        {
            foreach (var p in new LPath(System.Environment.SystemDirectory).Lineage)
            {
                Console.WriteLine(p);
                Assert.IsTrue(LDirectory.Exists(p));
            }
        }

        [Test]
        public void UncPaths()
        {
            var tempDir = LPath.GetTempPath();
            Assert.IsTrue(tempDir.IsDirectory);

            log.Info(tempDir.DriveLetter);
            var unc = LPath.GetUncRoot(System.Environment.MachineName, tempDir.DriveLetter + "$");
            log.Info(unc);
            Assert.IsTrue(System.IO.Directory.Exists(unc));

            var longNameUnc = new Sidi.IO.LPath(unc);
            log.Info(longNameUnc);
            Assert.IsTrue(longNameUnc.IsDirectory);
            Assert.IsTrue(longNameUnc.IsUnc);

            tempDir = longNameUnc;
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
        public void GetValidFilename()
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
            var fn = LPath.GetValidFilename(new string(Enumerable.Range(0, 4096).Select(_ => (char)_).ToArray()));
            log.Info(fn);
            Assert.IsTrue(LPath.IsValidFilename(fn));

        }

        [Test]
        public void PathRoot()
        {
            var n = Sidi.IO.LPath.GetTempPath();
            Assert.AreEqual(System.IO.Path.GetPathRoot(n), n.GetPathRoot().ToString());
        }

        [Test]
        public void Relative()
        {
            var n = new LPath(@"C:\temp\abc.txt");
            var root = new LPath(@"C:\Temp");
            Assert.AreEqual(new LPath("abc.txt"), n.RelativeTo(root));
            Assert.AreEqual(new LPath(@"..\abc.txt"), n.RelativeTo(n));
        }

        [Test]
        public void IsValid()
        {
            Assert.IsTrue(LPath.IsValid(@"C:\temp"));
        }

        public class ObjectGraph
        {
            [XmlElement]
            public LPath Path;

            public override bool Equals(object obj)
            {
                return object.Equals(Path, ((ObjectGraph)obj).Path);
            }

            public override int GetHashCode()
            {
                return Path == null ? 0 : Path.GetHashCode();
            }
        }

        [Test]
        public void XmlSerialize()
        {
            TestXmlSerialize(Sidi.IO.LPath.GetTempPath());

            // passes - string null values are handled properly
            TestXmlSerialize<string>(null);
            // would fail - IXmlSerializable cannot handle null values properly
            // TestXmlSerialize<LPath>(null);

            TestXmlSerialize(new ObjectGraph());
        }

        [Test, Explicit("todo")]
        public void XmlSerializeNull()
        {
            TestXmlSerialize<LPath>(null);
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
            var p = new LPath(@"C:\temp").CatDir(someNum.ToString());
            Assert.AreEqual(new LPath(@"C:\temp\123"), p);
            Assert.AreEqual(new LPath(@"C:\temp\123\dir"), p.CatDir("dir"));
        }

        static string GetFileNameWhichIsTooLong()
        {
            return new string('a', 512);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CatDirLong()
        {
            var p = TestFile("root");
            p.CatDir(GetFileNameWhichIsTooLong());
        }

        [Test]
        public void CatDirWildCards()
        {
            var p = TestFile("root").CatDir("*.*");
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

        LPath[] examplePaths = new LPath[]
        {
            new LPath(@"a\b\c"),
            new LPath(@"C:\temp\something.txt"),
            new LPath(@"\\server\share\somedir\somefile"),
            new LPath(@"\a\b\c")
        };

        [Test]
        public void GetFullPath()
        {
            foreach (var p in examplePaths)
            {
                Assert.IsTrue(p.IsFullPath ^ !object.Equals(p, p.GetFullPath()), p);
            }
        }

#pragma warning disable 618

        [Test]
        public void Absolute()
        {
            var relative = new LPath(@"a\b\c");
            Assert.IsFalse(relative.IsAbsolute);
            Assert.AreNotEqual(relative, relative.GetFullPath());

            var abs = new LPath(@"C:\temp\something.txt");
            Assert.IsTrue(abs.IsAbsolute);
            Assert.AreEqual(new LPath(@"C:\"), abs.GetPathRoot());
            Assert.AreEqual(abs, abs.GetFullPath());

            var unc = new LPath(@"\\server\share\somedir\somefile");
            Assert.IsTrue(unc.IsAbsolute);
            Assert.IsTrue(unc.IsUnc);
            Assert.AreEqual(new LPath(@"\\server\share\"), unc.GetPathRoot());
            Assert.AreEqual(unc, unc.GetFullPath());

            var abs2 = new LPath(@"\a\b\c");
            Assert.IsFalse(abs2.IsAbsolute);
            Assert.AreNotEqual(abs2, abs2.GetFullPath());
        }

#pragma warning restore 618
        
        [Test]
        public void EnsureFileNotExists()
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
            tf.EnsureNotExists();
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
            Assert.AreEqual(new LPath(@"\\somehost.com\path1\path2"), LPath.Parse(@"file://somehost.com/path1/path2"));
        }

        [Test, RequiresSTA, Explicit]
        public void ParseClipboard()
        {
            var paths = new PathList(new []{ new LPath(@"C:\temp\somefile") });
            paths.WriteClipboard();
            var p = LPath.Parse(":paste");
            Assert.AreEqual(paths.First(), p);
        }

        [Test]
        public void VolumePath()
        {
            var t = TestFile("test");
            log.Info(t.VolumePath);
        }

        [Test]
        public void DriveRootsExist()
        {
            Assert.IsFalse(new LPath(@"a:\").Exists);
            var drives = DriveInfo.GetDrives();
            var allDrives = Enumerable.Range('a', 'z' - 'a').Select(x => new String((char)x, 1))
                .Select(x => new
                    {
                        Letter = x,
                        Exists = drives.Any(d => d.RootDirectory.FullName.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)),
                    });

            foreach (var i in allDrives)
            {
                if (new DriveInfo(i.Letter).DriveType != DriveType.CDRom)
                {
                    Assert.AreEqual(i.Exists, new LPath(i.Letter + @":\").IsDirectory, i.Letter);
                }
            }
        }

        [Test]
        public void ToUri()
        {
            Assert.AreEqual(
                new Uri("file://C:/temp/doc"),
                new LPath(@"C:\temp\doc").Uri);

            Assert.AreEqual(
                new Uri("file://server/share/doc.txt"),
                new LPath(@"\\server\share\doc.txt").Uri);

            Assert.AreEqual(
                new Uri("file://C:/"),
                new LPath(@"C:\").Uri);
        }

        [Test]
        public void Equals()
        {
            var x = Paths.Temp;
            Assert.IsTrue(x.Equals(x));
            var y = Paths.Temp;
            Assert.AreEqual(x.Equals(y), y.Equals(x));
            y = Paths.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            Assert.AreEqual(x.Equals(y), y.Equals(x));
            Assert.IsFalse(x.Equals(null));

            Assert.IsFalse(object.Equals(null, x));

            Assert.AreEqual(new LPath("a"), new LPath(@"A"));
        }

        [Test]
        public void GetChildren()
        {
            var dir = Paths.GetFolderPath(Environment.SpecialFolder.System);

            var c = dir.GetChildren();
            Assert.IsTrue(c.All(_ => _.Exists));
            Assert.IsTrue(c.All(_ => _.Parent.Equals(dir)));

            c = dir.GetChildren("*.dll");
            Assert.IsTrue(c.All(_ => _.Exists));

            c = dir.GetFiles();
            Assert.IsTrue(c.All(_ => _.IsFile));

            // GetChildren should return an empty list for files
            Assert.AreEqual(0, c.First().GetChildren().Count);

            c = dir.GetFiles("*.dll");
            Assert.IsTrue(c.All(_ => _.IsFile));

            c = dir.GetDirectories();
            Assert.IsTrue(c.All(_ => _.IsDirectory));

            c = dir.GetDirectories("de*");
            Assert.IsTrue(c.All(_ => _.IsDirectory));
        }

        [Test]
        public void GetChildrenNoException()
        {
            var f = Paths.Temp.CatDir(LPath.GetRandomFileName());
            Assert.IsFalse(f.Exists);
            f.GetChildren();
        }
    }
}
