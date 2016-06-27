using NUnit.Framework;
using Sidi.Treemapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Treemapping.Tests
{
    [TestFixture()]
    public class TreeNodeOperationsTests
    {
        [Test()]
        public void UpdateSizeTest()
        {
            var tree = TreeNodeTest.CreateTestTree();
            tree.UpdateSize();
            Assert.AreEqual(tree.GetLeafs().Sum(_ => _.Size), tree.Size);
        }

        [Test()]
        public void Recurse()
        {
            var tree = TreeNodeTest.CreateTestTree();
            Assert.AreEqual(tree.RecurseBreadthFirst().Count(), tree.RecurseDepthFirst().Count());
        }

        [Test()]
        public void GetLeafsTest()
        {
            var tree = TreeNodeTest.CreateTestTree();
            var leafs = tree.GetLeafs().ToList();
            Assert.IsTrue(leafs.All(_ => _.Nodes.Count == 0));
            Assert.IsTrue(leafs.Count > 0);
        }

        [Test()]
        public void DumpTest()
        {
            var tree = TreeNodeTest.CreateTestTree();
            tree.Dump(Console.Out);
        }

        [Test()]
        public void SquarifyTest()
        {
            var tree = TreeNodeTest.CreateTestTree();
            tree.UpdateSize();
            var rectangle = RectangleD.FromLTRB(0, 0, 1, 1);
            var layout = tree.Squarify(rectangle);
            foreach (var n in layout.RecurseDepthFirst())
            {
                Assert.AreEqual(n.Data.Tag.Size / tree.Size, n.Data.Rectangle.Area, 1e-5);
            }
            tree.Dump(Console.Out);
        }
    }
}