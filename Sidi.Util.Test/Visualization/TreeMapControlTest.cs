using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Forms;
using System.Windows.Forms;
using System.Drawing;
using Sidi.IO;

namespace Sidi.Visualization
{
    [TestFixture]
    public class TreeMapControlTest : TestBase
    {
        [Test, Explicit("interactive")]
        public void TestTreeDisplay()
        {
            var t = TreeMapTest.GetTestTree();
            var tm = t.CreateTreemapControl();
            tm.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void TestTreeWithColorDisplay()
        {
            var t = TreeMapTest.GetTestTree();
            var tm = t.CreateTreemapControl();
            tm.TreeMap.GetColor = n => new HSLColor(n.Data.TreeNode.Data * 36.0f, 50, 120);
            tm.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void Interact()
        {
            var c = FileSystemTree.Get(new Sidi.IO.Long.LongName(
                Sidi.IO.FileUtil.BinFile(".")
                ).ParentDirectory.ParentDirectory)
                .CreateTreemapControl();

            c.TreeMap.GetColor = n => FileSystemTree.ExtensionToColor(n.Data.TreeNode.Data);

            c.Paint += (s, e) =>
            {
                var white = new SolidBrush(Color.White);
                var sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Near;

                c.ForEachNode(n =>
                {

                    if (n.Data.Rectangle.Area() > 10000)
                    {
                        e.Graphics.DrawString(
                            n.Data.TreeNode.Data.Name, 
                            c.Font, 
                            white, 
                            n.Data.Rectangle.ToRectangleF(),
                            sf
                            );
                    }
                });

                if (c.MouseHoverNode != null)
                {
                    e.Graphics.DrawString(c.MouseHoverNode.Data.TreeNode.Data.ToString(), c.Font, new SolidBrush(Color.White), 0, 0);
                }
            };

            c.ItemMouseHover += (s, e) =>
                {
                    c.Invalidate();
                };

            c.ItemActivate += (s, e) =>
                {
                    try
                    {
                        e.Item.Data.FullPath.NoPrefix.ShellOpen();
                    }
                    catch
                    {
                    }
                };

            c.RunFullScreen();
        }

        public void Display<T>(T tree) where T : ITree
        {
            var tm = new TreeMap<T>(tree);
            var c = new TreeMapControl<T>();
            c.TreeMap = tm;
            var f = c.AsForm("test Cushion Tree Map");
            System.Windows.Forms.Application.Run(f);
        }
    }
}
    