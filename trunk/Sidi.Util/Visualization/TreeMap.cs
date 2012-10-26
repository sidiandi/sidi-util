using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Sidi.IO.Long;
using System.Drawing.Drawing2D;
using Sidi.Extensions;

namespace Sidi.Visualization
{
    public class TreeMap : Control
    {
        public TreeMap()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);

            this.SizeChanged += new EventHandler(CushionTreeMapControl_SizeChanged);
            this.MouseMove += new MouseEventHandler(CushionTreeMapControl_MouseMove);

            this.MouseDoubleClick += (s, e) =>
            {
                if (ItemActivate != null)
                {
                    ItemActivate(this, new ItemEventEventArgs(GetLayoutAt(e.Location)));
                }
            };

            zoomPanController = new ZoomPanController(this);

            this.ContextMenuStrip = new ContextMenuStrip();

            this.ContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(ContextMenuStrip_Opening);
                
            this.MouseClick += (s, e) =>
            {
                ClickLocation = e.Location;
                SelectedObject = GetObjectAt(e.Location);
            };

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

            CushionPainter = new CushionPainter(this);
        }

        void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs ce)
        {
            var strip = this.ContextMenuStrip;

            strip.Items.Clear();

            strip.Items.Add(
                "View All", null, (s, e) =>
                {
                    ZoomOut(Int32.MaxValue);
                    zoomPanController.Reset();
                });

            strip.Items.Add(
                "Zoom In", null, (s, e) => { ZoomIn(ClickLocation, 1); });
            strip.Items.Add(
                "Zoom Out", null, (s, e) => { ZoomOut(1); });

            var layout = this.GetLayoutAt(PointToClient(Control.MousePosition));
            foreach (var i in layout.Up.Cast<Sidi.Visualization.TreeMapLayout.Layout>())
            {
                var tree = i.Tree;
                strip.Items.Add(
                    String.Format("View {0}", i.Tree.SafeToString()), null, (s, e) =>
                        {
                            Tree = tree;
                        });
            }
        }

        public static TreeMap FromTree(Tree tree)
        {
            return new TreeMap() { Tree = tree };
        }

        public TreeMapLayout Layout
        {
            get
            {
                return _layout;
            }

            private set
            {
                _layout = value;
                WorldTransform = new Matrix();
                Invalidate();
            }
        }

        TreeMapLayout _layout;
        ZoomPanController zoomPanController;
        public CushionPainter CushionPainter;
        TreeMapLayout.Layout hoveredNode;

        public object SelectedObject { set; get; }
        public Point ClickLocation { private set; get; }

        void CushionTreeMapControl_MouseMove(object sender, MouseEventArgs e)
        {
            var node = GetLayoutAt(e.Location);
            if (node != hoveredNode)
            {
                hoveredNode = node;

                if (ItemMouseHover != null && hoveredNode != null)
                {
                    ItemMouseHover(this, new ItemEventEventArgs(hoveredNode));
                }
            }
        }

        /// <summary>
        /// Orders all tree objects by orderBy and assigns a color based on the percentile
        /// </summary>
        /// <param name="orderBy"></param>
        public void SetBinnedNodeColor(Func<object, IComparable> orderBy)
        {
            var bins = new Bins(Tree.GetAllNodes().Cast<object>().Select(orderBy));
            var colorMap = ColorMap.BlueRed(0.0, 1.0);
            CushionPainter.NodeColor = n => colorMap.ToColor(bins.Percentile(orderBy(n)));
        }

        TreeMapLayout.Layout GetLayoutAt(Point p)
        {
            if (Layout == null)
            {
                return null;
            }

            return Layout.GetLayoutAt(GetWorldPoint(p), Int32.MaxValue);
        }

        Matrix WorldTransform
        {
            get
            {
                return zoomPanController.Transform;
            }

            set
            {
                zoomPanController.Transform = new Matrix();
            }
        }

        public float[] GetWorldPoint(Point p)
        {
            var fp = p.ToArray();
            var inverse = WorldTransform.Clone();
            inverse.Invert();
            inverse.Transform(fp);
            return fp;
        }

        public object GetObjectAt(Point p)
        {
            var l = GetLayoutAt(p);
            return l == null ? null : l.Tree.Object;
        }

        TreeMapLayout.Layout GetLayoutAt(Point p, int levels)
        {
            return Layout.GetLayoutAt(GetWorldPoint(p), levels);
        }

        void CushionTreeMapControl_SizeChanged(object sender, EventArgs e)
        {
            Tree = Tree;
        }

        public Tree Tree
        {
            get
            {
                return Layout == null ? null : Layout.Root.Tree;
            }

            set
            {
                if (value == null || value.Size == 0.0)
                {
                    Layout = null;
                }
                else
                {
                    Layout = new TreeMapLayout(value, this.Bounds);
                }
            }
        }

        public LabelPainter CreateLabelPainter()
        {
            return new LabelPainter(this);
        }

        public void Highlight(PaintEventArgs e, TreeMapLayout.Layout layoutNode, Color color)
        {
            var b = new SolidBrush(Color.FromArgb(128, color));
            e.Graphics.FillRectangle(b, layoutNode.Rectangle.ToRectangleF());
        }

        public void Highlight(PaintEventArgs e, Color color, Func<Tree, Tree, bool> f)
        {
            if (MouseHoverNode != null)
            {
                var hovered = MouseHoverNode.Tree;
                ForEachLeaf(n =>
                {
                    if (f(hovered, (Tree) n.Tree))
                    {
                        Highlight(e, n, color);
                    }
                });
            }
        }

        Matrix worldTransform = new Matrix();
        
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Transform = zoomPanController.Transform;
            CushionPainter.Paint(e);
            base.OnPaint(e);
        }

        public TreeMapLayout.Layout MouseHoverNode
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
        public void Track(PaintEventArgs e, TreeMapLayout.Layout tree)
        {
            for (; tree != null; tree = (TreeMapLayout.Layout) tree.Parent)
            {
                e.Graphics.DrawRectangle(redPen, tree.Rectangle.ToRectangle());
            }
        }

        Pen redPen = new Pen(Color.Red);

        void DrawOutlines(PaintEventArgs e, TreeMapLayout.Layout layoutTree)
        {
            var layout = layoutTree;
            e.Graphics.DrawRectangle(redPen, layout.Rectangle.ToRectangle());
            foreach (var i in layoutTree.Children)
            {
                DrawOutlines(e, i);
            }
        }

        public void ForEachNode(Action<TreeMapLayout.Layout> a)
        {
            ForEachNode(Layout.Root, a);
        }

        public void ForEachLeaf(Action<TreeMapLayout.Layout> a)
        {
            ForEachLeaf(Layout.Root, a);
        }

        void ForEachNode(TreeMapLayout.Layout layoutTree, Action<TreeMapLayout.Layout> a)
        {
            a(layoutTree);
            foreach (var i in layoutTree.Children.Cast<TreeMapLayout.Layout>())
            {
                ForEachNode(i, a);
            }
        }

        void ForEachLeaf(TreeMapLayout.Layout layoutTree, Action<TreeMapLayout.Layout> a)
        {
            if (!layoutTree.Children.Any())
            {
                a(layoutTree);
            }
            foreach (var i in layoutTree.Children.Cast<TreeMapLayout.Layout>())
            {
                ForEachLeaf(i, a);
            }
        }

        public class ItemEventEventArgs : EventArgs
        {
            public ItemEventEventArgs(TreeMapLayout.Layout layout)
            {
                this.layout = layout;


            }

            public TreeMapLayout.Layout Layout
            {
                get
                {
                    return layout;
                }
            }
            TreeMapLayout.Layout layout;

            public Tree Tree
            {
                get
                {
                    return layout.Tree;
                }
            }
        }
        
        public delegate void ItemEventHandler(object sender, ItemEventEventArgs e);

        public event ItemEventHandler ItemMouseHover;
        public event ItemEventHandler ItemActivate;

        static IList<ITree> GetLineage(ITree t)
        {
            var lineage = new List<ITree>();
            for (var i = t; i != null; i = i.Parent)
            {
                lineage.Add(i);
            }
            lineage.Reverse();
            return lineage;
        }

        public void ZoomIn(Point p, int levels)
        {
            Tree = GetLayoutAt(p, levels).Tree;
        }

        public void ZoomOut(int levels)
        {
            var t = Tree;
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
