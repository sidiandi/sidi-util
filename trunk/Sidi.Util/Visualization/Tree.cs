// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

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
        public double Size { get; set; }

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
        public double ChildSize
        {
            get
            {
                return Children.Aggregate(0.0, (s, i) => s + i.Size);
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
            }
        }

        public void Sort(Func<Tree, IComparable> by)
        {
            if (children != null)
            {
                foreach (var c in children)
                {
                    c.Sort(by);
                }
                children = children.OrderBy(x => by(x)).ToList();
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

    public static class ITreeExtensions
    {
        public static IEnumerable<ITree> GetAllNodes(this ITree tree)
        {
            return new ITree[] { tree }.Concat(tree.Children.SelectMany(c => c.GetAllNodes()));
        }
    }
}
