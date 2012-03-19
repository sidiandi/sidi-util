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
            var tm = t.CreateTreemapControl();
            tm.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void TestTreeWithColorDisplay()
        {
            var t = TreeMapTest.GetTestTree();
            var tm = t.CreateTreemapControl();
            tm.CushionPainter.NodeColor = n => new HSLColor(n.Data * 36.0f, 50, 120);
            tm.RunFullScreen();
        }

        [Test, Explicit("interactive")]
        public void Interact()
        {
            var c = FileSystemTree.Get(
                
                // Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                @"H:\media\album"
                
                .Long())
                .CreateTreemapControl();

            var lp = c.CreateLabelPainter();
            lp.Text = t => t.Data.Name;
            
            c.Paint += (s, e) =>
            {
                lp.Paint(e);
            };

            c.ContextMenu.MenuItems.AddRange(new []
            {
                new MenuItem("Open in Explorer", (s, e) =>
                {
                    Process.Start("explorer.exe", "/select," + c.SelectedNode.Data.FullName.ToString());
                }){ Break = true },
                new MenuItem("Color by File Type", (s,e) =>
                {
                    c.CushionPainter.NodeColor = n => FileSystemTree.ExtensionToColor(n.Data);
                }),
                new MenuItem("Color by Change Date", (s,e) =>
                    {
                        c.SetBinnedNodeColor(n => n.Data.LastWriteTimeUtc);
                    })
            });

            var ti = new ToolTip();

            c.ItemMouseHover += (s, e) =>
                {
                    ti.SetToolTip(c, e.Item.Data.FullName.ToString());
                };

            c.ItemActivate += (s, e) =>
                {
                    try
                    {
                        e.Item.Data.FullName.NoPrefix.ShellOpen();
                    }
                    catch
                    {
                    }
                };

            c.RunFullScreen();
        }

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
            var file = new Sidi.IO.Long.Path(TestFile(@".")).ParentDirectory.ParentDirectory;
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

        public void Display<T>(T tree) where T : ITree
        {
            var c = new TreeMapControl<T>();
            c.Tree = tree;
            var f = c.AsForm("test Cushion Tree Map");
            System.Windows.Forms.Application.Run(f);
        }
    }
}
    