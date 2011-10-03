using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Sidi.IO.Long;

namespace Sidi.Visualization
{
    public class CushionTreeMapControl : Control
    {
        public CushionTreeMapControl()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer , true);
            SetStyle(ControlStyles.UserPaint, true);

            this.SizeChanged += new EventHandler(CushionTreeMapControl_SizeChanged);
            this.MouseMove += new MouseEventHandler(CushionTreeMapControl_MouseMove);
        }

        void CushionTreeMapControl_MouseMove(object sender, MouseEventArgs e)
        {
            var l = GetLayoutAt(e.Location);
            if (l != layoutUnderMouse)
            {
                layoutUnderMouse = l;
                Invalidate();
            }
        }

        ITree GetLayoutAt(Point p)
        {
            return TreeMap.GetLayoutAt(p.ToArray());
        }

        ITree layoutUnderMouse;

        void CushionTreeMapControl_SizeChanged(object sender, EventArgs e)
        {
            if (cushions != null)
            {
                cushions.Dispose();
                cushions = null;
            }
        }

        public CushionTreeMap TreeMap { set; get; }

        Bitmap cushions;
        void PaintCushions(PaintEventArgs e)
        {
            if (cushions == null || !cushions.Size.Equals(this.ClientSize))
            {
                cushions = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
                TreeMap.Render(cushions, this.ClientRectangle);
            }
            e.Graphics.DrawImage(cushions, 0, 0);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            PaintCushions(e);

                if (layoutUnderMouse != null)
                {
                    Track(e, layoutUnderMouse);
                    DrawForEachNode(e, (ea, layout) =>
                    {
                        var fi = (Sidi.IO.Long.FileSystemInfo)layout.TreeNode.Data;
                        var fi1 = (Sidi.IO.Long.FileSystemInfo)TreeNodeUnderMouse.Data;
                        if (fi.Extension.Equals(fi1.Extension))
                        {
                            var b = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
                            ea.Graphics.FillRectangle(b, layout.Rectangle.ToRectangleF());
                        }
                    });

                    var fsi = (FileSystemInfo)TreeNodeUnderMouse.Data;
                    e.Graphics.DrawString(fsi.FullPath.NoPrefix, font, textBrush, 0,0);
                }
        }

        ITree TreeNodeUnderMouse
        {
            get
            {
                var ly = (CushionTreeMap.Layout)layoutUnderMouse.Data;
                return ly.TreeNode;
            }
        }

        Font font = new Font(FontFamily.GenericSansSerif, 10.0f);
        Brush textBrush = new SolidBrush(Color.White);

        /// <summary>
        /// Outlines specified layout item and all its parents
        /// </summary>
        /// <param name="e"></param>
        /// <param name="tree"></param>
        public void Track(PaintEventArgs e, ITree tree)
        {
            for (; tree != null; tree = tree.Parent)
            {
                var layout = (CushionTreeMap.Layout)tree.Data;
                e.Graphics.DrawRectangle(redPen, layout.Rectangle.ToRectangle());
            }
        }

        Pen redPen = new Pen(Color.Red);

        void DrawOutlines(PaintEventArgs e, ITree layoutTree)
        {
            var layout = (CushionTreeMap.Layout)layoutTree.Data;
            e.Graphics.DrawRectangle(redPen, layout.Rectangle.ToRectangle());
            foreach (var i in layoutTree.Children)
            {
                DrawOutlines(e, i);
            }
        }

        void DrawForEachNode(PaintEventArgs e, Action<PaintEventArgs, CushionTreeMap.Layout> a)
        {
            DrawForEachNode(e, TreeMap.LayoutTree, a);
        }

        void DrawForEachNode(PaintEventArgs e, ITree layoutTree, Action<PaintEventArgs, CushionTreeMap.Layout> a)
        {
            var layout = (CushionTreeMap.Layout)layoutTree.Data;
            a(e, layout);
            foreach (var i in layoutTree.Children)
            {
                DrawForEachNode(e, i, a);
            }
        }
    }
}
