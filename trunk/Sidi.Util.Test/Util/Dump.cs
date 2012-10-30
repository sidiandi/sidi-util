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
                var x = new Sidi.IO.Path(@"C:\temp");
                d.Write(x, Console.Out);
            }
        }
}
