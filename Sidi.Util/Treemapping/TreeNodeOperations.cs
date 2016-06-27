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
            Dump(tree, w, "", "|");
        }

        static void Dump(this TreeNode tree, TextWriter w, string indent, string indentIncrement)
        {
            Console.WriteLine("{0}{1}", indent, tree);
            indent = indentIncrement + indent;
            foreach (var c in tree.Nodes)
            {
                Dump(c, w, indent, indentIncrement);
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
