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
using Sidi.Extensions;
using Sidi.Forms;
using System.Diagnostics;

namespace Sidi.Util
{
    [TestFixture]
    public class ListFormatTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        FileInfo[] data;

        public ListFormatTest()
        {
            data = new DirectoryInfo(TestFile(".")).GetFiles();
        }

        [Test]
        public void Test()
        {
            {
                data.ListFormat()
                    .AddColumn("Name", f => f.Name)
                    .AddColumn("Date", f => f.LastWriteTime)
                    .RenderText(Console.Out);
            }

            {
                data.ListFormat()
                    .Add("Name", "LastWriteTime")
                    .RenderText(Console.Out);
            }

            {
                data.ListFormat()
                    .AllPublic()
                    .RenderText(Console.Out);
            }

            {
                data.ListFormat()
                    .AllFields()
                    .AllProperties()
                    .RenderDetails(Console.Out);
            }
        }

        [Test]
        public void Width()
        {
            var lf = data.ListFormat()
                .AllPublic();

            foreach (var c in lf.Columns)
            {
                c.Width = 10;
                c.AutoWidth = false;
            }
            lf.RenderText(Console.Out);
        }

        [Test, Explicit]
        public void Chart()
        {
            var data = new DirectoryInfo(TestFile(".")).GetFiles();

            data.ListFormat()
                .AddColumn("Created", f => f.CreationTime)
                .AddColumn("LastModified", f => f.LastWriteTime)
                .AddColumn("Size", f => f.Length)
                .Chart().RunFullScreen();
        }

        [Test, Explicit]
        public void Bubbles()
        {
            var data = Process.GetProcesses();

            data.ListFormat()
                .AddColumn("Created", p => p.StartTime)
                .AddColumn("CPU", p => p.TotalProcessorTime.TotalHours)
                .AddColumn("Size", p => p.VirtualMemorySize64)
                .Bubbles().RunFullScreen();
        }

        [Test, Explicit]
        public void ProcessChart()
        {
            var data = Process.GetProcesses();
            data.ListFormat()
                .AddColumn("Name", f => f.ToString())
                .Property("Id")
                .Chart().RunFullScreen();
        }
    }
}
