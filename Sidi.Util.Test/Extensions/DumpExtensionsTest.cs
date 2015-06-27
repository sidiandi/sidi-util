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
using System.Diagnostics;
using Sidi.Test;
using Sidi.Util;
using Sidi.IO;

namespace Sidi.Extensions
{
    [TestFixture]
    public class DumpExtensionsTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Test()
        {
            var files = TestFile(".").Info.GetFiles();
            var w = new StringWriter();
            files
                .ListFormat()
                .Add(f => f.Name, f => f.LastWriteTimeUtc, f => f.Length)
                .RenderText(w);
            log.Info(w.ToString());    
        }

        [Test]
        public void Log_dump()
        {
            Process p = Process.GetCurrentProcess();
            log.Info(() => this);
            log.Info(() => p);
            int a = 123;
            log.Info(() => a);
            log.Info(() => "Hello");
            log.Info(() => Paths.BinDir.Info);
        }

        [Test]
        public void DumpProperties_ListsAllItems()
        {
            var p = Process.GetProcesses();
            var count = Math.Min(p.Length, Dumper.Instance.MaxEnumElements);
            var s = Dumper.Instance.ToString(p);
            for (int i = 0; i < count; ++i)
            {
                Assert.That(s, Contains.Substring(String.Format("[{0}]", i)));
            }
        }

        [Test]
        public void DumpDictionary()
        {
            var d = Enumerable.Range(0, 20)
                .ToDictionary(x => Path.GetRandomFileName(), x => Path.GetRandomFileName());

            d.ListFormat()
                .AllPublic()
                .RenderText();
        }

        [Test]
        public void DumpGroup()
        {
            var d = Enumerable.Range(0, 200)
                .Select(x => Path.GetRandomFileName());

            var group = d.GroupBy(x => x.Substring(0, 1));

            group
                .OrderBy(x => x.Key)
                .ListCount()
                .RenderText();

            group
                .OrderByDescending(x => x.Count())
                .ListCountPercent()
                .RenderText();
        }
    }
}
