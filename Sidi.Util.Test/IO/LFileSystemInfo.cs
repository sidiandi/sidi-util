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
using NUnit.Framework;
using Sidi.Util;
using Sidi.Extensions;
using Sidi.Test;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Sidi.IO
{
    [TestFixture]
    public class FileSystemInfoTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        [Test]
        public void DriveRoot()
        {
            var drive = FileSystem.Current.CurrentDirectory.Root;
            Assert.IsTrue(drive.IsRoot);
            var c = drive.Info;
            Assert.IsTrue(c.Exists);
            Dumper.Instance.Write(Console.Out, c);
            log.Info(c.GetChildren().Join());
        }

        [Test]
        public void DotHandling()
        {
            var dot = new LPath(".");
            Assert.IsTrue(dot.Info.Exists);
        }

        [Test]
        public void Fullname()
        {
            var currentDirectory = new LPath(System.Environment.CurrentDirectory);
            var dot = new LPath(".");
            Dumper.Instance.Write(Console.Out, dot.Info);
            Assert.AreEqual(currentDirectory, dot.Info.FullName);
        }

        [Test]
        public void HardLinkSupport()
        {
            var testFile = TestFile("hardlinktest");
            testFile.EnsureNotExists();
            testFile.WriteAllText("hello");
            var testFile1 = testFile.CatName(".hl");
            testFile1.EnsureNotExists();
            testFile.CreateHardLink(testFile1);
            Assert.AreEqual(2, testFile.HardLinkInfo.FileLinkCount);

            var hl = (List<LPath>)testFile.HardLinkInfo.HardLinks;

            Assert.AreEqual(2, hl.Count);
            Assert.Contains(testFile, hl);
            Assert.Contains(testFile1, hl);

            log.Info(testFile.HardLinkInfo.FileIndex);
            Assert.AreEqual(testFile.HardLinkInfo.FileIndex, testFile1.HardLinkInfo.FileIndex);
        }

        static T TestSerialization<T>(T x)
        {
            var b = new BinaryFormatter();
            var m = new MemoryStream();
            b.Serialize(m, x);
            m.Seek(0, SeekOrigin.Begin);
            var x1 = (T) b.Deserialize(m);
            Assert.AreEqual(x, x1);
            return x1;
        }

        [Test]
        public void Serialize()
        {
            var p = TestFile("dir.txt");
            var info = p.Info;
            var infoSerialized = TestSerialization(info);
            Assert.AreEqual(info.FullName, infoSerialized.FullName);
        }

        [Test]
        public void SameAsSystemIoFileSystemInfo()
        {
            var testFile = TestFile("test.txt");
            testFile.EnsureFileNotExists();
            testFile.WriteAllText("hello");

            var system = new FileInfo(testFile.StringRepresentation);
            var sidi = testFile.Info;

            Assert.AreEqual(system.Attributes, sidi.Attributes);
            Assert.AreEqual(system.CreationTime, sidi.CreationTime);
            Assert.AreEqual(system.CreationTimeUtc, sidi.CreationTimeUtc);
            Assert.AreEqual(system.Exists, sidi.Exists);
            Assert.AreEqual(system.Extension, sidi.Extension);
            Assert.AreEqual(system.FullName, sidi.FullName.ToString());
            Assert.AreEqual(system.LastAccessTime, sidi.LastAccessTime);
            Assert.AreEqual(system.LastAccessTimeUtc, sidi.LastAccessTimeUtc);
            Assert.AreEqual(system.LastWriteTime, sidi.LastWriteTime);
            Assert.AreEqual(system.LastWriteTimeUtc, sidi.LastWriteTimeUtc);
            Assert.AreEqual(system.Name, sidi.Name);
            sidi.Delete();
            Assert.IsFalse(testFile.Exists);
        }
    }
}
