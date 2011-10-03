using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Visualization
{
    public interface ITree
    {
        object Data { get; }
        float Size { get; }
        ITree Parent { get; }
        IList<ITree> Children { get; set; }
    }

    public class Tree : ITree
    {
        public object Data { get; set; }
        public float Size { get; set; }
        public ITree Parent { get; set; }
        public IList<ITree> Children
        {
            get
            {
                if (children == null)
                {
                    children = new List<ITree>();
                }
                return children;
            }
            set
            {
                children = value;
            }
        }
        public IList<ITree> children;

        public float ChildSize
        {
            get
            {
                return Children.Cast<Tree>().Aggregate(0.0f, (s, i) => s + i.Size);
            }
        }
    }
}
