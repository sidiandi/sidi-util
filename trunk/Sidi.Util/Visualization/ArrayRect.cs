using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sidi.Visualization
{
    public enum Dir
    {
        X,
        Y
    };

    public enum Bound
    {
        Min,
        Max
    };

    static class ArrayRectExtensions
    {
        public static double Area(this double[,] rectangle)
        {
            var w = rectangle[0, 1] - rectangle[0, 0];
            var h = rectangle[1, 1] - rectangle[1, 0];
            return w * h;
        }

        const double cf = 1.0 / 256.0;

        public static double[] ToArray(this Color color)
        {
            return new double[]
            {
                color.B * cf,
                color.G * cf,
                color.R * cf
            };
        }

        public static double Width(this double[,] rectangle, Dir direction)
        {
            return rectangle[(int)direction, (int)Bound.Max] - rectangle[(int)direction, (int)Bound.Min];
        }

        public static double[,] ToArray(this RectangleF r)
        {
            var ar = new double[2, 2];
            ar[(int)Dir.X, (int)Bound.Min] = r.Left;
            ar[(int)Dir.X, (int)Bound.Max] = r.Right;
            ar[(int)Dir.Y, (int)Bound.Min] = r.Top;
            ar[(int)Dir.Y, (int)Bound.Max] = r.Bottom;
            return ar;
        }

        public static double[] ToArray(this Point p)
        {
            return new double[] { p.X, p.Y };
        }

        public static void Add(this double[,] a, double[,] b)
        {
            a[0, 0] += b[0, 0];
            a[0, 1] += b[0, 1];
            a[1, 0] += b[1, 0];
            a[1, 1] += b[1, 1];
        }

        public static Rectangle ToRectangle(this double[,] r)
        {
            return Rectangle.FromLTRB(
                (int)r[(int)Dir.X, (int)Bound.Min],
                (int)r[(int)Dir.Y, (int)Bound.Min],
                (int)r[(int)Dir.X, (int)Bound.Max],
                (int)r[(int)Dir.Y, (int)Bound.Max]);
        }

        public static RectangleF ToRectangleF(this double[,] r)
        {
            return RectangleF.FromLTRB(
                (float) r[(int)Dir.X, (int)Bound.Min],
                (float) r[(int)Dir.Y, (int)Bound.Min],
                (float) r[(int)Dir.X, (int)Bound.Max],
                (float) r[(int)Dir.Y, (int)Bound.Max]);
        }

        public static bool Contains(this double[,] r, double[] p)
        {
            return
                r[0, 0] <= p[0] &&
                r[0, 1] > p[0] &&
                r[1, 0] <= p[1] &&
                r[1, 1] > p[1];
        }

        public static bool CoversPixel(this double[,] rectangle)
        {
            return ((int)rectangle[0, 0] != (int)rectangle[0, 1])
                && ((int)rectangle[1, 0] != (int)rectangle[1, 1]);
        }

        public static double[,] Copy(this double[,] x)
        {
            return (double[,])x.Clone();
        }

        public static double[,] Create(PointF p0, PointF p1)
        {
            var r = new double[2, 2];
            r[(int)Dir.X, (int)Bound.Min] = p0.X;
            r[(int)Dir.X, (int)Bound.Max] = p1.X;
            r[(int)Dir.Y, (int)Bound.Min] = p0.Y;
            r[(int)Dir.Y, (int)Bound.Max] = p1.Y;
            return r;
        }

        public static double Width(this double[,] f)
        {
            return f[(int)Dir.X, (int)Bound.Max] - f[(int)Dir.X, (int)Bound.Min];
        }

        public static double Height(this double[,] f)
        {
            return f[(int)Dir.Y, (int)Bound.Max] - f[(int)Dir.Y, (int)Bound.Min];
        }

        public static bool Intersects(this double[,] a, double[,] b)
        {
            return a.ToRectangleF().IntersectsWith(b.ToRectangleF());
        }

        public static double[,] Create(double x0, double y0, double x1, double y1)
        {
            var r = new double[2, 2];
            r[(int)Dir.X, (int)Bound.Min] = x0;
            r[(int)Dir.X, (int)Bound.Max] = x1;
            r[(int)Dir.Y, (int)Bound.Min] = y0;
            r[(int)Dir.Y, (int)Bound.Max] = y1;
            return r;
        }

        public static double[,] Create(RectangleF rect)
        {
            var r = new double[2, 2];
            r[(int)Dir.X, (int)Bound.Min] = rect.Left;
            r[(int)Dir.X, (int)Bound.Max] = rect.Right;
            r[(int)Dir.Y, (int)Bound.Min] = rect.Top;
            r[(int)Dir.Y, (int)Bound.Max] = rect.Bottom;
            return r;
        }

        public static void Transform(this Matrix m, double[,] x)
        {
            var p = new PointF[]{
                    new PointF((float)x[(int)Dir.X, (int)Bound.Min], (float)x[(int)Dir.Y, (int)Bound.Min]),
                    new PointF((float)x[(int)Dir.X, (int)Bound.Max], (float)x[(int)Dir.Y, (int)Bound.Max])};

            m.TransformPoints(p);
            x[(int)Dir.X, (int)Bound.Min] = p[0].X;
            x[(int)Dir.X, (int)Bound.Max] = p[1].X;
            x[(int)Dir.Y, (int)Bound.Min] = p[0].Y;
            x[(int)Dir.Y, (int)Bound.Max] = p[1].Y;
        }

        public static void Transform(this Matrix m, double[] x)
        {
            var p = new PointF[]{ new PointF((float)x[(int)Dir.X], (float) x[(int)Dir.Y]) };
            m.TransformPoints(p);
            x[(int)Dir.X] = p[0].X;
            x[(int)Dir.Y] = p[0].Y;
        }
    }
}
