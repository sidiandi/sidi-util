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
            GetColor = _ => Color.White;
            GetSize = _ => 1.0;

            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.StandardClick, true);

            levelZoomPanController = new LevelZoomPanController(this);
            zoomPanController = new ZoomPanController(this, () => WorldToScreen, t => this.WorldToScreen = t);
            labelPainterController = new LabelPainterController(this, labelPainter);

            Tree = new Tree<object>();
        }

        public System.Windows.Point GetWorldPoint(Point clientPoint)
        {
            return WorldToScreen.GetInverse().Transform(clientPoint.ToPointD());
        }

        public ITree<TreeLayout> GetNode(Point clientLocation)
        {
            return Layout.GetNodeAt(GetWorldPoint(clientLocation));
        }

        public Func<ITree, Color> GetColor
        {
            get; set;
        }

        public Func<ITree, string> GetLabel
        {
            get;
            set;
        }

        public Func<ITree, double> GetSize
        {
            get;
            set;
        }

        public ITree Tree
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
        ITree m_Tree;


        void UpdateLayout()
        {
            var layout = TreeLayoutExtensions.CreateLayoutTree(this.Tree, GetColor, GetSize);
            layout.Squarify(this.ClientRectangle);
            Layout = layout;
            this.cushionPainter.Clear();
            this.zoomPanController.Limits = Layout.Data.Rectangle;
            Invalidate();
        }

        public ITree<TreeLayout> Layout { get; private set; }

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
                Tree = this.Layout,
                ScreenRect = this.ClientRectangle,
            };

            cushionPainter.Paint(tpe);
            labelPainter.Paint(tpe);
            // PaintWireFrame(tpe);
        }

        void PaintWireFrame(TreePaintArgs tpe)
        {
            var pen = Pens.Red;
            foreach (var i in Layout.GetLeafs())
            {
                var r = tpe.WorldToScreen.Transform(i.Data.Rectangle);
                tpe.PaintEventArgs.Graphics.DrawRectangle(pen, (float)r.Left, (float)r.Top, (float)r.Width, (float)r.Height);
            }
        }
    }
}
