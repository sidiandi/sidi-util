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
using System.Diagnostics;

namespace Sidi.Treemapping.Test
{
    [TestFixture]
    public class TreeNodeOperationsTest : TestBase
    {
        public TreeNode CreateTree()
        {
            var leafs = Enumerable.Range(0, 1000);

            var tree = TreeNodeOperations.CreateTree(
                leafs,
                _ => _.ToString("D4").Cast<object>(),
                _ => System.Drawing.Color.Red,
                _ => (double)_);

            Assert.AreEqual(leafs.Sum(_ => (double)_), tree.Size);

            return tree;
        }

        public TreeNode CreateFileTree()
        {
            var dir = new LPath(@"Z:\site");
            // var dir = Paths.GetFolderPath(SpecialFolder.MyDocuments);
            var leafs = Sidi.Caching.Cache.Get(dir, _ => Find.AllFiles(_).ToList());

            var tree = TreeNodeOperations.CreateTree<IFileSystemInfo>(
                leafs,
                _ => _.FullName.Parts,
                _ => Color.White,
                _ => _.Length);

            return tree;
        }

        [Test, Ignore("ui"), Apartment(System.Threading.ApartmentState.STA)]
        public void View()
        {
            var tree = CreateFileTree();

            var v = new View { Tree = tree, Size = new Size(640, 480) };

            v.MouseDoubleClick += (s, e) =>
            {
                var n = v.GetNode(e.Location);
                if (n != null)
                {
                    var file = (IFileSystemInfo)n.Data.Tag;
                    Process.Start(file.FullName);
                }
            };
            v.Run();
        }

        private void V_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
