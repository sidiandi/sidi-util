using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Visualization
{
    public interface ITree
    {
        float Size { get; }
        ITree Parent { get; }
        IEnumerable<ITree> Children { get; }
    }

    public class Tree<T> : ITree
    {
        public Tree(Tree<T> parent)
        {
            this.Parent = parent;
        }

        public IList<T> Lineage
        {
            get
            {
                var lineage = new List<T>();
                for (var i = this; i != null; i = i.Parent)
                {
                    lineage.Add(i.Data);
                }
                lineage.Reverse();
                return lineage;
            }
        }

        public T Data { get; set; }
        public float Size { get; set; }

        ITree ITree.Parent
        {
            get
            {
                return this.Parent;
            }
        }

        public Tree<T> Parent { get; set; }
        
        IEnumerable<ITree> ITree.Children
        {
            get
            {
                return this.Children;
            }
        }

        public List<Tree<T>> Children
        {
            get
            {
                if (children == null)
                {
                    children = new List<Tree<T>>();
                }
                return children;
            }
            set
            {
                children = value;
            }
        }

        List<Tree<T>> children;

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
                Children = Children.OrderByDescending(x => x.Size).ToList();
            }
        }

        public override string ToString()
        {
            return Data.ToString();
        }

        public IEnumerable<Tree<T>> AllNodes
        {
            get
            {
                return new Tree<T>[] { this }.Concat(this.Children.SelectMany(c => c.AllNodes));
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
