using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Visualization
{
    public interface ITree<T>
    {
        T Data { get; }
        float Size { get; }
        ITree<T> Parent { get; }
        IList<ITree<T>> Children { get; set; }
    }

    public class Tree<T> : ITree<T>
    {
        public T Data { get; set; }
        public float Size { get; set; }
        public ITree<T> Parent { get; set; }
        public IList<ITree<T>> Children
        {
            get
            {
                if (children == null)
                {
                    children = new List<ITree<T>>();
                }
                return children;
            }
            set
            {
                children = value;
            }
        }
        public IList<ITree<T>> children;

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
    }
}
