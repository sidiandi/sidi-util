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
using Sidi.IO;
using System.IO;

namespace Sidi.IO
{
    [TestFixture]
    public class PathExTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void GetRelativePath()
        {
            Assert.AreEqual("a.txt", PathExtensions.GetRelativePath(@"d:\temp\a.txt", @"d:\temp"));
            Assert.AreEqual("..", PathExtensions.GetRelativePath(@"d:\temp", @"d:\temp\a.txt"));
        }

        [Test]
        public void Find()
        {
            foreach (var i in PathExtensions.Find(TestFile(".")))
            {
                log.Info(i);
                Assert.IsTrue(File.Exists(i) || Directory.Exists(i));
            }
        }
    }
}
