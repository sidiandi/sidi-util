// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace Sidi.Treemapping
{
    public interface ITree<T>
    {
        T Data { get; set; }

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
        public IEnumerable<ITree<T>> Children
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
                if (parent == null)
                {
                }
                else
                {
                    parent.children.Remove(this);
                }
                parent = value;
                parent.children.Add(this);
            }
        }

        ITree<T> ITree<T>.Parent
        {
            get
            {
                return ((ITree<T>)Parent).Parent;
            }
        }

        Tree<T> parent;
    }
}
