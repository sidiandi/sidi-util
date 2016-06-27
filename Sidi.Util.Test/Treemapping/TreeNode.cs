using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Treemapping.Tests
{
    [TestFixture]
    public class TreeNodeTest
    {
        [Test]
        public void Ctor()
        {
            var p = new TreeNode(null);
            var c = new TreeNode(p);
            Assert.AreEqual(p, c.Parent);
            Assert.IsTrue(p.Nodes.Contains(c));
            c.Parent = null;
            Assert.AreEqual(null, c.Parent);
            Assert.IsFalse(p.Nodes.Contains(c));
        }

        internal static TreeNode CreateTestTree()
        {
            var random = new Random(0);
            var root = new TreeNode(null);
            AddRandomChilds(random, root, 1);
            root.UpdateSize();
            return root;
        }

        static void AddRandomChilds(Random random, TreeNode parent, int level)
        {
            var iEnd = random.Next(5, 10);
            for (int i = 0; i < iEnd; ++i)
            {
                new TreeNode(parent) { Text = random.Next().ToString() };
            }

            if (level > 0)
            {
                foreach (var c in parent.Nodes)
                {
                    AddRandomChilds(random, c, level - 1);
                }
            }
            else
            {
                foreach (var c in parent.Nodes)
                {
                    c.Size = random.Next(5, 10);
                    c.Color = Color.White;
                }
            }
        }
    }
}
    