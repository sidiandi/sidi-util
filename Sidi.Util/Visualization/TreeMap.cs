// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Sidi.IO;
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
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.StandardClick, true);

            this.SizeChanged += new EventHandler(CushionTreeMapControl_SizeChanged);
            this.MouseMove += new MouseEventHandler(CushionTreeMapControl_MouseMove);

            this.MouseDoubleClick += (s, e) =>
            {
                if (ItemActivate != null)
                {
                    ItemActivate(this, new TreeEventArgs(GetLayoutAt(e.Location)));
                }
            };

            zoomPanController = new ZoomPanController(this);

            this.ContextMenuStrip = new ContextMenuStrip();

            this.ContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(ContextMenuStrip_Opening);
                
            this.MouseClick += (s, e) =>
            {
                this.Focus();
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (CanSelect) Select();
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
            foreach (var i in layout.Up.Cast<Sidi.Visualization.Layout>())
            {
                var tree = i.Tree;
                strip.Items.Add(
                    String.Format("View {0}", i.Tree.SafeToString()), null, (s, e) =>
                        {
                            Tree = tree;
                        });
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static TreeMap FromTree(Tree tree)
        {
            return new TreeMap() { Tree = tree };
        }

        public LayoutManager LayoutManager
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

        LayoutManager _layout;
        ZoomPanController zoomPanController;
        public CushionPainter CushionPainter;
        Layout hoveredNode;

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
                    ItemMouseHover(this, new TreeEventArgs(hoveredNode));
                }
            }
        }

        Layout GetLayoutAt(Point p)
        {
            if (LayoutManager == null)
            {
                return null;
            }

            return LayoutManager.GetLayoutAt(GetWorldPoint(p), Int32.MaxValue);
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

        public System.Windows.Point GetWorldPoint(Point p)
        {
            var fp = p.ToPointD();
            var inverse = WorldTransform.ToMatrixD();
            inverse.Invert();
            return inverse.Transform(fp);
        }

        public object GetObjectAt(Point p)
        {
            var l = GetLayoutAt(p);
            return l == null ? null : l.Tree.Object;
        }

        Layout GetLayoutAt(Point p, int levels)
        {
            return LayoutManager.GetLayoutAt(GetWorldPoint(p), levels);
        }

        void CushionTreeMapControl_SizeChanged(object sender, EventArgs e)
        {
            Tree = Tree;
        }

        public Tree Tree
        {
            get
            {
                return LayoutManager == null ? null : LayoutManager.Root.Tree;
            }

            set
            {
                if (value == null || value.Size == 0.0)
                {
                    LayoutManager = null;
                }
                else
                {
                    LayoutManager = new LayoutManager(value, this.Bounds);
                }
            }
        }

        public LabelPainter CreateLabelPainter()
        {
            return new LabelPainter(this);
        }

        public void Highlight(PaintEventArgs e, Layout layoutNode, Color color)
        {
            using (var b = new SolidBrush(Color.FromArgb(128, color)))
            {
                e.Graphics.FillRectangle(b, layoutNode.Bounds.ToRectangleF());
            }
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

        void DrawWireFrame(PaintEventArgs e, Layout layout)
        {
            using (var pen = new Pen(Color.Red))
            {
                e.Graphics.DrawRectangle(pen, layout.Bounds.ToRectangle());
                foreach (var i in layout.Children)
                {
                    DrawWireFrame(e, i);
                }
            }
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Transform = zoomPanController.Transform;

            /*
            DrawWireFrame(e, this.LayoutManager.Root);
            return;
            */

            CushionPainter.Paint(e);
            base.OnPaint(e);
        }

        public Layout MouseHoverNode
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
        public void Track(PaintEventArgs e, Layout tree)
        {
            for (; tree != null; tree = (Layout) tree.Parent)
            {
                e.Graphics.DrawRectangle(redPen, tree.Bounds.ToRectangle());
            }
        }

        Pen redPen = new Pen(Color.Red);

        void DrawOutlines(PaintEventArgs e, Layout layoutTree)
        {
            var layout = layoutTree;
            e.Graphics.DrawRectangle(redPen, layout.Bounds.ToRectangle());
            foreach (var i in layoutTree.Children)
            {
                DrawOutlines(e, i);
            }
        }

        public void ForEachNode(Action<Layout> a)
        {
            ForEachNode(LayoutManager.Root, a);
        }

        public void ForEachLeaf(Action<Layout> a)
        {
            ForEachLeaf(LayoutManager.Root, a);
        }

        void ForEachNode(Layout layoutTree, Action<Layout> a)
        {
            a(layoutTree);
            foreach (var i in layoutTree.Children.Cast<Layout>())
            {
                ForEachNode(i, a);
            }
        }

        void ForEachLeaf(Layout layoutTree, Action<Layout> a)
        {
            if (!layoutTree.Children.Any())
            {
                a(layoutTree);
            }
            foreach (var i in layoutTree.Children.Cast<Layout>())
            {
                ForEachLeaf(i, a);
            }
        }

        public event EventHandler<TreeEventArgs> ItemMouseHover;
        public event EventHandler<TreeEventArgs> ItemActivate;

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
