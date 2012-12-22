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
using System.Drawing;
using System.Drawing.Imaging;
using Sidi.Extensions;

namespace Sidi.Visualization
{
    /// <summary>
    /// http://www.win.tue.nl/~vanwijk/ctm.pdf
    /// </summary>
    public class LayoutManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public LayoutManager(Tree root, RectangleF bounds)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

            layout = new Layout(null) { Tree = root, Bounds = bounds.ToBounds() };
            // DoLayout = Squares;
            DoLayout = DivideAndConquer;
            Update(layout);
        }

        Layout layout;

        public Layout Root
        {
            get
            {
                return layout;
            }
        }

        Bounds margin = new Bounds();

        /// <summary>
        /// Recursively updates a layout tree.
        /// </summary>
        /// <param name="layout">Layout tree node to be updated. Rectangle and TreeNode must be initialized.</param>
        /// <returns></returns>
        void Update(Layout layout)
        {
            if (layout.Tree != null && layout.Tree.Children.Any() && layout.Tree.Size > 0)
            {
                var lc = new LayoutContext()
                {
                    Layout = layout.Tree.Children.Select(x => new Layout(layout) { Tree = x }).ToArray(),
                    Bounds = layout.Bounds.Inflate(margin)
                };
                
                DoLayout(lc);

                foreach (var i in layout.Children)
                {
                    Update(i);
                }
            }
        }

        public Layout GetLayoutAt(System.Windows.Point p, int levels)
        {
            return GetLayoutAt(layout, p, levels);
        }

        Layout GetLayoutAt(Layout t, System.Windows.Point p, int levels)
        {
            if (t != null && t.Bounds.Contains(p))
            {
                if (levels > 0)
                {
                    foreach (var i in t.Children.Cast<Layout>())
                    {
                        var il = GetLayoutAt(i, p, levels - 1);
                        if (il != null)
                        {
                            return il;
                        }
                    }
                }
                return t;
            }
            else
            {
                return null;
            }
        }

        public class LayoutContext
        {
            public Bounds Bounds;
            public Dimension Direction;
            public Layout[] Layout;
        }

        public Action<LayoutContext> DoLayout;

        static void Stripes(LayoutContext c)
        {
            var d = c.Direction;
            Dimension od;
            if (d == Dimension.X)
            {
                od = Dimension.Y;
            }
            else
            {
                od = Dimension.X;
            }

            double m = c.Bounds.Extent(d) / c.Layout.First().Tree.Parent.Size;
            double x = c.Bounds[d, Bound.Min];
            foreach (var tc in c.Layout)
            {
                var r = tc.Bounds;
                r[d, Bound.Min] = x;
                x += tc.Tree.Size * m;
                r[d, Bound.Max] =
                r[od, Bound.Min] = c.Bounds[od, Bound.Min];
                r[od, Bound.Max] = c.Bounds[od, Bound.Max];
            }
        }

        static double SizeSum(Layout[] c, int b, int e)
        {
            var s = 0.0;
            for (int i = b; i < e; ++i)
            {
                s += c[i].Tree.Size;
            }
            return s;
        }

        static double GetWorstAspectRatio(Layout[] layout, int b, int e, double sizeToPix, double h)
        {
            var war = 1.0;

            if (h == 0.0f)
            {
                throw new ArgumentException("Cannot be 0", "h");
            }

            for (int i = b; i < e; ++i)
            {
                var w = layout[i].Tree.Size * sizeToPix / h;
                var ar = (w == 0.0f) ? 1.0f : (w > h ? (w / h) : (h / w));
                if (ar > war)
                {
                    war = ar;
                }
            }
            return war;
        }

        public static void Squares(LayoutContext c)
        {
            Squarify(c.Layout, c.Bounds, c.Direction, 0, c.Layout.Length);
        }

        static void Squarify(Layout[] layout, Bounds r, Dimension d, int b, int e)
        {
            if (layout.Length == 0)
            {
                return;
            }

            // fill one row until aspect ratio gets worse again
            if (r.Width > r.Height)
            {
                d = Dimension.Y;
            }
            else
            {
                d = Dimension.X;
            }

            double aspectRatio = double.MaxValue;
            var extent = r.Extent(d);
            if (extent < double.Epsilon)
            {
                for (int i = b; i < e; ++i)
                {
                    layout[i].Bounds = r;
                }
                return;
            }
            var pixPerSize = r.Area / SizeSum(layout, b, e);
            var pixPerHeight = pixPerSize / extent;
            double rowSize = 0.0;
            double newRowSize;
            int rowEnd;
            double h = 0.0f;
            double newH;
            double newAspectRatio;
            for (rowEnd = b + 1; rowEnd <= e; ++rowEnd)
            {
                newRowSize = rowSize + layout[rowEnd - 1].Tree.Size;
                if (newRowSize == 0.0f)
                {
                    continue;
                }

                newH = newRowSize * pixPerHeight;
                newAspectRatio = GetWorstAspectRatio(layout, b, rowEnd, pixPerSize, newH);
                if (newAspectRatio > aspectRatio)
                {
                    break;
                }
                else
                {
                    aspectRatio = newAspectRatio;
                    h = newH;
                    rowSize = newRowSize;
                }
            }
            --rowEnd;

            // new row consists of array elements [b, rowEnd[
            var od = Flip(d);
            Bounds rowRect = r;
            rowRect[od, Bound.Max] = rowRect[od, Bound.Min] + h;
            var x = rowRect[d, Bound.Min];
            var extentPerSize = pixPerSize / h;
            if (double.IsNaN(extentPerSize))
            {
                extentPerSize = 0.0f;
            }
            for (int i = b; i < rowEnd; ++i)
            {
                Bounds ib = new Bounds();
                ib[d, Bound.Min] = x;
                var w = layout[i].Tree.Size * extentPerSize;
                x = Math.Min(x + w, r[d, Bound.Max]);
                if (double.IsNaN(x))
                {
                    throw new InvalidOperationException();
                }
                ib[d, Bound.Max] = x;
                ib[od, Bound.Min] = rowRect[od, Bound.Min];
                ib[od, Bound.Max] = rowRect[od, Bound.Max];
                layout[i].Bounds = ib;
            }

            // check results
            for (int i = b; i < rowEnd; ++i)
            {
                var re = layout[i].Bounds;
                foreach (Dimension id in Enum.GetValues(typeof(Dimension)))
                {
                    foreach (Bound ib in Enum.GetValues(typeof(Bound)))
                    {
                        if (double.IsNaN(re[id, ib]))
                        {
                            throw new Exception(i.ToString());
                        }
                    }
                }
            }

            // calculate remaining rectangle
            r[od, Bound.Min] = rowRect[od, Bound.Max];

            // squarify the rest
            if (rowEnd < e)
            {
                Squarify(layout, r, od, rowEnd, e);
            }

            /*
            float ca = 0.0f;
            for (int i = b; i < e; ++i)
            {
                ca += layout[i].Rectangle.Area();
            }
            var pa = r.Area();
            if (Math.Abs(pa - ca) > 0.1)
            {
                throw new Exception();
            }
             */
        }

        static Dimension Flip(Dimension d)
        {
            switch (d)
            {
                case Dimension.X:
                    return Dimension.Y;
                case Dimension.Y:
                    return Dimension.X;
            }
            throw new ArgumentException(d.ToString());
        }

        public static void DivideAndConquer(LayoutContext c)
        {
            if (c.Layout.Length >= 2)
            {
                foreach (var i in Split2(c))
                {
                    DivideAndConquer(i);
                }
            }
            else
            {
                c.Layout[0].Bounds = c.Bounds;
            }
        }

        static LayoutContext[] Split2(LayoutContext c)
        {
            var splitDim = c.Bounds.Width > c.Bounds.Height ? Dimension.X : Dimension.Y;
            double totalSize = 0;
            var accumulatedSize = c.Layout.Select(x => { totalSize += x.Tree.Size; return totalSize; }).ToArray();
            var error = accumulatedSize.Select(x => Math.Abs(x - totalSize / 2)).ToArray();

            var minError = Double.MaxValue;
            int splitIndex = 0;
            for (; splitIndex < error.Length; ++splitIndex)
            {
                if (error[splitIndex] < minError)
                {
                    minError = error[splitIndex];
                }
                else
                {
                    break;
                }
            }

            // split at splitindex
            var size0 = accumulatedSize[splitIndex - 1];
            var splitX = c.Bounds[splitDim, Bound.Min] + c.Bounds.Extent(splitDim) * size0 / totalSize;

            var r = new LayoutContext[]
            {
                new LayoutContext()
                {
                    Bounds = Split(c.Bounds, splitDim, splitX, 1),
                    Layout = SubArray(c.Layout, 0, splitIndex)
                },
                new LayoutContext()
                {
                    Bounds = Split(c.Bounds, splitDim, splitX, 0),
                    Layout = SubArray(c.Layout, splitIndex, c.Layout.Length)
                }
            };

            //log.InfoFormat("{0} {1}", size0, r[0].Layout.Sum(x => x.Tree.Size));
            //log.Info(r.Select(x => x.Bounds.Area / x.Layout.Sum(j => j.Tree.Size) ).Join(", "));
            //log.Info(r.Select(x => x.Bounds).Join(", "));

            return r;
        }

        static Bounds Split(Bounds b, Dimension dim, double x, int part)
        {
            var r = new Bounds(b.P0.X, b.P0.Y, b.P1.X, b.P1.Y);
            r[dim, part == 0 ? Bound.Min : Bound.Max] = x;
            return r;
        }

        static Layout[] SubArray(Layout[] a, int i0, int i1)
        {
            var r = new Layout[i1 - i0];
            Array.Copy(a, i0, r, 0, i1 - i0);
            return r;
        }
    }
}
