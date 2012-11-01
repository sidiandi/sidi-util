using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Sidi.Visualization
{
    public enum Dimension
    {
        X,
        Y
    };

    public enum Bound
    {
        Min,
        Max
    };

    public struct Bounds
    {
        public Point P0;
        public Point P1;

        public Bounds(double l, double t, double r, double b)
        {
            P0 = new Point(l, t);
            P1 = new Point(r, b);
        }

        public Bounds Inflate(Bounds b)
        {
            return new Bounds()
            {
                P0 = P0.Add(b.P0),
                P1 = P1.Add(b.P1)
            };
        }

        public bool Intersects(Bounds r)
        {
            return !(
                    P1.X < r.P0.X ||
                    P1.Y < r.P0.Y ||
                    r.P1.X < P0.X ||
                    r.P1.Y < P0.Y
                );
        }

        public double Area
        {
            get
            {
                return Width * Height;
            }
        }

        public double Width
        {
            get
            {
                return P1.X - P0.X;
            }
        }

        public double Extent(Dimension direction)
        {
            return direction == Dimension.X ? Width : Height;
        }

        public double Height
        {
            get
            {
                return P1.Y - P0.Y;
            }
        }

        public double this[Dimension dir, Bound bound]
        {
            get
            {
                switch (bound)
                {
                    case Bound.Min:
                        return dir == Dimension.X ? P0.X : P0.Y;
                    case Bound.Max:
                        return dir == Dimension.X ? P1.X : P1.Y;
                    default:
                        throw new NotSupportedException();
                }
            }

            set
            {
                switch (bound)
                {
                    case Bound.Min:
                        if (dir == Dimension.X)
                        {
                            P0.X = value;
                        }
                        else
                        {
                            P0.Y = value;
                        }
                        break;
                    case Bound.Max:
                        if (dir == Dimension.X)
                        {
                            P1.X = value;
                        }
                        else
                        {
                            P1.Y = value;
                        }
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public bool Contains(Point p)
        {
            return P0.X <= p.X && p.X < P1.X &&
                P0.Y <= p.Y && p.Y < P1.Y;
        }
    }

    public static class BoundsExtensions
    {
        public static Bounds ToBounds(this System.Drawing.RectangleF rect)
        {
            return new Bounds(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public static Point Add(this Point p0, Point p1)
        {
            return new Point(p0.X + p1.X, p0.Y + p1.Y);
        }

        public static System.Drawing.Rectangle ToRectangle(this Bounds bounds)
        {
            return System.Drawing.Rectangle.FromLTRB(
                (int) bounds.P0.X, 
                (int) bounds.P0.Y, 
                (int) bounds.P1.X, 
                (int) bounds.P1.Y);
        }

        public static System.Drawing.RectangleF ToRectangleF(this Bounds bounds)
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

        public static Bounds Transform(this Matrix m, Bounds x)
        {
            return new Bounds()
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
    }
}
