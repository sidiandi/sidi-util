using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Forms;
using System.Windows.Forms;
using System.Drawing;
using Sidi.IO;
using Sidi.IO.Long;
using System.Diagnostics;
using Sidi.IO.Long.Extensions;
using System.Text.RegularExpressions;
using Sidi.Forms;

namespace Sidi.Visualization
{
    [TestFixture]
    public class SimpleTreeMapControlTest : TestBase
    {
        [Test, Explicit("interactive")]
        public void Simple()
        {
            var file = TestFile(@"mail\message-1-1456.eml");
            var words = Regex.Split(File.ReadAllText(new Sidi.IO.Long.Path(file)), @"\s+");
            var st = new SimpleTreeMap();
            st.Lineage = x => ((string)x).Cast<object>();
            st.Color = x => new HSLColor((double)(char)x/100.0, 1.0, 0.5);
            st.Items = words;
            st.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void Simple2()
        {
            var files = System.IO.File.ReadAllLines(TestFile("dir.txt"));
            var tm = new SimpleTreeMap();
            tm.Items = files.ToList();
            tm.DistinctColor = x => System.IO.Path.GetExtension((string)x);
            tm.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void Simple2ReverseOrder()
        {
            var files = System.IO.File.ReadAllLines(TestFile("dir.txt"));
            var tm = new SimpleTreeMap();
            tm.DistinctColor = x => System.IO.Path.GetExtension((string)x);
            tm.Items = files.ToList();
            tm.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void Empty()
        {
            var files = new string[] { };
            var tm = new SimpleTreeMap();
            tm.DistinctColor = x => System.IO.Path.GetExtension((string)x);
            tm.Items = files.ToList();
            tm.RunFullScreen();
        }
    }
}
