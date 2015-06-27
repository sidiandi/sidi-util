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
using Sidi.Parse;

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
            Assert.AreEqual("server", p.Server);
            Assert.AreEqual("share", p.Share);
            Assert.AreEqual(@"\\server\share\a\b\c", p.ToString());

            p = new LPath(@"\\server\share\");
            Assert.AreEqual("server", p.Server);
            Assert.AreEqual("share", p.Share);
            Assert.AreEqual(@"\\server\share\", p.ToString());

            p = new LPath(@"C:");
            Assert.AreEqual("C", p.DriveLetter);
            Assert.AreEqual(@"C:\", p.Prefix);
            Assert.IsFalse(p.IsRelative);
            Assert.AreEqual(0, p.Parts.Count());
            p = p.CatDir("someDir");
            Assert.AreEqual(new LPath(@"C:\someDir"), p);
        }

        [TestCase(@":", ExpectedException = typeof(ArgumentOutOfRangeException))]
        [TestCase(@"asda:\", ExpectedException = typeof(ArgumentOutOfRangeException))]
        [TestCase(@"_:", ExpectedException = typeof(ArgumentOutOfRangeException))]
        public void CtorEx(string pathSpec)
        {
            var p = new LPath(pathSpec);
            log.Info(p.Prefix);
            log.Info(p.Parts);
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
            Assert.AreEqual(cd.ToString(), new System.IO.DirectoryInfo(cd.StringRepresentation).FullName);
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
            Assert.AreEqual(null, FileSystem.Current.GetDrives().First().Parent);
        }

        [Test]
        public void SpecialPaths()
        {
            foreach (var p in new LPath(System.Environment.SystemDirectory).Lineage)
            {
                Console.WriteLine(p);
                Assert.IsTrue(p.IsDirectory);
            }
        }

        [Test]
        public void UncPaths()
        {
            var tempDir = LPath.GetTempPath();
            Assert.IsTrue(tempDir.IsDirectory);

            var server = System.Environment.MachineName;
            var share = tempDir.DriveLetter + "$";
            var unc = tempDir.GetAdministrativeShareUnc();

            Assert.IsTrue(unc.IsDirectory);
            Assert.IsTrue(unc.IsUnc);

            Assert.AreEqual(server, unc.Server);
            Assert.AreEqual(share, unc.Share);
            Assert.IsNull(unc.DriveLetter);

            Assert.IsNull(unc.Root.Parent);
            Assert.IsTrue(unc.Children.Any());
        }

        [Test]
        public void GetValidFilename()
        {
            var validName = "I am a valid filename";
            var invalidName = System.IO.Path.GetInvalidFileNameChars().Join(" ");

            Assert.IsTrue(LPath.IsValidFilename(validName));
            Assert.IsFalse(LPath.IsValidFilename(invalidName));

            var f = LPath.GetValidFilename(invalidName);
            Assert.IsTrue(LPath.IsValidFilename(f), f.ToString());
            Assert.AreNotEqual(invalidName, f);
            Assert.AreEqual(System.IO.Path.GetInvalidFileNameChars().Select(c => "_").Join(" "), f);
            Assert.AreEqual(validName, LPath.GetValidFilename(validName));
            var fn = LPath.GetValidFilename(new string(Enumerable.Range(0, 4096).Select(_ => (char)_).ToArray()));
            Assert.IsTrue(LPath.IsValidFilename(fn));
        }

        [Test]
        public void PathRoot()
        {
            var n = Sidi.IO.LPath.GetTempPath();
            Assert.AreEqual(System.IO.Path.GetPathRoot(n.StringRepresentation), n.Root.ToString());
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
        public void valid_paths_are_recognized_as_valid([Values(
            @"C:",
            @"C:\",
            @"C:\temp",
            @"\\server\share",
            @"\\server\share\",
            @"\\server\share\somedir",
            @"\\?\C:\"
            )] string path)
        {
            Assert.IsTrue(LPath.IsValid(path), path);
        }

        [Test]
        public void invalid_paths_are_recognized_as_invalid([Values(
            @":C:\",
            @"\\\server\share"
            )] string path)
        {
            Assert.IsFalse(LPath.IsValid(path), path);
        }

        [Test]
        public void paths_with_invalid_characters_are_recognized_as_invalid()
        {
            foreach (var c in System.IO.Path.GetInvalidPathChars())
            {
                var path = new string(c, 1);
                Assert.IsFalse(LPath.IsValid(path), path);
            }
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
            var p = new Sidi.IO.LPath(@"a\b");
            var s = p.Sibling("c");
            Assert.AreEqual(p.Parent, s.Parent);
            Assert.AreEqual(new LPath(@"a\c"), s);
        }

        [Test]
        public void Sibling2()
        {
            var p = new Sidi.IO.LPath(@"a");
            var s = p.Sibling("c");
            Assert.AreEqual(p.Parent, s.Parent);
            Assert.AreEqual(new LPath(@"c"), s);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Sibling3()
        {
            var p = new Sidi.IO.LPath(@"");
            var s = p.Sibling("c");
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

#pragma warning disable 618

        [Test]
        public void Absolute()
        {
            var relative = new LPath(@"a\b\c");
            Assert.IsTrue(relative.IsRelative);
            Assert.AreNotEqual(relative, relative.GetFullPath());

            var abs = new LPath(@"C:\temp\something.txt");
            Assert.IsFalse(abs.IsRelative);
            Assert.AreEqual(new LPath(@"C:\"), abs.Root);
            Assert.AreEqual(abs, abs.GetFullPath());

            var unc = new LPath(@"\\server\share\somedir\somefile");
            Assert.IsFalse(unc.IsRelative);
            Assert.IsTrue(unc.IsUnc);
            Assert.AreEqual(new LPath(@"\\server\share\"), unc.Root);
            Assert.AreEqual(unc, unc.GetFullPath());

            var abs2 = new LPath(@"\a\b\c");
            Assert.IsFalse(abs2.IsRelative);
            Assert.IsTrue(abs2.IsRootRelative);
            Assert.AreNotEqual(abs2, abs2.GetFullPath());
        }

#pragma warning restore 618
        
        [Test]
        public void EnsureFileNotExists()
        {
            var tf = NewTestFile("file-to-delete");
            tf.WriteAllText("hello");
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

            var files = p.Parent.GetChildren(LPath.JoinFileName(p.FileNameWithoutExtension, "*").StringRepresentation);
            log.Info(files.Join());
            foreach (var i in files)
            {
                i.EnsureNotExists();
            }

            Assert.AreEqual(p, p.Parent.CatDir(LPath.JoinFileName(p.FileNameParts)));
            p.EnsureNotExists();
            p.WriteAllText("hello");
            for (int i = 0; i < 10; ++i)
            {
                var p1 = p.UniqueFileName();
                log.Info(p1);
                Assert.IsFalse(p1.Exists);
                p1.WriteAllText("hello");
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

        [Test]
        public void IsDescendantOrSelf()
        {
            var a = new LPath(@"a\b\c");
            var b = new LPath(@"a\b");

            Assert.IsTrue(a.IsDescendantOrSelf(b));
            Assert.IsTrue(a.IsDescendant(b));
            
            Assert.IsTrue(a.IsDescendantOrSelf(a));
            Assert.IsFalse(a.IsDescendant(a));

            Assert.IsFalse(b.IsDescendantOrSelf(a));
            Assert.IsFalse(b.IsDescendant(a));
        }

        [Test]
        public void DriveRoot()
        {
            var r = LPath.GetDriveRoot('x');
            Assert.IsTrue(r.IsDriveRoot);
        }

        [Test]
        public void AbsolutePaths()
        {
            var p = new LPath(@"C:\");
            Assert.IsTrue(p.IsAbsolute);
            Assert.IsFalse(p.IsRelative);

            p = new LPath(@"\\server\share\someFile.txt");
            Assert.IsTrue(p.IsAbsolute);
            Assert.IsFalse(p.IsRelative);

            p = new LPath(@"1\2\3");
            Assert.IsFalse(p.IsAbsolute);
            Assert.IsTrue(p.IsRelative);
        }
    }
}
