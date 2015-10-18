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
using System.Drawing.Drawing2D;
using System.Drawing;
using Sidi.Forms;

namespace Sidi.Treemapping
{
    public class ZoomPanController : IDisposable
    {
        public ZoomPanController(Control control, Func<Matrix> getTransform, Action<Matrix> setTransform)
        {
            this.control = control;
            this.getTransform = getTransform;
            this.setTransform = setTransform;

            PanScale = 1.0f;

            control.MouseWheel += (s, e) =>
                {
                    float scale = (float) Math.Pow(2.0, ((double)e.Delta) * 0.001);
                    var screenPoint = new PointF(e.X, e.Y);
                    
                    var im = Transform.Clone(); im.Invert();

                    var worldPoint = new[] { screenPoint };
                    im.TransformPoints(worldPoint);

                    var t = Transform;
                    t.Translate(-screenPoint.X, -screenPoint.Y, MatrixOrder.Append);
                    t.Scale(scale, scale, MatrixOrder.Append);
                    t.Translate(screenPoint.X, screenPoint.Y, MatrixOrder.Append);
                    Transform = t;
                };

            control.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Middle)
                    {
                        Reset();
                    }
                    else if (e.Button == MouseButtons.Left)
                    {
                        StartPan();
                    }
                };

            control.MouseUp += (s, e) =>
                {
                    StopPan();
                };
        }

        void StartPan()
        {
            mouseDelta = new MouseDelta();
            mouseDelta.Move += (ms, me) =>
                {
                    var t = Transform;
                    t.Translate(me.Delta.X * PanScale, me.Delta.Y * PanScale, MatrixOrder.Append);
                    Transform = t;
                };
        }

        float PanScale { set; get; }

        void StopPan()
        {
            if (mouseDelta != null)
            {
                mouseDelta.Dispose();
                mouseDelta = null;
            }
        }

        MouseDelta mouseDelta;
        Control control;
        Func<Matrix> getTransform;
        Action<Matrix> setTransform;

        Matrix Transform
        {
            get
            {
                return getTransform();
            }

            set
            {
                setTransform(value);
                control.Invalidate();
            }
        }

        public void Reset()
        {
            Transform = new Matrix();
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
                if (mouseDelta != null)
                {
                    mouseDelta.Dispose();
                }
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
            disposed = true;
          }
        }

        // Use C# destructor syntax for finalization code.
        ~ZoomPanController()
        {
          // Simply call Dispose(false).
          Dispose(false);
        }    
    }
}
