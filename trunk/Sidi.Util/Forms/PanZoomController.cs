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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sidi.Forms
{
    public class PanZoomController : IDisposable
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



        private bool disposed = false;

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    this.m_mouseDelta.Dispose();
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~PanZoomController()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

    }

}
