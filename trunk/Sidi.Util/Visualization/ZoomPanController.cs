using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace Sidi.Visualization
{
    public class ZoomPanController
    {
        public ZoomPanController(Control control)
        {
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

            control.MouseClick += (s, e) =>
                {
                    if (e.Button == MouseButtons.Middle)
                    {
                        Transform = new Matrix();
                        control.Invalidate();
                    }
                };
        }

        public Matrix Transform = new Matrix();
    }
}
