using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Visualization
{
    public class Layout : Tree
    {
        public Layout(Layout parent)
            : base(parent)
        {
            Bounds = new float[2, 2];
        }

        public IEnumerable<Layout> Children { get { return base.Children.Cast<Layout>(); } }

        public float[,] Bounds;
        public Tree Tree;
    }

}
