using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

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

    public static class ArrayRectEx
    {
        public static float Area(this float[,] r)
        {
            var w = r[0, 1] - r[0, 0];
            var h = r[1, 1] - r[1, 0];
            return w * h;
        }

        const float cf = 1.0f / 256.0f;

        public static float[] ToArray(this Color c)
        {
            return new float[]
            {
                c.B * cf,
                c.G * cf,
                c.R * cf
            };
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
    }
}
