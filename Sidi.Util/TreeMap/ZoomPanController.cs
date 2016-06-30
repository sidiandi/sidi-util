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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                    Transform = EnforceLimits(t);
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
                t = EnforceLimitsSoft(t);
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

        static double f(double x)
        {
            var s = 0.01;
            return x - ((Math.Sqrt(Math.Abs(x) * s + 1.0) - 1.0) / s * Math.Sign(x));
        }

        static double t(double a0, double a1, double b0, double b1)
        {
            if ((a1 - a0) > (b1 - b0))
            {
                // screen is bigger than object
                return (a0 + a1) / 2 - (b0 + b1) / 2;
            }
            else
            {
                if (a0 < b0)
                {
                    return a0 - b0;
                }
                if (b1 < a1)
                {
                    return a1 - b1;
                }
                return 0;
            }
        }

        Matrix EnforceLimitsSoft(Matrix m)
        {
            var d = GetEnforceLimitsTranslation(m);
            m.Translate(f(d.X), f(d.Y));
            return m;
        }

        System.Windows.Point GetEnforceLimitsTranslation(Matrix m)
        {
            if (Limits != null)
            {
                var screen = (RectangleD)this.control.ClientRectangle;
                var limits = m.Transform(Limits.Value);

                return new System.Windows.Point(
                    t(screen.Left, screen.Right, limits.Left, limits.Right),
                    t(screen.Top, screen.Bottom, limits.Top, limits.Bottom));
            }
            else
            {
                return new System.Windows.Point(0, 0);
            }
        }

        Matrix EnforceLimits(Matrix m)
        {
            var d = GetEnforceLimitsTranslation(m);
            m.Translate(d.X, d.Y);
            return m;
        }

        void StopPan(Point panStopLocation)
        {
            if (panning)
            {
                var delta = panStopLocation.Sub(panStartLocation);
                var t = panStartTransform;
                t.Translate(delta.Width, delta.Height);
                t = EnforceLimits(t);
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
                limits = value;
                Transform = Transform;
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
