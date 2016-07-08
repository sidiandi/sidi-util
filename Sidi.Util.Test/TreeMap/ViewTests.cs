using NUnit.Framework;
using Sidi.IO;
using Sidi.Test;
using Sidi.TreeMap;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

namespace Sidi.TreeMap.Tests
{
    [TestFixture, Apartment(System.Threading.ApartmentState.STA)]
    public class ViewTests : TestBase
    {
        internal static ITree CreateTestTree()
        {
            var random = new Random(0);
            var root = new Tree<Layout>();
            AddRandomChilds(random, root, 3);
            return root;
        }

        static void AddRandomChilds(Random random, Tree<Layout> parent, int level)
        {
            var iEnd = random.Next(5, 10);
            for (int i = 0; i < iEnd; ++i)
            {
                new Tree<Layout>
                {
                    Parent = parent,
                    Data = new Layout
                    {
                    }
                };
            }

            if (level > 0)
            {
                foreach (var c in parent.Children)
                {
                    AddRandomChilds(random, c, level - 1);
                }
            }
            else
            {
                foreach (var c in parent.Children)
                {
                    c.Data.Size = random.Next(1, 10);
                }
            }
        }

        [Test]
        public void ViewTest()
        {
            var view = new View
            {
                Tree = CreateTestTree()
            };

            Sidi.Forms.Util.RunFullScreen(view);
        }

        [Test]
        public void FileTree()
        {
            var files = FileSystemTree.Create(TestFile(".").Parent.Parent.Parent);
            var colorScale = ColorScale.Distinct(files.GetLeafs(), _ => _.Data.Extension);
            var view = new View
            {
                Tree = files
            };

            using (var a = view.GetAdapter(files))
            {
                a.GetSize = _ => _.Data.Length;
                a.GetLabel = _ => _.Data.Name;
                a.GetColor = _ => colorScale(_);
                a.GetToolTipText = _ =>
                {
                    if (_.Data == null)
                    {
                        return null;
                    }
                    return String.Format(
                        BinaryPrefix.Instance,
                        "{0}\r\nSize:{1}\r\nLast modified: {2}", 
                        _.Data.FullName,
                        _.Data.Length, 
                        _.Data.LastWriteTime);
                };

                a.Activate += A_Activate;

                Sidi.Forms.Util.RunFullScreen(view);
            }
        }

        private void A_Activate(object sender, TreeEventArgs<IFileSystemInfo> e)
        {
            Process.Start(e.Tree.Data.FullName);
        }
    }
}