using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sidi.Treemapping
{
    public class View : Control
    {
        public View()
        {
            Tree = new TreeNode(null) { Size = 1.0 };
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.StandardClick, true);
            Size = new Size(100, 100);

            zoomPanController = new ZoomPanController(this, () => WorldToScreen.ToMatrixF(), t => this.WorldToScreen = t.ToMatrixD());
            labelPainterController = new LabelPainterController(this, labelPainter);
            levelZoomPanController = new LevelZoomPanController(this);
        }

        public System.Windows.Point GetWorldPoint(Point clientPoint)
        {
            return WorldToScreen.GetInverse().Transform(clientPoint.ToPointD());
        }

        public TreeNode Tree
        {
            get
            {
                return m_Tree;
            }

            set
            {
                m_Tree = value;
                UpdateLayout();
            }

        }
        TreeNode m_Tree;

        void UpdateLayout()
        {
            Tree.Rectangle = this.ClientRectangle;
            Tree.Squarify();
            this.cushionPainter.Clear();
            Invalidate();
        }

        public System.Windows.Media.Matrix WorldToScreen
        {
            get
            {
                return m_transform;
            }

            set
            {
                m_transform = value;
                Invalidate();
            }
        }
        System.Windows.Media.Matrix m_transform = new System.Windows.Media.Matrix();

        ZoomPanController zoomPanController;
        LabelPainterController labelPainterController;
        LevelZoomPanController levelZoomPanController;

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateLayout();
        }

        TiledCushionPainter cushionPainter = new TiledCushionPainter(new CushionPainter());
        LabelPainter labelPainter = new LabelPainter();

        protected override void OnPaint(PaintEventArgs e)
        {
            var screenToWorld = WorldToScreen.GetInverse();

            var tpe = new TreePaintArgs
            {
                PaintEventArgs = e,
                WorldToScreen = WorldToScreen,
                ScreenToWorld = screenToWorld,
                Tree = this.Tree,
                ScreenRect = this.ClientRectangle,
            };

            cushionPainter.Paint(tpe);
            labelPainter.Paint(tpe);
            // PaintWireFrame(e, Transform, Tree);
        }

        void PaintWireFrame(PaintEventArgs e, System.Windows.Media.Matrix transform, TreeNode tree)
        {
            var pen = Pens.Black;
            foreach (var i in Tree.GetLeafs())
            {
                var r = transform.Transform(i.Rectangle);
                e.Graphics.DrawRectangle(pen, (float)r.Left, (float)r.Top, (float)r.Width, (float)r.Height);
            }
        }
    }
}
