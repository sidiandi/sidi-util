using NUnit.Framework;
using Sidi.IO;
using Sidi.Test;
using Sidi.TreeMap;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.TreeMap.Tests
{
    [TestFixture()]
    public class TreeLayoutExtensionsTests : TestBase
    {
        [Test()]
        public void SquarifyTest()
        {
            var tree = FileSystemTree.Create(TestFile(".").Parent.Parent);
            var layout = TreeLayoutExtensions.CreateLayoutTree(tree, _ => Color.White, _ => ((IFileSystemInfo)_.Data).Length);
            var rect = RectangleD.FromLTRB(0, 0, 1, 1);
            layout.Squarify(rect);
            Assert.AreEqual(rect.Area, layout.GetLeafs().Sum(_ => _.Data.Rectangle.Area));
        }
    }
}