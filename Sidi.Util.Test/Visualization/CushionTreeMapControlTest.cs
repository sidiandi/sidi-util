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
    public class CushionTreeMapControlTest : TestBase
    {
        [Test, Explicit("interactive")]
        public void Display()
        {
            Display(FileSystemTree.Get(new Sidi.IO.Long.LongName(Sidi.IO.FileUtil.BinFile("."))));
        }

        [Test, Explicit("interactive")]
        public void Interact()
        {
            var c = CushionTreeMapControl<Sidi.IO.Long.FileSystemInfo>.FromTree((FileSystemTree.Get(new Sidi.IO.Long.LongName(
                // Sidi.IO.FileUtil.BinFile(".")
                @"C:\Users\Andreas\Pictures"
                // @"C:\Users\Andreas\Pictures"
                // @"C:\work"
                
                ))));

            c.Paint += (s, e) =>
            {
                c.ForEachLeaf(n =>
                {
                    if (n.Data.TreeNode.Data.Extension.Length > 1)
                    {
                        c.Highlight(e, n, FileSystemTree.ExtensionToColor(n.Data.TreeNode.Data));
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
                        e.Item.FullPath.NoPrefix.ShellOpen();
                    }
                    catch
                    {
                    }
                };

            c.RunFullScreen();
        }

        public void Display<T>(ITree<T> t)
        {
            var tm = new CushionTreeMap<T>(t);
            var c = new CushionTreeMapControl<T>();
            c.TreeMap = tm;
            var f = c.AsForm("test Cushion Tree Map");
            System.Windows.Forms.Application.Run(f);
        }
    }
}
