// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace Sidi.Treemapping
{
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

    }

    public class Tree<T> : ITree<T>
    {
        IEnumerable<ITree<T>> ITree<T>.Children
        {
            get
            {
                return children;
            }
        }

        public IList<Tree<T>> Children
        {
            get
            {
                return children;
            }
        }

        List<Tree<T>> children = new List<Tree<T>>();

        public T Data
        {
            get
            {
                return data;
            }

            set
            {
                data = value;
            }
        }

        T data;

        public Tree<T> Parent
        {
            get
            {
                return parent;
            }

            set
            {
                if (parent != null)
                {
                    parent.children.Remove(this);
                }

                parent = value;

                if (parent != null)
                {
                    parent.children.Add(this);
                }
            }
        }

        ITree<T> ITree<T>.Parent
        {
            get
            {
                return ((ITree<T>)Parent).Parent;
            }
        }

        object ITree.Data
        {
            get
            {
                return data;
            }
        }

        IEnumerable<ITree> ITree.Children
        {
            get
            {
                return children;
            }
        }

        ITree ITree.Parent
        {
            get
            {
                return parent;
            }
        }

        Tree<T> parent;
    }
}
