// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using Sidi.Util;

namespace Sidi.Forms
{
    public class ImageView : Control
    {
        Image m_image;
        float m_overscan = 0.1f;
        Matrix m_transform = new Matrix();

        enum ZoomMode
        {
            Manual,
            SizeToFit,
        };

        ZoomMode m_zoomMode = ZoomMode.SizeToFit;

        public ImageView()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.UserPaint | 
                ControlStyles.UserMouse
                , true);

            SetStyle(
                ControlStyles.ResizeRedraw
                , false);
        }
        
        public Image Image
        {
            set
            {
                if (m_image != value)
                {
                    m_image = value;
                    Invalidate();
                }
            }

            get { return m_image; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush backGround = Brushes.Black;

            if (m_image == null)
            {
                g.FillRectangle(backGround, ClientRectangle);
                return;
            }

            g.Transform = Transform;
            RectangleF r = new Rectangle(new Point(0, 0), m_image.Size);
            g.DrawImage(m_image, r);

            Matrix inv = m_transform.Clone();
            inv.Invert();
            RectangleF cr = GraMath.TransformBoundingBox(ClientRectangle, inv);

            if (r.Left > cr.Left)
            {
                g.FillRectangle(backGround, cr.Left, cr.Top, r.Left - cr.Left, cr.Height);
            }

            if (r.Right < cr.Right)
            {
                g.FillRectangle(backGround, r.Right, cr.Top, cr.Right - r.Right, cr.Height);
            }

            if (r.Top > cr.Top)
            {
                g.FillRectangle(backGround, r.Left, cr.Top, r.Width, r.Top - cr.Top);
            }

            if (r.Bottom < cr.Bottom)
            {
                g.FillRectangle(backGround, r.Left, r.Bottom, r.Width, cr.Bottom - r.Bottom);
            }

            g.ResetTransform();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            
            switch (m_zoomMode)
            {
                case ZoomMode.SizeToFit:
                    SizeToFit(m_overscan);
                    break;
                default:
                    break;
            }
        }

        override protected void OnPaintBackground(PaintEventArgs e)
        {
            // do nothing
        }

        public void Pan(PointF d)
        {
            Matrix t = m_transform.Clone();
            t.Translate(d.X, d.Y, MatrixOrder.Append);
            Transform = t;
            Center();
        }

        void Center()
        {
            if (!HasImage)
            {
                return;
            }

            Matrix m = Transform.Clone();
            RectangleF r = GraMath.TransformBoundingBox(new RectangleF(0.0f, 0.0f, (float) m_image.Size.Width, (float) m_image.Size.Height), m);
            Rectangle c = ClientRectangle;

            PointF d = new PointF(0.0f, 0.0f);

            if (c.Width > r.Width)
            {
                d.X = (c.Left + c.Width / 2 - r.Width / 2 - r.Left);
            }
            else
            {
                d.X = Math.Max(0, c.Right - r.Right) + Math.Min(0, c.Left - r.Left);
            }

            if (c.Height > r.Height)
            {
                d.Y = c.Top + c.Height / 2 - r.Height / 2 - r.Top;
            }
            else
            {
                d.Y = Math.Max(0, c.Bottom - r.Bottom) + Math.Min(0, c.Top - r.Top);
            }

            m.Translate(d.X, d.Y, MatrixOrder.Append);

            Transform = m;
        }
        
        public Matrix Transform
        {
            get { return m_transform; }
            set
            {
                m_zoomMode = ZoomMode.Manual;
                InternalTransform = value;
            }
        }

        private Matrix InternalTransform
        {
            get { return m_transform; }
            set
            {
                m_transform = value.Clone();
                Invalidate();
            }
        }

        public void SizeToFit(float overscan)
        {
            m_overscan = overscan;
            m_zoomMode = ZoomMode.SizeToFit;

            if (m_image == null)
            {
                return;
            }

            Rectangle s = new Rectangle(0, 0, m_image.Width, m_image.Height);
            Rectangle i = GraMath.SizeToFit(s, ClientRectangle, overscan);
            InternalTransform = GraMath.CalcTransform(s, i);
        }

        public bool HasImage
        {
            get { return m_image != null; } 
        }
    }

    public class PanZoomController
    {
        ImageView m_view;
        MouseDelta m_mouseDelta;
        float m_panScale = 3.0f;
        float m_overscan = 0.1f;

        public PanZoomController(ImageView view)
        {
            m_view = view;
            view.MouseWheel += new MouseEventHandler(view_MouseWheel);
            view.MouseDoubleClick += new MouseEventHandler(view_MouseDoubleClick);
            view.MouseDown += new MouseEventHandler(view_MouseDown);
            view.MouseUp += new MouseEventHandler(view_MouseUp);
        }

        void view_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_mouseDelta != null)
            {
                m_mouseDelta.Move -= new MouseDeltaEventHandler(m_mouseDelta_Move);
                m_mouseDelta.Dispose();
                m_mouseDelta = null;
            }
        }

        void view_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                m_mouseDelta = new MouseDelta();
                m_mouseDelta.Move += new MouseDeltaEventHandler(m_mouseDelta_Move);
            }
        }

        void m_mouseDelta_Move(object sender, MouseDeltaEventArgs e)
        {
            m_view.Pan(new PointF(e.Delta.X * m_panScale, e.Delta.Y * m_panScale));
        }

        void view_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            m_view.SizeToFit(m_overscan);
        }

        void view_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Shift)
            {
                float s = (float)(90 * Math.Floor(e.Delta / 120.0));
                Matrix m = m_view.Transform.Clone();
                m.Translate(-(float)e.X, -(float)e.Y, MatrixOrder.Append);
                m.Rotate(s, MatrixOrder.Append);
                m.Translate((float)e.X, (float)e.Y, MatrixOrder.Append);
                m_view.Transform = m;
            }
            else
            {
                float s = (float)Math.Pow(1.5, (double)Math.Sign(e.Delta));
                Matrix m = m_view.Transform.Clone();
                m.Translate(-(float)e.X, -(float)e.Y, MatrixOrder.Append);
                m.Scale(s, s, MatrixOrder.Append);
                m.Translate((float)e.X, (float)e.Y, MatrixOrder.Append);
                m_view.Transform = m;
            }
        }
    }
}
