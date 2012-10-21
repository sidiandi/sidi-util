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

namespace Sidi.Visualization
{
    [TestFixture]
    public class TreeMapControlTest : TestBase
    {
        [Test, Explicit("interactive")]
        public void TestTreeDisplay()
        {
            var t = TreeMapTest.GetTestTree();
            var tm = TreeMapControl.FromTree(t);
            tm.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void TestTreeWithColorDisplay()
        {
            var t = TreeMapTest.GetTestTree();
            var tm = new TreeMapControl() { Tree = t };
            tm.CushionPainter.NodeColor = n => new HSLColor(((int)n) * 36.0f, 50, 120);
            tm.RunFullScreen();
        }

        /*
        [Test, Explicit("interactive")]
        public void Simple()
        {
            var file = TestFile(@"mail\message-1-1456.eml");
            var words = Regex.Split(File.ReadAllText(new Sidi.IO.Long.Path(file)), @"\s+");
            var st = new SimpleTreeMap();
            st.Items = words.Select(x => new SimpleTreeMap.Item()
                {
                    Lineage = x.Cast<object>().ToArray(),
                    Size = 1.0f,
                    Color = Color.White,
                });
            st.Show();
        }

        [Test, Explicit("interactive")] 
        public void Simple2()
        {
            var file = new Sidi.IO.Long.Path(TestFile(@".")).Parent.Parent;
            var files = Sidi.IO.Long.FileEnum.AllFiles(file);

            var data = files.Select(x => new SimpleTreeMap.ColorMapItem()
            {
                Lineage = x.FullName.Parts.ToArray(),
                Size = x.Length,
                Color = x.Extension.ToLower(),
            });

            SimpleTreeMap.Show(SimpleTreeMap.ToItems(data));
        }

        [Test, Explicit("interactive")]
        public void Simple3()
        {
            var file = new Sidi.IO.Long.Path(@"C:\work\lib");
            var files = Sidi.IO.Long.FileEnum.AllFiles(file);

            var data = files.Select(x => new SimpleTreeMap.LinearColorMapItem()
            {
                Lineage = x.FullName.Parts.ToArray(),
                Size = x.Length,
                Color = x.LastWriteTime.Ticks,
            });

            SimpleTreeMap.Show(SimpleTreeMap.ToItems(data));
        }
        */

        public void Display(Tree tree)
        {
            var c = new TreeMapControl() { Tree = tree };
            var f = c.AsForm("test Cushion Tree Map");
            System.Windows.Forms.Application.Run(f);
        }
    }
}
    