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
using Sidi.Test;

namespace Sidi.Util
{
    [TestFixture]
    public class DumperTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Process[] processArray = Process.GetProcesses();

        [Test]
        public void Dump()
        {
            var dump = new Dumper();
            dump.Write(Console.Out, processArray);
        }

        [Test]
        public void DumpADictionary()
        {
            var dump = new Dumper();
            dump.Write(Console.Out, processArray.ToDictionary(_ => _.Id));
        }

        [Test]
        public void DumpBinaryData()
        {
            byte[] data = ASCIIEncoding.ASCII.GetBytes(@"using NUnit.Framework;
using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
");

            var sw = new StringWriter();
            var dump = new Dumper();
            dump.Write(sw, data);
            StringAssert.EndsWith(@"000000A0 3B 0D 0A                                        ;..             
", sw.ToString());
            Console.Out.WriteLine(sw.ToString());
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
            var dump = new Dumper();
            dump.Write(Console.Out, a);
        }

        [Test]
        public void Path()
        {
            var d = new Dumper();
            var x = new Sidi.IO.LPath(@"C:\temp");
        }

        [Test()]
        public void ToStringTest()
        {
            var d = new Dumper();
            var a = "Hello";
            var dumpText = d.ToString(a);
            log.Info(() => dumpText);
            StringAssert.Contains(a, dumpText);
        }

        [Test()]
        public void TruncateLongEnumerables()
        {
            var d = new Dumper();
            var a = Enumerable.Range(0, 1000);
            var dumpText = d.ToString(a);
            StringAssert.DoesNotContain("[100]", dumpText);
            StringAssert.Contains("--- truncated after 100 elements ---", dumpText);
        }

        [Test()]
        public void ToStringTest1()
        {
            var d = new Dumper();
            var a = "Hello";
            var dumpText = d.ToString(() => a);
            log.Info(() => dumpText);
            StringAssert.Contains("a = ", dumpText);
        }

        class ClassWithPublicField
        {
            public string Greeting = "hello";
        }

        [Test]
        public void DumpPublicFields()
        {
            var dumpString = Dumper.Instance.ToString(new ClassWithPublicField());
            log.Info(() => dumpString);
            StringAssert.Contains("hello", dumpString);
        }
    }
}
