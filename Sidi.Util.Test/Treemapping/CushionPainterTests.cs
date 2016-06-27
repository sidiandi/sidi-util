using NUnit.Framework;
using Sidi.Treemapping;
using Sidi.Treemapping.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Treemapping.Test
{
    [TestFixture()]
    public class CushionPainterTests
    {
        [Test()]
        public void RenderTest()
        {
            ITree tree = ViewTests.CreateTestTree();
            var cp = new CushionPainter();

            var runTime = TimeSpan.FromSeconds(3);
            var size = new System.Drawing.Size(0x200, 0x120);

            var sw = new Stopwatch();
            sw.Start();
            int loopCounter = 0;
            var layout = TreeLayoutExtensions.CreateLayoutTree(tree, _ => ((TreeLayout)_.Data).Color, _ => ((TreeLayout)_.Data).Size);
            layout.Squarify(RectangleD.FromLTRB(0, 0, size.Width, size.Height));
            for (; sw.Elapsed < runTime; ++loopCounter)
            {
                using(var bm = cp.Render(layout, layout.Data.Rectangle, size))
                {
                    Assert.AreEqual(size, bm.Size);
                }
            }
            Console.WriteLine("{0:E} pixel/s", (double) loopCounter * (double)size.Width * (double) size.Height / sw.Elapsed.TotalSeconds);
        }
    }
}