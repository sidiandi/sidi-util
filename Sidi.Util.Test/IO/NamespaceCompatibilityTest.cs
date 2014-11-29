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
using System.IO;
using Sidi.IO;

#pragma warning disable 219

namespace Sidi.Test
{
    /// <summary>
    /// Checks if Sidi.IO names do not collide with System.IO names
    /// </summary>
    [TestFixture]
    public class NamespaceCompatibilityTest
    {
        /// <summary>
        /// Test names of static classes
        /// </summary>
        [Test]
        public void TestNames()
        {
            Console.WriteLine(Paths.BinDir);
            Copy copy = null;
            CopyOp copyOp = null;
            CopyProgress copyProgress = null;
            FileSystemInfo fsInfo = null;
            FileType fileType = null;
            Find find = null;
            FindConfig findConfig = null;
            LPath lPath = null;
            PathList pathList = null;
            SearchPath searchPath = null;
        }
    }
}
