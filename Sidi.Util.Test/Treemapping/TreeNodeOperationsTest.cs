using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.Forms;
using Sidi.IO;
using static System.Environment;
using Sidi.Test;

namespace Sidi.Treemapping.Test
{
    [TestFixture]
    public class TreeNodeOperationsTest : TestBase
    {
        [Test]
        public TreeNode CreateTree()
        {
            var leafs = Enumerable.Range(0, 1000);

            var tree = TreeNodeOperations.CreateTree(
                leafs,
                _ => _.ToString("D4").Cast<object>(),
                _ => System.Drawing.Color.Red,
                _ => (double)_);

            Assert.AreEqual(leafs.Sum(_ => (double)_), tree.Size);

            // calculate bounds
            tree.Rectangle = RectangleD.FromLTRB(0, 0, 1, 1);
            tree.Squarify();

            Assert.AreEqual(tree.Rectangle.Area, tree.GetLeafs().Sum(_ => _.Rectangle.Area), 1e-9);

            return tree;
        }

        public TreeNode CreateFileTree()
        {
            var leafs = Sidi.Caching.Cache.Get(Paths.GetFolderPath(SpecialFolder.MyDocuments), _ => Find.AllFiles(_).ToList());

            var tree = TreeNodeOperations.CreateTree<IFileSystemInfo>(
                leafs,
                _ => _.FullName.Parts,
                _ => Color.White,
                _ => _.Length);

            return tree;
        }

        [Test, Explicit("ui"), RequiresSTA]
        public void View()
        {
            var tree = CreateFileTree();

            var v = new View { Tree = tree, Size = new Size(640, 480) };
            v.Run();
        }
    }
}
