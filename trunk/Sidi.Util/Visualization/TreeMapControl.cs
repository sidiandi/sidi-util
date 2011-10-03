using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Sidi.IO.Long;

namespace Sidi.Visualization
{
    public class TreeMapControl<T> : Control
    {
        public static TreeMapControl<T> FromTree(ITree<T> tree)
        {
            var c = new TreeMapControl<T>();
            c.TreeMap = new TreeMap<T>(tree);
            return c;
        }
        
        public TreeMapControl()
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

            this.ContextMenu = new ContextMenu(new[]{
                new MenuItem("Zoom In", (s,e ) => { ZoomIn(this.PointToClient(Control.MousePosition), 1); }),
                new MenuItem("Zoom Out", (s,e) => { ZoomOut(1); })
            });

            this.KeyPress += (s, e) =>
                {
                    switch (e.KeyChar)
                    {
                        case '+':
                            ZoomIn(PointToClient(Control.MousePosition), 1);
                            break;
                        case '-':
                            ZoomOut(1);
                            break;
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

        ITree<TreeMap<T>.Layout> GetLayoutAt(Point p)
        {
            return TreeMap.GetLayoutAt(p.ToArray(), Int32.MaxValue);
        }

        ITree<TreeMap<T>.Layout> GetLayoutAt(Point p, int levels)
        {
            return TreeMap.GetLayoutAt(p.ToArray(), levels);
        }

        ITree<TreeMap<T>.Layout> hoveredNode;

        void CushionTreeMapControl_SizeChanged(object sender, EventArgs e)
        {
            if (cushions != null)
            {
                cushions.Dispose();
                cushions = null;
            }
        }

        public TreeMap<T> TreeMap { set; get; }

        Bitmap cushions;
        void PaintCushions(PaintEventArgs e)
        {
            if (cushions == null || !cushions.Size.Equals(this.ClientSize))
            {
                TreeMap.Bounds = new RectangleF(0, 0, ClientSize.Width, ClientSize.Height);
                cushions = TreeMap.Render();
            }
            e.Graphics.DrawImage(cushions, 0, 0);
        }

        public void Highlight(PaintEventArgs e, ITree<TreeMap<T>.Layout> layoutNode, Color color)
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

        public ITree<TreeMap<T>.Layout> MouseHoverNode
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
        public void Track(PaintEventArgs e, ITree<TreeMap<T>.Layout> tree)
        {
            for (; tree != null; tree = tree.Parent)
            {
                e.Graphics.DrawRectangle(redPen, tree.Data.Rectangle.ToRectangle());
            }
        }

        Pen redPen = new Pen(Color.Red);

        void DrawOutlines(PaintEventArgs e, ITree<TreeMap<T>.Layout> layoutTree)
        {
            var layout = layoutTree.Data;
            e.Graphics.DrawRectangle(redPen, layout.Rectangle.ToRectangle());
            foreach (var i in layoutTree.Children)
            {
                DrawOutlines(e, i);
            }
        }

        public void ForEachNode(Action<ITree<TreeMap<T>.Layout>> a)
        {
            ForEachNode(TreeMap.LayoutTree, a);
        }

        public void ForEachLeaf(Action<ITree<TreeMap<T>.Layout>> a)
        {
            ForEachLeaf(TreeMap.LayoutTree, a);
        }

        void ForEachNode(ITree<TreeMap<T>.Layout> layoutTree, 
            Action<ITree<TreeMap<T>.Layout>> a)
        {
            a(layoutTree);
            foreach (var i in layoutTree.Children)
            {
                ForEachNode(i, a);
            }
        }

        void ForEachLeaf(ITree<TreeMap<T>.Layout> layoutTree,
            Action<ITree<TreeMap<T>.Layout>> a)
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
            public ItemEventEventArgs(ITree<TreeMap<T>.Layout> layout)
            {
                this.layout = layout;
            }

            public ITree<TreeMap<T>.Layout> Layout
            {
                get
                {
                    return layout;
                }
            }
            ITree<TreeMap<T>.Layout> layout;

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

        ITree<T> Tree
        {
            set
            {
                cushions.Dispose();
                cushions = null;
                TreeMap.Tree = value;
                Invalidate();
            }
        }

        public void ZoomIn(Point p, int levels)
        {
            Tree = GetLayoutAt(p, levels).Data.TreeNode;
        }

        public void ZoomOut(int levels)
        {
            var t = TreeMap.Tree;
            for (int i = 0; i < levels; ++i)
            {
                if (t.Parent != null)
                {
                    t = t.Parent;
                }
                else
                {
                    break;
                }
            }
            Tree = t;
        }
    }
}
