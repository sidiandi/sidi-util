// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.IO;

namespace Sidi.TreeMap
{
    public class Tree<T> : ITree<T>
    {
        Tree<T> parent;
        List<Tree<T>> children = new List<Tree<T>>();
        T data;

        IEnumerable<ITree<T>> ITree<T>.Children
        {
            get
            {
                return children;
            }
        }

        public IReadOnlyList<Tree<T>> Children
        {
            get
            {
                return children;
            }
        }

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

        public Tree()
        {
        }

        public Tree(T data, IEnumerable<Tree<T>> children)
        {
            this.Data = data;
            foreach (var i in children)
            {
                i.Parent = this;
            }
        }
    }
}
