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
using System.Drawing.Drawing2D;
using System.Drawing;
using Sidi.Forms;

namespace Sidi.Visualization
{
    public class ZoomPanController : IDisposable
    {
        public ZoomPanController(Control control)
        {
            PanScale = 5.0f;

            this.control = control;

            control.MouseWheel += (s, e) =>
                {
                    float scale = (float) Math.Pow(2.0, ((double)e.Delta) * 0.001);
                    var screenPoint = new PointF(e.X, e.Y);
                    
                    var im = Transform.Clone(); im.Invert();

                    var worldPoint = new[] { screenPoint };
                    im.TransformPoints(worldPoint);

                    Transform.Translate(-screenPoint.X, -screenPoint.Y, MatrixOrder.Append);
                    Transform.Scale(scale, scale, MatrixOrder.Append);
                    Transform.Translate(screenPoint.X, screenPoint.Y, MatrixOrder.Append);
                    control.Invalidate();
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
                    Transform.Translate(me.Delta.X * PanScale, me.Delta.Y * PanScale, MatrixOrder.Append);
                    control.Invalidate();
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

        public Matrix Transform = new Matrix();

        public void Reset()
        {
            Transform = new Matrix();
            control.Invalidate();
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
                Transform.Dispose();
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
