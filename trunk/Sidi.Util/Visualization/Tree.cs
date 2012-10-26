using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Visualization
{
    public class Tree : ITree
    {
        public Tree(Tree parent)
        {
            this.Parent = parent;
            if (Parent != null)
            {
                if (parent.children == null)
                {
                    parent.children = new List<Tree>();
                }
                parent.children.Add(this);
            }
        }

        public IEnumerable<Tree> Up
        {
            get
            {
                for (var i = this; i != null; i = i.Parent)
                {
                    yield return i;
                }
            }
        }
        
        public IEnumerable<object> Lineage
        {
            get
            {
                return Up.Select(x => x.Object).Reverse();
            }
        }

        public object Object { get; set; }
        public float Size { get; set; }

        ITree ITree.Parent
        {
            get
            {
                return this.Parent;
            }
        }

        public Tree Parent { get; set; }
        
        IEnumerable<ITree> ITree.Children
        {
            get
            {
                return this.Children;
            }
        }

        public IEnumerable<Tree> Children
        {
            get
            {
                if (children == null)
                {
                    return new Tree[] { };
                }
                else
                {
                    return children;
                }
            }
        }

        List<Tree> children;

        /// <summary>
        /// calculates the sum of the sizes of all children
        /// </summary>
        public float ChildSize
        {
            get
            {
                return Children.Aggregate(0.0f, (s, i) => s + i.Size);
            }
        }

        public void UpdateSize()
        {
            if (Children.Any())
            {
                foreach (var i in Children)
                {
                    i.UpdateSize();
                }
                Size = ChildSize;
                children = children.OrderByDescending(x => x.Size).ToList();
            }
        }

        public override string ToString()
        {
            return Object == null ? String.Empty : Object.ToString();
        }

        public IEnumerable<Tree> AllNodes
        {
            get
            {
                return new Tree[] { this }.Concat(this.Children.SelectMany(c => c.AllNodes));
            }
        }
    }

    public static class ITreeEx
    {
        public static IEnumerable<ITree> GetAllNodes(this ITree tree)
        {
            return new ITree[] { tree }.Concat(tree.Children.SelectMany(c => c.GetAllNodes()));
        }
    }
}
