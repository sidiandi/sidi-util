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
            var c = new LFileSystemInfo(drive);
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
    }
}
