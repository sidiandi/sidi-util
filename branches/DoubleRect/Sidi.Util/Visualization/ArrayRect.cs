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

    public static class ArrayRectExtensions
    {
        public static float Area(this float[,] rectangle)
        {
            var w = rectangle[0, 1] - rectangle[0, 0];
            var h = rectangle[1, 1] - rectangle[1, 0];
            return w * h;
        }

        const float cf = 1.0f / 256.0f;

        public static float[] ToArray(this Color color)
        {
            return new float[]
            {
                color.B * cf,
                color.G * cf,
                color.R * cf
            };
        }

        public static float Width(this float[,] r, Dir direction)
        {
            return r[(int)direction, (int)Bound.Max] - r[(int)direction, (int)Bound.Min];
        }

        public static float[,] ToArray(this RectangleF r)
        {
            var ar = new float[2, 2];
            ar[(int)Dir.X, (int)Bound.Min] = r.Left;
            ar[(int)Dir.X, (int)Bound.Max] = r.Right;
            ar[(int)Dir.Y, (int)Bound.Min] = r.Top;
            ar[(int)Dir.Y, (int)Bound.Max] = r.Bottom;
            return ar;
        }

        public static float[] ToArray(this Point p)
        {
            return new float[] { p.X, p.Y };
        }

        public static void Add(this float[,] a, float[,] b)
        {
            a[0, 0] += b[0, 0];
            a[0, 1] += b[0, 1];
            a[1, 0] += b[1, 0];
            a[1, 1] += b[1, 1];
        }

        public static Rectangle ToRectangle(this float[,] r)
        {
            return Rectangle.FromLTRB(
                (int)r[(int)Dir.X, (int)Bound.Min],
                (int)r[(int)Dir.Y, (int)Bound.Min],
                (int)r[(int)Dir.X, (int)Bound.Max],
                (int)r[(int)Dir.Y, (int)Bound.Max]);
        }

        public static RectangleF ToRectangleF(this float[,] r)
        {
            return RectangleF.FromLTRB(
                r[(int)Dir.X, (int)Bound.Min],
                r[(int)Dir.Y, (int)Bound.Min],
                r[(int)Dir.X, (int)Bound.Max],
                r[(int)Dir.Y, (int)Bound.Max]);
        }

        public static bool Contains(this float[,] r, float[] p)
        {
            return
                r[0, 0] <= p[0] &&
                r[0, 1] > p[0] &&
                r[1, 0] <= p[1] &&
                r[1, 1] > p[1];
        }

        public static bool CoversPixel(this float[,] r)
        {
            return ((int)r[0, 0] != (int)r[0, 1])
                && ((int)r[1, 0] != (int)r[1, 1]);
        }

        public static float[,] Copy(this float[,] x)
        {
            return (float[,])x.Clone();
        }

        public static float[,] Create(PointF p0, PointF p1)
        {
            var r = new float[2, 2];
            r[(int)Dir.X, (int)Bound.Min] = p0.X;
            r[(int)Dir.X, (int)Bound.Max] = p1.X;
            r[(int)Dir.Y, (int)Bound.Min] = p0.Y;
            r[(int)Dir.Y, (int)Bound.Max] = p1.Y;
            return r;
        }

        public static RectangleF ToRect(float[,] f)
        {
            return new RectangleF(
                f[(int)Dir.X, (int)Bound.Min],
                f[(int)Dir.Y, (int)Bound.Min],
                f.Width(),
                f.Height());
        }

        public static float Width(this float[,] f)
        {
            return f[(int)Dir.X, (int)Bound.Max] - f[(int)Dir.X, (int)Bound.Min];
        }

        public static float Height(this float[,] f)
        {
            return f[(int)Dir.Y, (int)Bound.Max] - f[(int)Dir.Y, (int)Bound.Min];
        }

        public static bool Intersects(this float[,] a, float[,] b)
        {
            return ToRect(a).IntersectsWith(ToRect(b));
        }

        public static float[,] Create(float x0, float y0, float x1, float y1)
        {
            var r = new float[2, 2];
            r[(int)Dir.X, (int)Bound.Min] = x0;
            r[(int)Dir.X, (int)Bound.Max] = x1;
            r[(int)Dir.Y, (int)Bound.Min] = y0;
            r[(int)Dir.Y, (int)Bound.Max] = y1;
            return r;
        }

        public static float[,] Create(RectangleF rect)
        {
            var r = new float[2, 2];
            r[(int)Dir.X, (int)Bound.Min] = rect.Left;
            r[(int)Dir.X, (int)Bound.Max] = rect.Right;
            r[(int)Dir.Y, (int)Bound.Min] = rect.Top;
            r[(int)Dir.Y, (int)Bound.Max] = rect.Bottom;
            return r;
        }

        public static void Transform(this Matrix m, float[,] x)
        {
            var p = new PointF[]{ new PointF(x[(int)Dir.X, (int)Bound.Min], x[(int)Dir.Y, (int)Bound.Min]),
                new PointF(x[(int)Dir.X, (int)Bound.Max], x[(int)Dir.Y, (int)Bound.Max])};

            m.TransformPoints(p);
            x[(int)Dir.X, (int)Bound.Min] = p[0].X;
            x[(int)Dir.X, (int)Bound.Max] = p[1].X;
            x[(int)Dir.Y, (int)Bound.Min] = p[0].Y;
            x[(int)Dir.Y, (int)Bound.Max] = p[1].Y;
        }

        public static void Transform(this Matrix m, float[] x)
        {
            var p = new PointF[]{ new PointF(x[(int)Dir.X], x[(int)Dir.Y]) };
            m.TransformPoints(p);
            x[(int)Dir.X] = p[0].X;
            x[(int)Dir.Y] = p[0].Y;
        }
    }
}
