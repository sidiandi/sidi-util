using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Sidi.IO.Long;
using System.Drawing.Drawing2D;

namespace Sidi.Visualization
{
    public class TreeMapControl : Control
    {
        public static TreeMapControl FromTree(Tree tree)
        {
            return new TreeMapControl() { Tree = tree };
        }

        ZoomPanController zoomPanController;
        
        public TreeMapControl()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer , true);
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

            this.ContextMenu = new ContextMenu(new[]{
                new MenuItem("Zoom In", (s,e) => { ZoomIn(ClickLocation, 1); }),
                new MenuItem("Zoom Out", (s,e) => { ZoomOut(1); })
            });

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

        public CushionPainter CushionPainter;

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
            return Layout.GetLayoutAt(p.ToArray(), Int32.MaxValue);
        }

        public object GetObjectAt(Point p)
        {
            return ((Tree)GetLayoutAt(p).Tree).Object;
        }

        TreeMapLayout.Layout GetLayoutAt(Point p, int levels)
        {
            return Layout.GetLayoutAt(p.ToArray(), levels);
        }

        TreeMapLayout.Layout hoveredNode;

        void CushionTreeMapControl_SizeChanged(object sender, EventArgs e)
        {
            this.Layout = new TreeMapLayout(Layout.LayoutTree.Tree, this.Bounds);
        }

        public TreeMapLayout Layout { get; private set; }

        public Tree Tree
        {
            get
            {
                return Layout.LayoutTree.Tree;
            }

            set
            {
                Layout = new TreeMapLayout(value, this.Bounds);
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
            ForEachNode(Layout.LayoutTree, a);
        }

        public void ForEachLeaf(Action<TreeMapLayout.Layout> a)
        {
            ForEachLeaf(Layout.LayoutTree, a);
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
