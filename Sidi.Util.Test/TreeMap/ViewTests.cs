using NUnit.Framework;
using Sidi.IO;
using Sidi.Test;
using Sidi.TreeMap;
using System;
using System.Collections.Generic;
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
                Tree = files,
                GetSize = _ => ((IFileSystemInfo)_.Data).Length,
                GetLabel = _ => ((IFileSystemInfo)_.Data).Name,
                GetColor = _ => colorScale((ITree<IFileSystemInfo>)_)
            };

            Sidi.Forms.Util.RunFullScreen(view);
        }
    }
}