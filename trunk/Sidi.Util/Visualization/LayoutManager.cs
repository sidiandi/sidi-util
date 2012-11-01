using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

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
            DoLayout = Squares;
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
                    Rectangle = layout.Bounds.Inflate(margin)
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
            public Bounds Rectangle;
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

            double m = c.Rectangle.Extent(d) / c.Layout.First().Tree.Parent.Size;
            double x = c.Rectangle[d, Bound.Min];
            foreach (var tc in c.Layout)
            {
                var r = tc.Bounds;
                r[d, Bound.Min] = x;
                x += tc.Tree.Size * m;
                r[d, Bound.Max] =
                r[od, Bound.Min] = c.Rectangle[od, Bound.Min];
                r[od, Bound.Max] = c.Rectangle[od, Bound.Max];
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
            Squarify(c.Layout, c.Rectangle, c.Direction, 0, c.Layout.Length);
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

    }
}
