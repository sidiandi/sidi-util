using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Treemapping.Test
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
    }
}
    