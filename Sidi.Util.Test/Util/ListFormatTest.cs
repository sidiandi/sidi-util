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
using Sidi.Test;
using Sidi.IO;
using Sidi.Net;
using Sidi.Util;

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

        public class Example
        {
            public string Name { set; get; }
        }

        [Test]
        public void GuessColumnNames()
        {
            var lf = data.ListFormat(_ => _.FullName, _=> _.Extension, _ => _.ToString());
            Assert.AreEqual("FullName", lf.Columns[0].Name);
            Assert.AreEqual("Extension", lf.Columns[1].Name);
            Assert.AreEqual("ToString", lf.Columns[2].Name);
        }

        [Test]
        public void ToStringTest()
        {
            var s = data.ListFormat().ToString();
            Assert.IsTrue(s.StartsWith("\r\n"));
            Assert.IsTrue(s.EndsWith("\r\n"));
            log.Info(s);
        }

        [Test]
        public void DefaultColumns()
        {
            var data = Process.GetProcesses();
            log.Info(data.ListFormat());
        }

        [Test]
        public void DefaultColumns2()
        {
            var data = Enumerable.Range(0, 10).Select(_ => new { D = Enumerable.Range(0, 10) });
            log.Info(data.ListFormat().AllPublic());
        }

        [Test]
        public void DefaultColumns3()
        {
            var data = Enumerable.Range(0, 10).Select(_ => new { A = Path.GetRandomFileName(), B = Path.GetRandomFileName(), C = Path.GetRandomFileName() });
            var lf = data.ListFormat();
            log.Info(lf);
            Assert.AreEqual("#", lf.Columns[0].Name);
            Assert.AreEqual("A", lf.Columns[1].Name);
        }

        [Test]
        public void DefaultColumnsDictionary()
        {
            var d = Enumerable.Range(1, 100).ToDictionary(_ => LPath.GetRandomFileName(), _ => LPath.GetRandomFileName());
            var lf = d.ListFormat();
            log.Info(lf);
            Assert.AreEqual("#", lf.Columns[0].Name);
            Assert.AreEqual("Key", lf.Columns[1].Name);
            Assert.AreEqual("Value", lf.Columns[2].Name);
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
            data
                .ListFormat()
                .AddColumn("Name", f => f.ToString())
                .Property("Id")
                .Chart().RunFullScreen();
        }

        [Test, Explicit]
        public void AsHtml()
        {
            var data = Process.GetProcesses();
            var list = data
                .ListFormat()
                .AllPublic()
                .AddColumn("Details", Dumper.Instance.ToString);

            list.Columns.Last().Tag<HtmlCellFormat<Process>>().HasDedicatedRow = true;
            list["Threads"].GetHtmlCellFormat().HasDedicatedRow = true;
                

            var h = new HtmlGenerator();

            var f = list.Columns[0].Tag<HtmlCellFormat<Process>>();
            var oldF = f.Format;
            f.Format = (r, i, c) => h.a(h.href("http://www.spiegel.de"), oldF(r,i,c));

            var page = h.html
            (
                h.head(h.TableStyle()),
                h.body
                (
                    h.Table(list)
                )
            );

            HtmlPage.Show(page);
        }

        [Test, Explicit]
        public void AsHtmlDetails()
        {
            var data = Process.GetProcesses();
            var list = data
                .ListFormat()
                .AllPublic()
                .AddColumn("Details", x => Dumper.Instance.ToString(x));

            list.Columns.Last().Tag<HtmlCellFormat<Process>>().HasDedicatedRow = true;
            list["Threads"].GetHtmlCellFormat().HasDedicatedRow = true;


            var h = new HtmlGenerator();

            var f = list.Columns[0].Tag<HtmlCellFormat<Process>>();
            var oldF = f.Format;
            f.Format = (r, i, c) => h.a(h.href("http://www.spiegel.de"), oldF(r, i, c));

            var page = h.html
            (
                h.head(h.TableStyle()),
                h.body
                (
                    h.DetailsTable(list)
                )
            );

            HtmlPage.Show(page);
        }
    }
}
