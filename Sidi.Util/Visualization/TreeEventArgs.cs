using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Visualization
{
    public class TreeEventArgs : EventArgs
        {
            public TreeEventArgs(Layout layout)
            {
                this.layout = layout;
            }

            public Layout Layout
            {
                get
                {
                    return layout;
                }
            }
            Layout layout;

            public Tree Tree
            {
                get
                {
                    return layout.Tree;
                }
            }
        }
}
