using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using System.IO;
using System.Drawing;

namespace Sidi.Treemapping
{
    public enum Direction
    {
        X,
        Y
    };

    public static class TreeNodeOperations
    {
        /// <summary>
        /// Recalculates the size of nodes by adding up the leaf node sizes.
        /// </summary>
        /// <param name="tree"></param>
        public static void UpdateSize(this TreeNode tree)
        {
            if (tree.Nodes.Count == 0)
            {
            }
            else
            {
                double newSize = 0;
                foreach (var i in tree.Nodes)
                {
                    UpdateSize(i);
                    newSize += i.Size;
                }
                tree.Size = newSize;
            }
        }

        public static IEnumerable<TreeNode> RecurseBreadthFirst(this TreeNode tree)
        {
            var todo = new List<TreeNode> { tree };

            for (; todo.Any();)
            {
                var i = todo.Pop();
                yield return i;
                todo.AddRange(i.Nodes);
            }
        }

        public static IEnumerable<TreeNode> RecurseDepthFirst(this TreeNode tree)
        {
            var todo = new List<TreeNode> { tree };

            for (; todo.Any();)
            {
                var i = todo.Pop();
                yield return i;
                todo.InsertRange(0, i.Nodes);
            }
        }

        public static IEnumerable<TreeNode> GetLeafs(this TreeNode tree)
        {
            return tree.RecurseDepthFirst().Where(_ => _.IsLeaf);
        }

        public static TreeNode CreateTree<T>(
            IEnumerable<T> leafs,
            Func<T, IEnumerable<object>> getLineage,
            Func<T, System.Drawing.Color> getColor,
            Func<T, double> getSize)
        {
            var root = new TreeNode(null);

            foreach (var i in leafs)
            {
                var p = root;
                foreach (var j in getLineage(i))
                {
                    var existingNode = p.Nodes.FirstOrDefault(_ => object.Equals(_.Tag, j));
                    if (existingNode == null)
                    {
                        existingNode = new TreeNode(p) { Tag = j };
                    }
                    else
                    {
                    }
                    p = existingNode;
                }


                p.Size = getSize(i);
                p.Color = getColor(i);
            }

            root.UpdateSize();
            return root;
        }

        public static void Dump(this TreeNode tree, TextWriter w)
        {
            Dump(tree, w, String.Empty, "- ");
        }

        static void Dump(this TreeNode tree, TextWriter w, string indent, string indentIncrement)
        {
            Console.WriteLine("{0}{1}", indent, tree);
            indent = indent + indentIncrement;
            foreach (var c in tree.Nodes)
            {
                Dump(c, w, indent, indentIncrement);
            }
        }

        public static void Stripes(this TreeNode tree)
        {
            Stripes(tree, Direction.X);
        }

        public static void Stripes(this TreeNode tree, Direction dir)
        {
            switch (dir)
            {
                case Direction.X:
                    Columns(tree);
                    break;
                case Direction.Y:
                    Rows(tree);
                    break;
            }

            if (dir == Direction.X)
            {
                dir = Direction.Y;
            }
            else
            {
                dir = Direction.X;
            }

            foreach (var i in tree.Nodes)
            {
                i.Stripes(dir);
            }
        }

        static void Columns(this TreeNode tree)
        {
            var bounds = tree.Rectangle;
            double s = 0;
            double sizeInv = 1.0 / tree.Size * bounds.Width;
            double p0 = s * sizeInv + bounds.Left;
            for (int i = 0; i < tree.Nodes.Count; ++i)
            {
                var n = tree.Nodes[i];
                s += n.Size;
                double p1 = s * sizeInv + bounds.Left;
                n.Rectangle = RectangleF.FromLTRB((float)p0, (float) bounds.Top, (float)p1, (float) bounds.Bottom);
                p0 = p1;
            }
        }

        static void Rows(this TreeNode tree)
        {
            var bounds = tree.Rectangle;
            double s = 0;
            double sizeInv = 1.0 / tree.Size * bounds.Height;
            double p0 = s * sizeInv + bounds.Top;
            for (int i = 0; i < tree.Nodes.Count; ++i)
            {
                var n = tree.Nodes[i];
                s += n.Size;
                double p1 = s * sizeInv + bounds.Top;
                n.Rectangle = RectangleD.FromLTRB(bounds.Left, p0, bounds.Right, p1);
                p0 = p1;
            }
        }

        public static void Squarify(this TreeNode tree)
        {
            var bounds = tree.Rectangle;
            var nodes = tree.Nodes
                .OrderByDescending(x => x.Size)
                .ToList();

            for (;nodes.Count > 0;)
            {
                RectangleD remaining;
                var bestAr = Double.MaxValue;
                double ar;
                int count = 1;
                for (; count < nodes.Count; ++count)
                {
                    Squarify(nodes, count, bounds, out remaining, out ar);
                    if (ar > bestAr)
                    {
                        --count;
                        break;
                    }
                    else
                    {
                        bestAr = ar;
                    }
                }
                Squarify(nodes, count, bounds, out remaining, out ar);
                bounds = remaining;
                nodes = nodes.Skip(count).ToList();
            }

            foreach (var i in tree.Nodes)
            {
                i.Squarify();
            }
        }

        /// <summary>
        /// Squarify the count first child nodes and returns the worst aspect ratio
        /// </summary>
        /// <param name="childs"></param>
        /// <param name="count"></param>
        /// <param name="bounds"></param>
        /// <param name="remaining"></param>
        /// <param name="worstAspectRatio"></param>
        static void Squarify(IList<TreeNode> childs, int count, RectangleD bounds, out RectangleD remaining, out double worstAspectRatio)
        {
            var size = childs.Sum(_ => _.Size);
            var isRow = bounds.Width < bounds.Height;

            var rowSize = childs.Take(count).Sum(_ => _.Size);
            var rowWidth = Math.Min(bounds.Width, bounds.Height);
            var height = Math.Max(bounds.Width, bounds.Height);
            var rowHeight = (float)(height * rowSize / size);
            var columnPerSize = rowWidth / rowSize;

            worstAspectRatio = 1.0;
            double s = 0;
            for (int i = 0; i < count; ++i)
            {
                var s1 = s + childs[i].Size;
                if (isRow)
                {
                    childs[i].Rectangle = RectangleD.FromLTRB(
                        bounds.Left + s * columnPerSize,
                        bounds.Top,
                        bounds.Left + s1 * columnPerSize,
                        bounds.Top + rowHeight);
                }
                else
                {
                    childs[i].Rectangle = RectangleD.FromLTRB(
                        bounds.Left,
                        (bounds.Top + s * columnPerSize),
                        bounds.Left + rowHeight,
                        (bounds.Top + s1 * columnPerSize));
                }
                s = s1;
                worstAspectRatio = Math.Max(childs[i].Rectangle.GetAspectRatio(), worstAspectRatio);
            }

            if (isRow)
            {
                remaining = RectangleD.FromLTRB(
                        bounds.Left,
                        bounds.Top + rowHeight,
                        bounds.Right,
                        bounds.Bottom);
            }
            else
            {
                remaining = RectangleD.FromLTRB(
                        bounds.Left + rowHeight,
                        bounds.Top,
                        bounds.Right,
                        bounds.Bottom);
            }
        }

        public static TreeNode GetFirstLeaf(this TreeNode tree)
        {
            if (tree.IsLeaf)
            {
                return tree;
            }
            else
            {
                return tree.Nodes.First().GetFirstLeaf();
            }
        }
    }
}
