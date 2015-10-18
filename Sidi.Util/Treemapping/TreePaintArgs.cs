using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sidi.Treemapping
{
    public class TreePaintArgs
    {
        public PaintEventArgs PaintEventArgs;
        public System.Windows.Media.Matrix WorldToScreen;
        public TreeNode Tree;
        public RectangleD ScreenRect;
    }
}
