// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace Sidi.Util
{
    public class GraMath
    {
        static public Rectangle SizeToFit(Rectangle s, Rectangle d, double os)
        {
            if (s.Width == 0 || s.Height == 0)
            {
                return new Rectangle();
            }

            double o;
            if (s.Width * d.Height > d.Width * s.Height)
            {
                o = 0.0;
            }
            else
            {
                o = 1.0;
            }

            double scale =
                o * ((double)(d.Width) / (double)(s.Width)) +
                (1 - o) * ((double)(d.Height) / (double)(s.Height));

            if (os < 1.0)
            {
                scale = Math.Min(scale, ((double)d.Width / (double)s.Width) / (1 - os));
                scale = Math.Min(scale, ((double)d.Height / (double)s.Height) / (1 - os));
            }

            Rectangle i = new Rectangle(
                0, 0,
                (int)(scale * (double)s.Width),
                (int)(scale * (double)s.Height)
                );

            return Center(i, d);
        }

        /// <summary>
        /// Calculates the 2D transform to required convert rectangle src to rectangle dst
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        public static Matrix CalcTransform(Rectangle src, Rectangle dst)
        {
            Point[] p = {
							new Point(dst.Left, dst.Top), 
							new Point(dst.Right, dst.Top), 
							new Point(dst.Left, dst.Bottom)
						};
            return new Matrix(src, p);
        }

        /// <summary>
        /// Returns the center point of the rectangle.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        static Point Center(Rectangle r)
        {
            return new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
        }

        /// <summary>
        /// returns the area for a size.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static double Area(Size s)
        {
            return (double)s.Width * (double)s.Height;
        }

        public static void Tile(Size a, int minCount, out Size n, out SizeF itemSize)
        {
            float ix = (float) Math.Sqrt(Area(a) / (double)Math.Max(1, minCount));

            n = new Size
            (
                Math.Max(1, (int)((float)a.Width / ix)),
                Math.Max(1, (int)((float)a.Height / ix))
            );

            for (;;)
            {
                itemSize = new SizeF((float)a.Width / (float)n.Width, (float)a.Height / (float)n.Height);
                if (n.Width * n.Height >= minCount)
                {
                    break;
                }
                if (itemSize.Width > itemSize.Height)
                {
                    ++n.Width;
                }
                else
                {
                    ++n.Height;
                }
            }
        }

        /// <summary>
        /// Centers rectangle r in rectangle and returns it.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="at"></param>
        /// <returns></returns>
        static public Rectangle Center(Rectangle r, Rectangle at)
        {
            return new Rectangle(
                at.Left + at.Width / 2 - r.Width / 2,
                at.Top + at.Height / 2 - r.Height / 2,
                r.Width, r.Height);
        }

        static public Point Mul(Point a, int b)
        {
            return new Point(a.X * b, a.Y * b);
        }

        static public RectangleF Transform(RectangleF r, Matrix m)
        {
            PointF[] p = { r.Location, r.Location + r.Size };
            m.TransformPoints(p);
            return new RectangleF(
                p[0].X, p[0].Y,
                p[1].X - p[0].X, p[1].Y - p[0].Y);
        }

        static public RectangleF TransformBoundingBox(RectangleF r, Matrix m)
        {
            PointF[] p = { 
                new PointF(r.Left, r.Top),
                new PointF(r.Right, r.Top),
                new PointF(r.Right, r.Bottom),
                new PointF(r.Left, r.Bottom),
            };

            m.TransformPoints(p);
            return BoundingBox(p);
        }

        static public RectangleF BoundingBox(PointF[] points)
        {
            int iEnd = points.Length;

            if (iEnd == 0)
            {
                return new RectangleF();
            }

            RectangleF b = new RectangleF(points[0], new SizeF(0, 0));

            for (int i=1; i < iEnd; ++i)
            {
                PointF p = points[i];
                if (p.X > b.Right)
                {
                    b.Width = p.X - b.Left;
                }
                if (p.X < b.Left)
                {
                    b.X = p.X;
                }
                if (p.Y > b.Bottom)
                {
                    b.Height = p.Y - b.Top;
                }
                if (p.Y < b.Top)
                {
                    b.Y = p.Y;
                }
            }

            return b;

        }

        static public int Floor(int x, int step)
        {
            return (x / step) * step;
        }

        static public int Ceil(int x, int step)
        {
            int f = Floor(x, step);
            if (f < x)
            {
                f += step;
            }
            return f;
        }

        public static int Clip(int minimum, int x, int maximum)
        {
            if (x < minimum)
            {
                return minimum;
            }
            if (x < maximum)
            {
                return x;
            }

            return maximum - 1;
        }

        public static float Clip(float minimum, float x, float maximum)
        {
            if (x < minimum)
            {
                return minimum;
            }
            if (x < maximum)
            {
                return x;
            }

            return maximum;
        }

        static public bool Contains(Rectangle c, Rectangle r)
        {
            return
                c.Left <= r.Left && r.Right <= c.Right &&
                c.Top <= r.Top && r.Bottom <= c.Bottom;
        }

        public static int CeilingPower2(int x)
        {
            int y = 1;
            for (; y < x; y *= 2)
            {
            }
            return y;
        }

        public static Rectangle BoundsRect(int left, int right, int top, int bottom)
        {
            return new Rectangle(left, top, right - left, bottom - top);
        }
        
        public static int WrapAround(int x, int modulo)
        {
            if (x < 0)
            {
                int r = ((modulo-x) % modulo);
                if (r == 0)
                {
                    r = modulo;
                }
                return modulo - r;
            }
            else
            {
                return x % modulo;
            }
        }
    }
}
