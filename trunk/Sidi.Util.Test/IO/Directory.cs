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
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace Sidi.IO
{
    [TestFixture]
    public class DirectoryTest
    {
        [Test]
        public void Exists()
        {
            var p = new LPath(String.Format(@"\\{0}\C$", System.Environment.MachineName));
            Assert.IsTrue(System.IO.Directory.Exists(p.NoPrefix)); 
            Assert.IsTrue(LDirectory.Exists(p));
        }
    }
}
