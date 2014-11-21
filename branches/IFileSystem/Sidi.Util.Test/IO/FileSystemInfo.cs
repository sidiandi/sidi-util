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
            var drive = LDirectory.Current.GetPathRoot();
            Assert.IsTrue(drive.IsRoot);
            var c = drive.Info;
            Assert.IsTrue(c.Exists);
            c.DumpProperties(Console.Out);
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
            dot.Info.DumpProperties(Console.Out);
            Assert.AreEqual(currentDirectory, dot.Info.FullName);
        }

        [Test]
        public void HardLinkSupport()
        {
            var testFile = TestFile("hardlinktest");
            testFile.EnsureNotExists();
            testFile.EnsureParentDirectoryExists();
            LFile.WriteAllText(testFile, "hello");
            var testFile1 = testFile.CatName(".hl");
            testFile1.EnsureNotExists();
            LFile.CreateHardLink(testFile1, testFile);
            Assert.AreEqual(2, testFile.Info.FileLinkCount);

            var hl = (List<LPath>) testFile.Info.HardLinks;

            Assert.AreEqual(2, hl.Count);
            Assert.Contains(testFile, hl);
            Assert.Contains(testFile1, hl);

            log.Info(testFile.Info.FileIndex);
            Assert.AreEqual(testFile.Info.FileIndex, testFile1.Info.FileIndex);
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
            LFile.WriteAllText(p, "changed");
            var newInfo = p.Info;
            Assert.AreNotEqual(infoSerialized.LastWriteTimeUtc, newInfo.LastWriteTimeUtc);
            Assert.AreNotEqual(infoSerialized, newInfo);
        }
    }
}
