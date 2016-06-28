using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Sidi.TreeMap
{
    public static class RectangleDExtensions
    {
        public static RectangleD ToBounds(this System.Drawing.RectangleF rect)
        {
            return RectangleD.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public static RectangleD GetEnvelope(this IEnumerable<RectangleD> rects)
        {
            return RectangleD.FromLTRB(
                rects.Min(_ => _.Left),
                rects.Min(_ => _.Top),
                rects.Max(_ => _.Right),
                rects.Max(_ => _.Bottom));
        }

        public static Point Add(this Point p0, Point p1)
        {
            return new Point(p0.X + p1.X, p0.Y + p1.Y);
        }

        public static System.Drawing.Rectangle ToRectangle(this RectangleD bounds)
        {
            return System.Drawing.Rectangle.FromLTRB(
                (int)bounds.P0.X,
                (int)bounds.P0.Y,
                (int)bounds.P1.X,
                (int)bounds.P1.Y);
        }

        public static System.Drawing.RectangleF ToRectangleF(this RectangleD bounds)
        {
            return System.Drawing.RectangleF.FromLTRB(
                (float)bounds.P0.X,
                (float)bounds.P0.Y,
                (float)bounds.P1.X,
                (float)bounds.P1.Y);
        }

        public static Point ToPointD(this System.Drawing.Point p)
        {
            return new Point(p.X, p.Y);
        }

        public static System.Drawing.PointF ToPointF(this Point p)
        {
            return new System.Drawing.PointF((float)p.X, (float)p.Y);
        }

        public static RectangleD Transform(this Matrix m, RectangleD x)
        {
            return new RectangleD()
            {
                P0 = m.Transform(x.P0),
                P1 = m.Transform(x.P1)
            };
        }

        public static Matrix ToMatrixD(this System.Drawing.Drawing2D.Matrix m)
        {
            return new Matrix(
                m.Elements[0],
                m.Elements[1],
                m.Elements[2],
                m.Elements[3],
                m.Elements[4],
                m.Elements[5]);
        }

        public static System.Drawing.Drawing2D.Matrix ToMatrixF(this Matrix m)
        {
            return new System.Drawing.Drawing2D.Matrix(
                (float)m.M11,
                (float)m.M12,
                (float)m.M21,
                (float)m.M22,
                (float)m.OffsetX,
                (float)m.OffsetY);
        }

        const double cf = 1.0 / 256.0;

        public static double[] ToArray(this System.Drawing.Color color)
        {
            return new double[]
            {
                color.B * cf,
                color.G * cf,
                color.R * cf
            };
        }

        public static double GetAspectRatio(this RectangleD rect)
        {
            double n;
            double d;
            if (rect.Width > rect.Height)
            {
                n = rect.Width;
                d = rect.Height;
            }
            else
            {
                n = rect.Height;
                d = rect.Width;
            }

            if (n == 0)
            {
                return 1.0f;
            }
            else if (d == 0)
            {
                return System.Single.MaxValue;
            }
            else
            {
                return n / d;
            }
        }

        public static Matrix GetInverse(this Matrix m)
        {
            var i = m;
            i.Invert();
            return i;
        }

        public static void Translate(this Matrix m, Point p)
        {
            m.Translate(p.X, p.Y);
        }
    }
}
