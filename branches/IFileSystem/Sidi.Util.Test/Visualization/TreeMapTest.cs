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
using System.Diagnostics;
using System.Text.RegularExpressions;
using Sidi.Test;

namespace Sidi.Visualization
{
    [TestFixture]
    public class TreeMapTest : TestBase
    {
        [Test, Explicit("interactive")]
        public void TestTreeDisplay()
        {
            var t = TreeMapTestData.GetTestTree();
            var tm = TreeMap.FromTree(t);
            tm.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void TestTreeWithColorDisplay()
        {
            var hsv = new HSLColor(Color.Red);
            
            var t = TreeMapTestData.GetTestTree();
            var tm = new TreeMap() { Tree = t };
            tm.CushionPainter.GetColor = n => new HSLColor(hsv.Hue + ((int)n)*0.02, 1.0, 0.5);
            var lp = tm.CreateLabelPainter();
            tm.Paint += (s, e) => lp.Paint(e);
            lp.InteractMode = InteractionMode.MouseFocus;
            tm.RunFullScreen();
        }

        [Test]
        public void DivideAndConquer()
        {
            var c = new Sidi.Visualization.LayoutManager.LayoutContext()
            {
                Bounds = new Bounds(0, 0, 1, 1),
                Layout = Enumerable.Range(0, 100).Select(x => new Layout(null) { Tree = new Tree(null) { Size = 1 } }).ToArray(),
            };
                
            LayoutManager.DivideAndConquer(c);

            /*
            foreach (var l in c.Layout)
            {
                Console.WriteLine("{0}: {1}", l.Bounds.Area, l.Bounds);
            }
            */
        }

        public void Display(Tree tree)
        {
            var c = new TreeMap() { Tree = tree };
            var f = c.AsForm("test Cushion Tree Map");
            System.Windows.Forms.Application.Run(f);
        }
    }
}
    
