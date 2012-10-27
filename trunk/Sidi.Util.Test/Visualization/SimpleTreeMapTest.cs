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
using Sidi.IO.Long;
using System.Diagnostics;
using Sidi.IO.Long.Extensions;
using System.Text.RegularExpressions;
using L = Sidi.IO.Long;

namespace Sidi.Visualization
{
    [TestFixture]
    public class SimpleTreeMapTest : TestBase
    {
        [Test, Explicit("interactive")]
        public void Simple()
        {
            var file = TestFile(@"mail\message-1-1456.eml");
            var words = Regex.Split(File.ReadAllText(new Sidi.IO.Long.Path(file)), @"\s+");
            var st = new SimpleTreeMap();
            st.GetLineage = x =>
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
            var files = System.IO.File.ReadAllLines(TestFile("dir.txt")).Select(x => new L.Path(x)).ToList();
            var tm = new TypedTreeMap<L.Path>();
            tm.GetParent = x => x.Parent;
            tm.GetDistinctColor = x => x.Extension;
            tm.GetSize = x => x.Info.Length;
            tm.Activate = x => MessageBox.Show(x.ToString());
            tm.GetText = x => x.Name;
            tm.Items = files;
            tm.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void ColorMap()
        {
            var files = System.IO.File.ReadAllLines(TestFile("dir.txt")).Select(x => new L.Path(x)).ToList();
            var tm = new TypedTreeMap<L.Path>();
            tm.GetParent = x => x.Parent;
            tm.GetSize = x => x.Info.Length;
            tm.Activate = x => MessageBox.Show(x.ToString());
            tm.Items = files;
            tm.GetText = x => x.Name;
            tm.PercentileColorMap = x => x.Info.LastWriteTimeUtc;
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
    }
}
