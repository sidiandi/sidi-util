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
using Sidi.CommandLine;
using cm = System.ComponentModel;
using System.IO;
using Sidi.Util;
using System.Linq;

namespace Sidi.CommandLine
{
    [TestFixture]
    public class SubCommandTest : TestBase
    {
        [Usage("Test app")]
        public class App
        {
            [SubCommand]
            public Math Math;
        }

        [Usage("Mathematical operations")]
        public class Math
        {
            [Usage("adds a and b")]
            public int Add(int a, int b)
            {
                return a + b;
            }
            [Usage("multiply a and b")]
            public int Multiply(int a, int b)
            {
                return a * b;
            }
        }

        [Test]
        public void Sub()
        {
            var app = new App();
            var p = new Parser(app);
            p.Parse(new string[] { "Math", "Add", "1", "1", "Multiply", "10", "10"});
        }
    }
}
