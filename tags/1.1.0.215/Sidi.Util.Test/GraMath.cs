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

#region "Mandatory NUnit Imports"
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using NUnit.Framework;
#endregion

//Test Specific Imports
//TODO - Add imports your going to test here
using Sidi.Util;

namespace Sidi.Util
{
    [TestFixture]
    public class GraMathTest : TestBase
    {
        [Test()]
        public void WrapAround()
        {
            Assert.AreEqual(0, GraMath.WrapAround(0, 1));
            Assert.AreEqual(0, GraMath.WrapAround(2, 1));
            Assert.AreEqual(4, GraMath.WrapAround(-1, 5));
            Assert.AreEqual(3, GraMath.WrapAround(-2, 5));
            Assert.AreEqual(0, GraMath.WrapAround(-1, 1));
            Assert.AreEqual(12, GraMath.WrapAround(-1, 13));
            Assert.AreEqual(0, GraMath.WrapAround(-13, 13));
        }
    }

}
