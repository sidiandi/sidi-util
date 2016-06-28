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
using Sidi.Forms;
using System.Windows.Media;

namespace Sidi.TreeMap
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

                    var im = Transform.GetInverse();

                    var screenPoint = e.Location.ToPointD();
                    var worldPoint = im.Transform(screenPoint);

                    var t = Transform;
                    t.Translate(-screenPoint.X, -screenPoint.Y);
                    t.Scale(scale, scale);
                    t.Translate(screenPoint.X, screenPoint.Y);
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
                        StartPan(e.Location);
                    }
                };

            control.MouseUp += (s, e) =>
                {
                    StopPan(e.Location);
                };

            control.MouseMove += Control_MouseMove;
        }

        Control control;
        Func<Matrix> getTransform;
        Action<Matrix> setTransform;

        bool panning = false;
        Point panStartLocation;
        Matrix panStartTransform;

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (panning)
            {
                var delta = e.Location.Sub(panStartLocation);
                var t = panStartTransform;
                t.Translate(delta.Width, delta.Height);
                Transform = t;
            }
        }

        void StartPan(Point panStartLocation)
        {
            panning = true;
            panStartTransform = Transform;
            this.panStartLocation = panStartLocation;
        }

        float PanScale { set; get; }

        void StopPan(Point panStopLocation)
        {
            if (panning)
            {
                var delta = panStopLocation.Sub(panStartLocation);
                var t = panStartTransform;
                t.Translate(delta.Width, delta.Height);
                Transform = t;
                panning = false;
            }
        }

        Matrix Transform
        {
            get
            {
                return getTransform();
            }

            set
            {
                var transform = value;

                if (Limits != null)
                {
                    var cr = (RectangleD)this.control.ClientRectangle;
                    var screenLimits = transform.Transform(Limits.Value);
                    if (!screenLimits.Includes(cr))
                    {
                        screenLimits = new[] { screenLimits, cr }.GetEnvelope();
                        var o = cr.Center - screenLimits.Center;
                        transform.Translate(o.X, o.Y);
                        var s = ((cr.Width / screenLimits.Width) + (cr.Height / screenLimits.Height)) * 0.5;
                        s = 1.0 / s;
                        transform.Scale(s, s);
                    }
                }

                setTransform(transform);
                control.Invalidate();
            }
        }

        public void Reset()
        {
            Transform = new Matrix();
        }

        public RectangleD? Limits
        {
            get { return limits; }
            set
            {
                // limits = value;
                // Transform = Transform;
            }
        }
        RectangleD? limits;

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
                control.MouseMove -= Control_MouseMove;
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
