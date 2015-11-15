using NUnit.Framework;
using Sidi.Treemapping;
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
            var tree = new TreeNodeOperationsTest().CreateTree();
            var cp = new CushionPainter();

            var runTime = TimeSpan.FromSeconds(3);
            var size = new System.Drawing.Size(0x200, 0x120);

            var sw = new Stopwatch();
            sw.Start();
            int loopCounter = 0;
            for (; sw.Elapsed < runTime; ++loopCounter)
            {
                using(var bm = cp.Render(tree, tree.Rectangle, size))
                {
                    Assert.AreEqual(size, bm.Size);
                }
            }
            Console.WriteLine("{0:E} pixel/s", (double) loopCounter * (double)size.Width * (double) size.Height / sw.Elapsed.TotalSeconds);
        }
    }
}