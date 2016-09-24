using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace Sidi.TreeMap
{
    /// <summary>
    /// Read-only tree data structure
    /// </summary>
    public interface ITree
    {
        object Data { get; }

        IEnumerable<ITree> Children { get; }

        ITree Parent { get; }
    }

    public interface ITree<T> : ITree
    {
        T Data { get; }

        IEnumerable<ITree<T>> Children { get; }

        ITree<T> Parent { get; }
    }

    public static class ITreeExtensions
    {
        public static ITree<T> GetFirstLeaf<T>(this ITree<T> tree)
        {
            using (var e = tree.Children.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    return e.Current.GetFirstLeaf();
                }
                else
                {
                    return tree;
                }
            }
        }

        public static bool IsLeaf<T>(this ITree<T> tree)
        {
            using (var e = tree.Children.GetEnumerator())
            {
                return !e.MoveNext();
            }
        }

        public static IEnumerable<ITree<T>> GetLeafs<T>(this ITree<T> tree)
        {
            return tree.RecurseDepthFirst().Where(_ => _.IsLeaf());
        }

        public static IEnumerable<ITree<T>> RecurseBreadthFirst<T>(this ITree<T> tree)
        {
            var todo = new List<ITree<T>> { tree };

            for (; todo.Any();)
            {
                var i = todo.Pop();
                yield return i;
                todo.AddRange(i.Children);
            }
        }

        public static IEnumerable<ITree<T>> RecurseDepthFirst<T>(this ITree<T> tree)
        {
            var todo = new List<ITree<T>> { tree };

            for (; todo.Any();)
            {
                var i = todo.Pop();
                yield return i;
                todo.InsertRange(0, i.Children);
            }
        }

        public static Tree<LeafTreeData<BranchType, LeafType>> FromLeaves<BranchType, LeafType>(
            IEnumerable<LeafType> leaves,
            Func<LeafType, IEnumerable<BranchType>> getLineage)
        {
            var tree = new Tree<LeafTreeData<BranchType, LeafType>>();
            tree.Data = new LeafTreeData<BranchType, LeafType>();

            foreach (var leaf in leaves)
            {
                var t = tree;
                foreach (var b in getLineage(leaf))
                {
                    var c = t.Children.FirstOrDefault(_ => object.Equals(_.Data.Name, b));
                    if (c == null)
                    {
                        // branch does not exist and must be created
                        c = new Tree<LeafTreeData<BranchType, LeafType>>(
                            new LeafTreeData<BranchType, LeafType>
                            {
                                Leaf = default(LeafType),
                                Name = b
                            },
                            Enumerable.Empty<Tree<LeafTreeData<BranchType, LeafType>>>());
                        c.Parent = t;
                    }
                    t = c;
                }
                t.Data.Leaf = leaf;
            }
            return tree;
        }
    }
}

public class LeafTreeData<BranchType, LeafType>
{
    public BranchType Name;
    public LeafType Leaf;
}
