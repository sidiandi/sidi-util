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
using Sidi.Forms;
using System.Windows.Forms;
using System.Drawing;
using Sidi.IO;
using Sidi.Extensions;
using System.Diagnostics;
using System.Text.RegularExpressions;
using L = Sidi.IO;

namespace Sidi.Visualization
{
    [TestFixture]
    public class SimpleTreeMapTest : TestBase
    {
        [Test, Explicit("interactive")]
        public void Simple()
        {
            var file = TestFile(@"mail\message-1-1456.eml");
            var words = Regex.Split(LFile.ReadAllText(new Sidi.IO.LPath(file)), @"\s+");
            var st = new SimpleTreeMap();
            st.GroupBy = x =>
                {
                    var word = ((string)x);
                    var wordEnum = word.AggregateSelect(String.Empty, (s, c) => s + c);
                    return wordEnum.Cast<object>();
                };
            st.GetColor = x => Color.White;
            st.Items = words;
            st.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void Simple2()
        {
            var files = System.IO.File.ReadAllLines(TestFile("dir.txt"));
            var tm = new SimpleTreeMap();
            tm.Items = files.ToList();
            tm.GetDistinctColor = x => System.IO.Path.GetExtension((string)x);
            tm.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void Simple2ReverseOrder()
        {
            var files = System.IO.File.ReadAllLines(TestFile("dir.txt"));
            var tm = new SimpleTreeMap();
            tm.GetDistinctColor = x => System.IO.Path.GetExtension((string)x);
            tm.Items = files.ToList();
            tm.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void TypedVerySimple()
        {
            var files = System.IO.File.ReadAllLines(TestFile("dir.txt"));
            var tm = new TypedTreeMap<string>();
            tm.Items = files;
            tm.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void Generic()
        {
            var files = System.IO.File.ReadAllLines(TestFile("dir.txt")).Select(x => new L.LPath(x)).ToList();
            var tm = new TypedTreeMap<L.LPath>();
            tm.GetParent = x => x.Parent;
            tm.GetDistinctColor = x => x.Extension;
            tm.GetSize = x => x.Info.Length;
            tm.Activate = x => MessageBox.Show(x.ToString());
            tm.GetText = x => x.FileName;
            tm.Items = files;
            tm.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void ColorScale()
        {
            var files = System.IO.File.ReadAllLines(TestFile("dir.txt")).Select(x => new L.LPath(x)).ToList();
            var tm = new TypedTreeMap<L.LPath>()
            {
                GetParent = x => x.Parent,
                GetSize = x => x.Info.Length,
                Activate = x => MessageBox.Show(x.ToString()),
                Items = files,
                GetText = x => x.FileName,
            };

            // tm.SetPercentileColorScale(i => i.Info.LastWriteTime, Sidi.Visualization.ColorScale.GreenYellowRed());
            tm.SetPercentileColorScale(i => i.Info.LastWriteTime, Sidi.Visualization.ColorScale.GetColorScale(256, Color.Gray, Color.Red));

            tm.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void Empty()
        {
            var files = new string[] { };
            var tm = new SimpleTreeMap();
            tm.GetDistinctColor = x => System.IO.Path.GetExtension((string)x);
            tm.Items = files.ToList();
            tm.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void ProcessTree()
        {
            var p = Process.GetProcesses().ToList();
            var tm = new TypedTreeMap<Process>()
            {
                Items = p,
                GetLineage = i => new LPath(i.MainModule.FileName).Parts,
                GetSize = i => i.WorkingSet64,
                GetText = i => i.ProcessName,
            };

            tm.RunFullScreen();
        }

        [Test, Explicit("big")]
        public void Big()
        {
            var items = Enumerable.Range(0, 10000).Select(x => x.ToString("D8"));

            var tm = new TypedTreeMap<string>()
            {
                Items = items.ToList(),
                GetLineage = i => new string[]{"Base"},
            };

            tm.RunFullScreen();
        }

        IEnumerable<string> SplitStr(string x)
        {
            int partSize = 2;
            for (int i = 0; i < x.Length - partSize; i += partSize)
            {
                yield return x.Substring(i, partSize);
            }
        }
    }
}
