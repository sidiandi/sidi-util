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
            c.TreeMap = new TreeMapLayout<T>(tree);
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
        }

        public T SelectedNode { set; get; }
        public Point ClickLocation { private set; get; }

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

        public Func<T, Color> NodeColor
        {
            set
            {
                TreeMap.GetColor = value;
                RenderCushions();
            }
        }

        public void SetBinnedNodeColor(Func<T, IComparable> f)
        {
            var bins = new Bins(TreeMap.Tree.GetAllNodes().Cast<T>().Select(f));
            var colorMap = ColorMap.BlueRed(0.0, 1.0);
            NodeColor = n => colorMap.ToColor(bins.Percentile(f(n)));
        }

        Tree<TreeMapLayout<T>.Layout> GetLayoutAt(Point p)
        {
            return TreeMap.GetLayoutAt(p.ToArray(), Int32.MaxValue);
        }

        public T GetNodeAt(Point p)
        {
            return GetLayoutAt(p).Data.TreeNode;
        }

        Tree<TreeMapLayout<T>.Layout> GetLayoutAt(Point p, int levels)
        {
            return TreeMap.GetLayoutAt(p.ToArray(), levels);
        }

        Tree<TreeMapLayout<T>.Layout> hoveredNode;

        void CushionTreeMapControl_SizeChanged(object sender, EventArgs e)
        {
            if (cushions != null)
            {
                cushions.Dispose();
                cushions = null;
            }
        }

        TreeMapLayout<T> TreeMap { set; get; }

        public T Tree
        {
            get
            {
                return TreeMap.Tree;
            }

            set
            {
                TreeMap = new TreeMapLayout<T>(value);
                RenderCushions();
            }
        }

        Bitmap cushions;
        void PaintCushions(PaintEventArgs e)
        {
            if (cushions == null || !cushions.Size.Equals(this.ClientSize))
            {
                RenderCushions();
            }
            if (cushions != null)
            {
                e.Graphics.DrawImage(cushions, 0, 0);
            }
        }

        public void RenderCushions()
        {
            if (cushions != null)
            {
                cushions.Dispose();
                cushions = null;
            }

            var rect = new RectangleF(0, 0, ClientSize.Width, ClientSize.Height);
            if (rect.Width > 0 && rect.Height > 0)
            {
                TreeMap.Bounds = rect;
                cushions = TreeMap.Render();
            }
            Invalidate();
        }

        public LabelPainter CreateLabelPainter()
        {
            return new LabelPainter(this.TreeMap);
        }

        public class LabelPainter
        {
            public StringFormat StringFormat;

            TreeMapLayout<T> treeMap;

            public LabelPainter(TreeMapLayout<T> treeMap)
            {
                this.treeMap = treeMap;
                StringFormat = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                Font = new Font(FontFamily.GenericSansSerif, 10.0f);
            }

            public float MinArea = 100.0f;

            public void Paint(PaintEventArgs e)
            {
                float maxArea = treeMap.LayoutTree.Data.Rectangle.Area();
                alphaF = 220.0 / (Math.Log10(maxArea) - Math.Log10(MinArea));
                PaintRecursive(e, treeMap.LayoutTree);
            }

            public Font Font;

            public Func<T, string> Text = t => t.ToString();

            double alphaF;

            bool PaintRecursive(PaintEventArgs e, Tree<TreeMapLayout<T>.Layout> n)
            {
                var rectArea = n.Data.Rectangle.Area();
                if (rectArea < MinArea || !e.ClipRectangle.IntersectsWith(n.Data.Rectangle.ToRectangle()))
                {
                    return false;
                }

                var a50 = Math.Max(rectArea * 0.6, MinArea);

                if (
                    n.Parent != null && 
                    (
                        n.Children.All(c => c.Data.Rectangle.Area() < a50)
                    ))
                {
                    var rect = n.Data.Rectangle.ToRectangleF();
                    var text = Text(n.Data.TreeNode);
                    var textSize = e.Graphics.MeasureString(text, Font);
                    var scale = Math.Min(rect.Width / Math.Max(1.0f, textSize.Width), rect.Height / Math.Max(1.0f, textSize.Height));
                    if ((scale * textSize.Height * 8) < rect.Height)
                    {
                        scale *= 2;
                    }

                    if (scale < 1.0)
                    {
                        return false;
                    }

                    var a = Util.ClipByte(255.0 - (Math.Log10(n.Data.Rectangle.Area()) - Math.Log10(MinArea)) * alphaF);

                    var white = new SolidBrush(Color.FromArgb(a, Color.White));
                    var font = new Font(FontFamily.GenericSansSerif, Font.Size * scale);

                    e.Graphics.DrawString(
                        text,
                        font,
                        white,
                        rect,
                        StringFormat
                        );

                    /*
                    var gp = new GraphicsPath();
                    gp.AddString(text, FontFamily.GenericSansSerif, (int)FontStyle.Regular, font.Size, rect, StringFormat);
                    var outlinePen = new Pen(Color.FromArgb(a, Color.Black), 3.0f);
                    e.Graphics.DrawPath(outlinePen, gp);
                    e.Graphics.FillPath(white, gp);
                     */
                }

                foreach (var c in n.Children)
                {
                    PaintRecursive(e, c);
                }

                return true;
            }
        }

        public void Highlight(PaintEventArgs e, Tree<TreeMapLayout<T>.Layout> layoutNode, Color color)
        {
            var b = new SolidBrush(Color.FromArgb(128, color));
            e.Graphics.FillRectangle(b, layoutNode.Data.Rectangle.ToRectangleF());
        }

        public void Highlight(PaintEventArgs e, Color color, Func<T, T, bool> f)
        {
            if (MouseHoverNode != null)
            {
                var h = MouseHoverNode.Data.TreeNode;
                ForEachLeaf(n =>
                {
                    if (f(h, n.Data.TreeNode))
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

        public Tree<TreeMapLayout<T>.Layout> MouseHoverNode
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
        public void Track(PaintEventArgs e, Tree<TreeMapLayout<T>.Layout> tree)
        {
            for (; tree != null; tree = tree.Parent)
            {
                e.Graphics.DrawRectangle(redPen, tree.Data.Rectangle.ToRectangle());
            }
        }

        Pen redPen = new Pen(Color.Red);

        void DrawOutlines(PaintEventArgs e, Tree<TreeMapLayout<T>.Layout> layoutTree)
        {
            var layout = layoutTree.Data;
            e.Graphics.DrawRectangle(redPen, layout.Rectangle.ToRectangle());
            foreach (var i in layoutTree.Children)
            {
                DrawOutlines(e, i);
            }
        }

        public void ForEachNode(Action<Tree<TreeMapLayout<T>.Layout>> a)
        {
            ForEachNode(TreeMap.LayoutTree, a);
        }

        public void ForEachLeaf(Action<Tree<TreeMapLayout<T>.Layout>> a)
        {
            ForEachLeaf(TreeMap.LayoutTree, a);
        }

        void ForEachNode(Tree<TreeMapLayout<T>.Layout> layoutTree, 
            Action<Tree<TreeMapLayout<T>.Layout>> a)
        {
            a(layoutTree);
            foreach (var i in layoutTree.Children)
            {
                ForEachNode(i, a);
            }
        }

        void ForEachLeaf(Tree<TreeMapLayout<T>.Layout> layoutTree,
            Action<Tree<TreeMapLayout<T>.Layout>> a)
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
            public ItemEventEventArgs(Tree<TreeMapLayout<T>.Layout> layout)
            {
                this.layout = layout;
            }

            public Tree<TreeMapLayout<T>.Layout> Layout
            {
                get
                {
                    return layout;
                }
            }
            Tree<TreeMapLayout<T>.Layout> layout;

            public T Item
            {
                get
                {
                    return layout.Data.TreeNode;
                }
            }
        }
        
        public delegate void ItemEventHandler(object sender, ItemEventEventArgs e);

        public event ItemEventHandler ItemMouseHover;
        public event ItemEventHandler ItemActivate;

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
                    t = (T) t.Parent;
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
