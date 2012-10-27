// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using Sidi.Util;
using System.Diagnostics;

namespace Sidi.Forms
{
    public class Viewport : Control
    {
        Rectangle m_scrollRectFixed;
        Control m_viewportWindow;
        HScrollBar m_hScrollBar;
        VScrollBar m_vScrollBar;
        Size m_smallChange = new Size(64,64);
        Point m_offset;

        public Control InternalViewportWindow
        {
            get { return m_viewportWindow; } 
        }
        
        public enum ScrollRectMode
        {
            Fixed,
            SizeToWindow
        }

        ScrollRectMode m_hScrollRectMode = ScrollRectMode.Fixed;
        ScrollRectMode m_vScrollRectMode = ScrollRectMode.Fixed;

        public ScrollRectMode ScrollRectModeHorizontal
        {
            set
            {
                m_hScrollRectMode = value; 
                PerformLayout();
            }
            get { return m_hScrollRectMode; }
        }

        public ScrollRectMode ScrollRectModeVertical
        {
            set
            {
                m_vScrollRectMode = value;
                PerformLayout();
            }
            get { return m_vScrollRectMode; }
        }

        class ViewportWindow : Control
        {
            public ViewportWindow()
            {
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
//                    ControlStyles.Selectable | 
                    ControlStyles.UserMouse |
                    ControlStyles.UserPaint |
                    0 , true);
            }

            protected override bool IsInputKey(Keys keyData)
            {
                return true;
            }
        }
        
        public Viewport()
        {
            SetStyle(ControlStyles.UserPaint, false);
            SetStyle(ControlStyles.Selectable, true);

            SuspendLayout();
            m_viewportWindow = new ViewportWindow();
            m_viewportWindow.Visible = true;
            m_viewportWindow.Paint += new PaintEventHandler(m_viewportWindow_Paint);
            m_viewportWindow.TabStop = true;
            Controls.Add(m_viewportWindow);

            m_hScrollBar = new HScrollBar();
            m_hScrollBar.Scroll += new ScrollEventHandler(ScrollBar_HScroll);
            Controls.Add(m_hScrollBar);

            m_vScrollBar = new VScrollBar();
            m_vScrollBar.Scroll += new ScrollEventHandler(ScrollBar_VScroll);
            Controls.Add(m_vScrollBar);

            m_viewportWindow.MouseWheel += new MouseEventHandler(Viewport_MouseWheel);
            m_viewportWindow.KeyDown += new KeyEventHandler(Viewport_KeyDown);
            m_viewportWindow.MouseClick += new MouseEventHandler(m_viewportWindow_MouseClick);
            m_viewportWindow.MouseMove += new MouseEventHandler(m_viewportWindow_MouseMove);
            m_viewportWindow.MouseDoubleClick += new MouseEventHandler(m_viewportWindow_MouseDoubleClick);
            m_viewportWindow.MouseDown += new MouseEventHandler(m_viewportWindow_MouseDown);
            ResumeLayout(false);
        }

        void m_viewportWindow_MouseDown(object sender, MouseEventArgs e)
        {
            this.OnWorldMouseDown(ToWorld(e));
        }

        void m_viewportWindow_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.OnWorldMouseDoubleClick(ToWorld(e));
        }

        void m_viewportWindow_MouseMove(object sender, MouseEventArgs e)
        {
            this.OnWorldMouseMove(ToWorld(e));
        }

        void m_viewportWindow_MouseClick(object sender, MouseEventArgs e)
        {
            this.OnWorldMouseClick(ToWorld(e));
        }

        protected virtual void OnWorldMouseDown(MouseEventArgs e)
        {
        }

        protected virtual void OnWorldMouseDoubleClick(MouseEventArgs e)
        {
        }

        protected virtual void OnWorldMouseClick(MouseEventArgs e)
        {
        }

        protected virtual void OnWorldMouseMove(MouseEventArgs e)
        {
        }

        public MouseEventArgs ToWorld(MouseEventArgs e)
        {
            Point wp = ToWorld(new Point(e.X, e.Y));
            return new MouseEventArgs(e.Button, e.Clicks, wp.X, wp.Y, e.Delta);
        }

        public Point ToWorld(Point p)
        {
            return new Point(p.X + m_offset.X, p.Y + m_offset.Y);
        }

        void Viewport_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);

            if (e.Handled)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.PageDown:
                    ScrollPages(new Size(0, 1));
                    break;
                case Keys.PageUp:
                    ScrollPages(new Size(0, -1));
                    break;
                default:
                    e.Handled = false;
                    return;
            }

            e.Handled = true;
        }

        void Viewport_MouseWheel(object sender, MouseEventArgs e)
        {
            ScrollSteps(new Size(0, - e.Delta / Sidi.Forms.MouseWheelSupport.Unit));
        }

        public void ScrollSteps(Size d)
        {
            Point offset = Offset;
            offset.X += d.Width * m_smallChange.Width;
            offset.Y += d.Height * m_smallChange.Height;
            Offset = offset;
        }

        public void ScrollPages(Size d)
        {
            Point offset = Offset;
            offset.X += d.Width * m_viewportWindow.Width;
            offset.Y += d.Height * m_viewportWindow.Height;
            Offset = offset;
        }

        void ScrollBar_HScroll(object sender, ScrollEventArgs e)
        {
            Point p = new Point(e.NewValue, m_vScrollBar.Value);
            SetOffsetInternal(p);
        }

        void ScrollBar_VScroll(object sender, ScrollEventArgs e)
        {
            Point p = new Point(m_hScrollBar.Value, e.NewValue);
            SetOffsetInternal(p);
        }

        void ReadScrollbars()
        {
            Point p = new Point(m_hScrollBar.Value, m_vScrollBar.Value);
            SetOffsetInternal(p);
        }

        void SyncScrollbars()
        {
            SetClippedValue(m_hScrollBar, m_offset.X);
            SetClippedValue(m_vScrollBar, m_offset.Y);
        }

        public Point Offset
        {
            get { return m_offset; }
            set
            {
                SetOffsetInternal(value);
                SyncScrollbars();
            }
        }

        void SetOffsetInternal(Point value)
        {
            m_offset = value;
            Rectangle sr = ScrollRect;
            if (m_offset.X > sr.Right - m_viewportWindow.Width)
            {
                m_offset.X = sr.Right - m_viewportWindow.Width;
            }
            if (m_offset.Y > sr.Bottom - m_viewportWindow.Height)
            {
                m_offset.Y = sr.Bottom - m_viewportWindow.Height;
            }
            if (m_offset.X < sr.Left)
            {
                m_offset.X = sr.X;
            }
            if (m_offset.Y < sr.Top)
            {
                m_offset.Y = sr.Y;
            }

            // TODO scroll window
            m_viewportWindow.Invalidate();
        }

        public void InvalidateViewport()
        {
            m_viewportWindow.Invalidate();
        }

        public void InvalidateWorld(Rectangle r)
        {
            r.Offset(- Offset.X, - Offset.Y);
            m_viewportWindow.Invalidate(r);
        }

        void m_viewportWindow_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle cr = e.ClipRectangle;
            cr.Offset(m_offset);
            g.TranslateTransform(-m_offset.X, -m_offset.Y);
            using (var pea = new PaintEventArgs(g, cr))
            {
                OnPaint(pea);
            }
        }

        protected override void  OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle cr = e.ClipRectangle;
            g.FillRectangle(Brushes.White, cr);
            int step = 100;
            for (int x = GraMath.Floor(cr.Left, step); x < cr.Right; x += step)
            {
                for (int y = GraMath.Floor(cr.Top, step); y < cr.Bottom; y += step)
                {
                    g.DrawString(String.Format("{0},{1}", x, y), this.Font, Brushes.Red, new PointF(x, y));
                }
            }
            base.OnPaint(e);
        }

        public Rectangle ScrollRect
        {
            get
            {
                Rectangle r = m_scrollRectFixed;
                if (m_hScrollRectMode == ScrollRectMode.SizeToWindow)
                {
                    r.X = 0;
                    r.Width = m_viewportWindow.Width;
                }
                if (m_vScrollRectMode == ScrollRectMode.SizeToWindow)
                {
                    r.Y = 0;
                    r.Height = m_viewportWindow.Height;
                }
                return r;
            }
            set
            {
                m_scrollRectFixed = value;
                PerformLayout();
            }
        }

        public Rectangle VisibleRect
        {
            get
            {
                return new Rectangle(m_offset.X, m_offset.Y, m_viewportWindow.Width, m_viewportWindow.Height);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            
            Rectangle c = ClientRectangle;
            Rectangle sr = ScrollRect;

            bool hScrollBarVisible = (c.Width < sr.Width);
            bool vScrollBarVisible = (c.Height < sr.Height);

            Size scrollbarSize = new Size(
                vScrollBarVisible ? m_vScrollBar.Size.Width : 0,
                hScrollBarVisible ? m_hScrollBar.Size.Height : 0);

            if (hScrollBarVisible)
            {
                m_hScrollBar.Bounds = new Rectangle
                (
                    c.Left, c.Bottom - scrollbarSize.Height,
                    c.Width - scrollbarSize.Width, scrollbarSize.Height
                );
            }
            m_hScrollBar.Visible = hScrollBarVisible;

            if (vScrollBarVisible)
            {
                m_vScrollBar.Bounds = new Rectangle
                (
                    c.Right - scrollbarSize.Width, c.Top,
                    scrollbarSize.Width, c.Height - scrollbarSize.Height
                );
            }
            m_vScrollBar.Visible = vScrollBarVisible;

            m_viewportWindow.Bounds = new Rectangle(
                c.Left, c.Top,
                c.Width - scrollbarSize.Width, c.Height - scrollbarSize.Height);

            if (hScrollBarVisible)
            {
                m_hScrollBar.Minimum = sr.Left;
                m_hScrollBar.Maximum = sr.Right;
                m_hScrollBar.LargeChange = System.Math.Max(0, m_viewportWindow.Width);
                m_hScrollBar.SmallChange = m_smallChange.Width;
                ClipValue(m_hScrollBar);
            }

            if (vScrollBarVisible)
            {
                m_vScrollBar.Minimum = sr.Top;
                m_vScrollBar.Maximum = sr.Bottom;
                m_vScrollBar.LargeChange = System.Math.Max(0, m_viewportWindow.Height);
                m_vScrollBar.SmallChange = m_smallChange.Height;
                ClipValue(m_vScrollBar);
            }

            ReadScrollbars();
        }

        void ClipValue(ScrollBar sb)
        {
            int v = sb.Value;
            if (v > sb.Maximum - sb.LargeChange)
            {
                v = sb.Maximum - sb.LargeChange;
            }
            if (v < sb.Minimum)
            {
                v = sb.Minimum;
            }
        }

        void SetClippedValue(ScrollBar sb, int v)
        {
            if (v > sb.Maximum - sb.LargeChange)
            {
                v = sb.Maximum - sb.LargeChange;
            }
            if (v < sb.Minimum)
            {
                v = sb.Minimum;
            }
            sb.Value = v;
        }

        public Size SmallChange
        {
            get { return m_smallChange; }
            set { m_smallChange = value; }
        }

        public void EnsureVisible(Rectangle r)
        {
            if (!GraMath.Contains(VisibleRect, r))
            {
                Rectangle v = VisibleRect;
                Point o = new Point();
                
                if (r.Left < v.Left)
                {
                    o.X = r.Right - v.Width;
                }

                if (v.Right < r.Right)
                {
                    o.X = r.Left;
                }

                if (r.Top < v.Top)
                {
                    o.Y = r.Bottom - v.Height;
                }

                if (v.Bottom < r.Bottom)
                {
                    o.Y = r.Top;
                }

                this.Offset = o;
            }
        }
    }
}
