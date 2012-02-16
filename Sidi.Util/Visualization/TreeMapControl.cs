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
    public static class TreeMapControlEx
    {
        public static TreeMapControl<T> CreateTreemapControl<T>(this T tree) where T : ITree
        {
            var c = new TreeMapControl<T>();
            c.Tree = tree;
            return c;
        }
    }

    public class TreeMapControl<T> : Control where T: ITree
    {
        public static TreeMapControl<T> FromTree(T tree)
        {
            var c = new TreeMapControl<T>();
            c.TreeLayout = new TreeMapLayout(tree);
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
                new MenuItem("Zoom In", (s,e) => { ZoomIn(ClickLocation, 1); }),
                new MenuItem("Zoom Out", (s,e) => { ZoomOut(1); })
            });

            this.MouseClick += (s, e) =>
            {
                ClickLocation = e.Location;
                SelectedNode = GetNodeAt(e.Location);
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

            CushionPainter = new CushionPainter<T>(this);
        }

        public T SelectedNode { set; get; }
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

        public CushionPainter<T> CushionPainter;

        public void SetBinnedNodeColor(Func<T, IComparable> f)
        {
            var bins = new Bins(TreeLayout.Tree.GetAllNodes().Cast<T>().Select(f));
            var colorMap = ColorMap.BlueRed(0.0, 1.0);
            CushionPainter.NodeColor = n => colorMap.ToColor(bins.Percentile(f(n)));
        }

        Tree<TreeMapLayout.Layout> GetLayoutAt(Point p)
        {
            return TreeLayout.GetLayoutAt(p.ToArray(), Int32.MaxValue);
        }

        public T GetNodeAt(Point p)
        {
            return (T) GetLayoutAt(p).Data.TreeNode;
        }

        Tree<TreeMapLayout.Layout> GetLayoutAt(Point p, int levels)
        {
            return TreeLayout.GetLayoutAt(p.ToArray(), levels);
        }

        Tree<TreeMapLayout.Layout> hoveredNode;

        void CushionTreeMapControl_SizeChanged(object sender, EventArgs e)
        {
            TreeLayout.Bounds = this.Bounds;
        }

        public TreeMapLayout TreeLayout { set; get; }

        public T Tree
        {
            get
            {
                return (T) TreeLayout.Tree;
            }

            set
            {
                TreeLayout = new TreeMapLayout(value);
                TreeLayout.Bounds = this.Bounds;
                Invalidate();
            }
        }

        public LabelPainter<T> CreateLabelPainter()
        {
            return new LabelPainter<T>(this);
        }

        public void Highlight(PaintEventArgs e, Tree<TreeMapLayout.Layout> layoutNode, Color color)
        {
            var b = new SolidBrush(Color.FromArgb(128, color));
            e.Graphics.FillRectangle(b, layoutNode.Data.Rectangle.ToRectangleF());
        }

        public void Highlight(PaintEventArgs e, Color color, Func<T, T, bool> f)
        {
            if (MouseHoverNode != null)
            {
                var hovered = (T) MouseHoverNode.Data.TreeNode;
                ForEachLeaf(n =>
                {
                    if (f(hovered, (T) n.Data.TreeNode))
                    {
                        Highlight(e, n, color);
                    }
                });
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            CushionPainter.Paint(e);
            base.OnPaint(e);
        }

        public Tree<TreeMapLayout.Layout> MouseHoverNode
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
        public void Track(PaintEventArgs e, Tree<TreeMapLayout.Layout> tree)
        {
            for (; tree != null; tree = tree.Parent)
            {
                e.Graphics.DrawRectangle(redPen, tree.Data.Rectangle.ToRectangle());
            }
        }

        Pen redPen = new Pen(Color.Red);

        void DrawOutlines(PaintEventArgs e, Tree<TreeMapLayout.Layout> layoutTree)
        {
            var layout = layoutTree.Data;
            e.Graphics.DrawRectangle(redPen, layout.Rectangle.ToRectangle());
            foreach (var i in layoutTree.Children)
            {
                DrawOutlines(e, i);
            }
        }

        public void ForEachNode(Action<Tree<TreeMapLayout.Layout>> a)
        {
            ForEachNode(TreeLayout.LayoutTree, a);
        }

        public void ForEachLeaf(Action<Tree<TreeMapLayout.Layout>> a)
        {
            ForEachLeaf(TreeLayout.LayoutTree, a);
        }

        void ForEachNode(Tree<TreeMapLayout.Layout> layoutTree, 
            Action<Tree<TreeMapLayout.Layout>> a)
        {
            a(layoutTree);
            foreach (var i in layoutTree.Children)
            {
                ForEachNode(i, a);
            }
        }

        void ForEachLeaf(Tree<TreeMapLayout.Layout> layoutTree,
            Action<Tree<TreeMapLayout.Layout>> a)
        {
            if (!layoutTree.Children.Any())
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
            public ItemEventEventArgs(Tree<TreeMapLayout.Layout> layout)
            {
                this.layout = layout;
            }

            public Tree<TreeMapLayout.Layout> Layout
            {
                get
                {
                    return layout;
                }
            }
            Tree<TreeMapLayout.Layout> layout;

            public T Item
            {
                get
                {
                    return (T) layout.Data.TreeNode;
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
            var itree = GetLayoutAt(p, levels).Data.TreeNode;
            Tree = (T)itree;
        }

        public void ZoomOut(int levels)
        {
            var t = TreeLayout.Tree;
            for (int i = 0; i < levels; ++i)
            {
                if (t.Parent != null)
                {
                    t = (T) t.Parent;
                }
                else
                {
                    break;
                }
            }
            Tree = (T) t;
        }
    }
}
