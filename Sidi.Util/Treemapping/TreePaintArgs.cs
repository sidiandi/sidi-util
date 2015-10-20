using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Sidi.Treemapping
{
    public class TreePaintArgs
    {
        public PaintEventArgs PaintEventArgs;
        public System.Windows.Media.Matrix WorldToScreen;
        public System.Windows.Media.Matrix ScreenToWorld;
        public TreeNode Tree;
        public RectangleD ScreenRect;

        public TreePaintArgs Clone()
        {
            return new TreePaintArgs
            {
                PaintEventArgs = this.PaintEventArgs,
                WorldToScreen = this.WorldToScreen,
                ScreenToWorld = this.ScreenToWorld,
                Tree = this.Tree,
                ScreenRect = this.ScreenRect
            };
        }
    }
}
