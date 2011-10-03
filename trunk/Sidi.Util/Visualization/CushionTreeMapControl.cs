using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Sidi.IO.Long;

namespace Sidi.Visualization
{
    public class CushionTreeMapControl<T> : Control
    {
        public static CushionTreeMapControl<T> FromTree(ITree<T> tree)
        {
            var c = new CushionTreeMapControl<T>();
            c.TreeMap = new CushionTreeMap<T>(tree);
            return c;
        }
        
        public CushionTreeMapControl()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer , true);
            SetStyle(ControlStyles.UserPaint, true);

            this.SizeChanged += new EventHandler(CushionTreeMapControl_SizeChanged);
            this.MouseMove += new MouseEventHandler(CushionTreeMapControl_MouseMove);

            this.MouseDoubleClick += (s, e) =>
                {
                    var l = GetLayoutAt(e.Location);
                    if (ItemActivate != null)
                    {
                        ItemActivate(this, new ItemEventEventArgs(l));
                    }
                };
        }

        void CushionTreeMapControl_MouseMove(object sender, MouseEventArgs e)
        {
            var l = GetLayoutAt(e.Location);
            if (l != hoveredNode)
            {
                hoveredNode = l;

                if (ItemMouseHover != null)
                {
                    ItemMouseHover(this, new ItemEventEventArgs(l));
                }
            }
        }

        ITree<CushionTreeMap<T>.Layout> GetLayoutAt(Point p)
        {
            return TreeMap.GetLayoutAt(p.ToArray());
        }

        ITree<CushionTreeMap<T>.Layout> hoveredNode;

        void CushionTreeMapControl_SizeChanged(object sender, EventArgs e)
        {
            if (cushions != null)
            {
                cushions.Dispose();
                cushions = null;
            }
        }

        public CushionTreeMap<T> TreeMap { set; get; }

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

        public void Highlight(PaintEventArgs e, ITree<CushionTreeMap<T>.Layout> layoutNode, Color color)
        {
            var b = new SolidBrush(Color.FromArgb(128, color));
            e.Graphics.FillRectangle(b, layoutNode.Data.Rectangle.ToRectangleF());
        }

        public void Highlight(PaintEventArgs e, Color color, Func<T, T, bool> f)
        {
            if (MouseHoverNode != null)
            {
                var h = MouseHoverNode.Data.TreeNode.Data;
                ForEachLeaf(n =>
                {
                    if (f(h, n.Data.TreeNode.Data))
                    {
                        Highlight(e, n, color);
                    }
                });
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            PaintCushions(e);
            base.OnPaint(e);
        }

        public ITree<CushionTreeMap<T>.Layout> MouseHoverNode
        {
            get
            {
                return hoveredNode;
            }
        }

        Font font = new Font(FontFamily.GenericSansSerif, 10.0f);
        Brush textBrush = new SolidBrush(Color.White);

        /// <summary>
        /// Outlines specified layout item and all its parents
        /// </summary>
        /// <param name="e"></param>
        /// <param name="tree"></param>
        public void Track(PaintEventArgs e, ITree<CushionTreeMap<T>.Layout> tree)
        {
            for (; tree != null; tree = tree.Parent)
            {
                e.Graphics.DrawRectangle(redPen, tree.Data.Rectangle.ToRectangle());
            }
        }

        Pen redPen = new Pen(Color.Red);

        void DrawOutlines(PaintEventArgs e, ITree<CushionTreeMap<T>.Layout> layoutTree)
        {
            var layout = layoutTree.Data;
            e.Graphics.DrawRectangle(redPen, layout.Rectangle.ToRectangle());
            foreach (var i in layoutTree.Children)
            {
                DrawOutlines(e, i);
            }
        }

        public void ForEachNode(Action<ITree<CushionTreeMap<T>.Layout>> a)
        {
            ForEachNode(TreeMap.LayoutTree, a);
        }

        public void ForEachLeaf(Action<ITree<CushionTreeMap<T>.Layout>> a)
        {
            ForEachLeaf(TreeMap.LayoutTree, a);
        }

        void ForEachNode(ITree<CushionTreeMap<T>.Layout> layoutTree, 
            Action<ITree<CushionTreeMap<T>.Layout>> a)
        {
            a(layoutTree);
            foreach (var i in layoutTree.Children)
            {
                ForEachNode(i, a);
            }
        }

        void ForEachLeaf(ITree<CushionTreeMap<T>.Layout> layoutTree,
            Action<ITree<CushionTreeMap<T>.Layout>> a)
        {
            if (layoutTree.Children.Count == 0)
            {
                a(layoutTree);
            }
            foreach (var i in layoutTree.Children)
            {
                ForEachLeaf(i, a);
            }
        }

        public class ItemEventEventArgs : EventArgs
        {
            public ItemEventEventArgs(ITree<CushionTreeMap<T>.Layout> layout)
            {
                this.layout = layout;
            }

            public ITree<CushionTreeMap<T>.Layout> Layout
            {
                get
                {
                    return layout;
                }
            }
            ITree<CushionTreeMap<T>.Layout> layout;

            public T Item
            {
                get
                {
                    return layout.Data.TreeNode.Data;
                }
            }
        }
        
        public delegate void ItemEventHandler(object sender, ItemEventEventArgs e);

        public event ItemEventHandler ItemMouseHover;
        public event ItemEventHandler ItemActivate;
    }
}
