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
            var hsv = new HSLColor(Color.Red);
            
            var t = TreeMapTest.GetTestTree();
            var tm = new TreeMapControl() { Tree = t };
            tm.CushionPainter.NodeColor = n => new HSLColor(hsv.Hue + ((int)n)*0.02, 1.0, 0.5);
            var lp = tm.CreateLabelPainter();
            tm.Paint += (s, e) => lp.Paint(e);
            lp.InteractMode = LabelPainter.Mode.MouseFocus;
            tm.RunFullScreen();
        }

        public void Display(Tree tree)
        {
            var c = new TreeMapControl() { Tree = tree };
            var f = c.AsForm("test Cushion Tree Map");
            System.Windows.Forms.Application.Run(f);
        }
    }
}
    