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
using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace Sidi.Treemapping
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

    public struct RectangleD
    {
        public Point P0;
        public Point P1;

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "({0}),({1})", P0, P1);
        }

        public bool CoversPixel
        {
            get
            {
                return ((int)P0.X != (int)P1.X) && ((int)P0.Y != (int)P1.Y);
            }
        }

        public int PixelsCovered
        {
            get
            {
                var r = this.ToRectangle();
                return r.Width * r.Height;
            }
        }

        public RectangleD Inflate(RectangleD b)
        {
            return new RectangleD()
            {
                P0 = P0.Add(b.P0),
                P1 = P1.Add(b.P1)
            };
        }

        public bool Intersects(RectangleD r)
        {
            return !(
                    P1.X <= r.P0.X ||
                    P1.Y <= r.P0.Y ||
                    r.P1.X <= P0.X ||
                    r.P1.Y <= P0.Y
                );
        }

        public bool Includes(RectangleD r)
        {
            return
                    P0.X <= r.P0.X &&
                    P0.Y <= r.P0.Y &&
                    P1.X >= r.P1.X &&
                    P1.Y >= r.P1.Y;
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

        public double Left { get { return P0.X; } }
        public double Top { get { return P0.Y; } }
        public double Right { get { return P1.X; } }
        public double Bottom { get { return P1.Y; } }

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

        public Matrix CreateTransform(Point[] point)
        {
            using (var mf = new System.Drawing.Drawing2D.Matrix(
                this.ToRectangleF(), 
                point.Select(x => x.ToPointF()).ToArray()))
            {
                return mf.ToMatrixD();
            }
        }

        public static implicit operator RectangleD(System.Drawing.RectangleF x)
        {
            return RectangleD.FromLTRB(x.Left, x.Top, x.Right, x.Bottom);
        }

        public static implicit operator RectangleD(System.Drawing.Rectangle x)
        {
            return RectangleD.FromLTRB(x.Left, x.Top, x.Right, x.Bottom);
        }

        public static RectangleD FromLTRB(double l, double t, double r, double b)
        {
            return new RectangleD { P0 = new Point(l, t), P1 = new Point(r, b) };
        }

        public Point Center
        {
            get
            {
                return new Point((Right + Left) * 0.5, (Top + Bottom) * 0.5);
            }
        }
    }
}
