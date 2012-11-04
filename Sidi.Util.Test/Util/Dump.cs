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
using System.IO;
using NUnit.Framework;
using Sidi.Util;
using Sidi.Extensions;
using System.Diagnostics;

namespace Sidi.Tool
{
        [TestFixture]
        public class DumpTest
        {
            [Test]
            public void Dump()
            {
                var data = Process.GetProcesses();
                var dump = new Dump();
                dump.Write(data, Console.Out);
            }

            class A
            {
                public A next { set; get; }
            }
            

            [Test]
            public void Recursion()
            {
                var a = new A();
                a.next = a;
                var dump = new Dump();
                dump.Write(a, Console.Out);
            }

            [Test]
            public void Path()
            {
                var d = new Dump();
                var x = new Sidi.IO.LPath(@"C:\temp");
                d.Write(x, Console.Out);
            }
        }
}
