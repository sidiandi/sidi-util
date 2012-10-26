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
    public class TreeMapLayout
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TreeMapLayout(Tree root, RectangleF bounds)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

            layout = new Layout(null) { Tree = root, Rectangle = bounds.ToArray() };
            DoLayout = Squares;
            Update(layout);
        }

        public class Layout : Tree
        {
            public Layout(Layout parent)
                : base(parent)
            {
                Rectangle = new float[2, 2];
            }

            public IEnumerable<Layout> Children { get { return base.Children.Cast<Layout>(); } }

            public float[,] Rectangle;
            public Tree Tree;
        }

        Layout layout;

        public Layout Root
        {
            get
            {
                return layout;
            }
        }

        float[,] margin = new float[,] { { 0, 0 }, { 0, 0 } };

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
                    Rectangle = layout.Rectangle.Copy()
                };
                lc.Rectangle.Add(margin);
                
                DoLayout(lc);

                foreach (var i in layout.Children)
                {
                    Update(i);
                }
            }
        }

        public Layout GetLayoutAt(float[] p, int levels)
        {
            return GetLayoutAt(layout, p, levels);
        }

        Layout GetLayoutAt(Layout t, float[] p, int levels)
        {
            if (t != null && t.Rectangle.Contains(p))
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
            public float[,] Rectangle;
            public Dir Direction;
            public Layout[] Layout;
        }

        public Action<LayoutContext> DoLayout;

        static void Stripes(LayoutContext c)
        {
            var d = c.Direction;
            Dir od;
            if (d == Dir.X)
            {
                od = Dir.Y;
            }
            else
            {
                od = Dir.X;
            }

            float m = c.Rectangle.Width(d) / c.Layout.First().Tree.Parent.Size;
            float x = c.Rectangle[(int)d, (int)Bound.Min];
            foreach (var tc in c.Layout)
            {
                var r = tc.Rectangle;
                r[(int)d, (int)Bound.Min] = x;
                x += tc.Tree.Size * m;
                r[(int)d, (int)Bound.Max] =
                r[(int)od, (int)Bound.Min] = c.Rectangle[(int)od, (int)Bound.Min];
                r[(int)od, (int)Bound.Max] = c.Rectangle[(int)od, (int)Bound.Max];
            }
        }

        static float SizeSum(Layout[] c, int b, int e)
        {
            var s = 0.0f;
            for (int i = b; i < e; ++i)
            {
                s += c[i].Tree.Size;
            }
            return s;
        }

        static float GetWorstAspectRatio(Layout[] layout, int b, int e, float sizeToPix, float h)
        {
            var war = 1.0f;

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

        static void Squarify(Layout[] layout, float[,] r, Dir d, int b, int e)
        {
            if (layout.Length == 0)
            {
                return;
            }

            // fill one row until aspect ratio gets worse again
            if (r.Width(Dir.X) > r.Width(Dir.Y))
            {
                d = Dir.Y;
            }
            else
            {
                d = Dir.X;
            }

            float aspectRatio = float.MaxValue;
            var width = r.Width(d);
            if (width < float.Epsilon)
            {
                for (int i = b; i < e; ++i)
                {
                    layout[i].Rectangle = (float[,])r.Clone();
                }
                return;
            }
            var pixPerSize = r.Area() / SizeSum(layout, b, e);
            var pixPerHeight = pixPerSize / width;
            var rowSize = 0.0f;
            float newRowSize;
            int rowEnd;
            float h = 0.0f;
            float newH;
            float newAspectRatio;
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
            var rowRect = (float[,])r.Clone();
            rowRect[(int)od, (int)Bound.Max] = rowRect[(int)od, (int)Bound.Min] + h;
            var x = rowRect[(int)d, (int)Bound.Min];
            var widthPerSize = pixPerSize / h;
            if (float.IsNaN(widthPerSize))
            {
                widthPerSize = 0.0f;
            }
            for (int i = b; i < rowEnd; ++i)
            {
                layout[i].Rectangle[(int)d, (int)Bound.Min] = x;
                var w = layout[i].Tree.Size * widthPerSize;
                x = Math.Min(x + w, r[(int)d, (int)Bound.Max]);
                if (float.IsNaN(x))
                {
                    throw new Exception();
                }
                layout[i].Rectangle[(int)d, (int)Bound.Max] = x;
                layout[i].Rectangle[(int)od, (int)Bound.Min] = rowRect[(int)od, (int)Bound.Min];
                layout[i].Rectangle[(int)od, (int)Bound.Max] = rowRect[(int)od, (int)Bound.Max];
            }

            // check results
            for (int i = b; i < rowEnd; ++i)
            {
                var re = layout[i].Rectangle;
                foreach (Dir id in Enum.GetValues(typeof(Dir)))
                {
                    foreach (Bound ib in Enum.GetValues(typeof(Bound)))
                    {
                        if (float.IsNaN(re[(int)id, (int)ib]))
                        {
                            throw new Exception(i.ToString());
                        }
                    }
                }
            }

            // calculate remaining rectangle
            r[(int)od, (int)Bound.Min] = rowRect[(int)od, (int)Bound.Max];

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

        static Dir Flip(Dir d)
        {
            switch (d)
            {
                case Dir.X:
                    return Dir.Y;
                case Dir.Y:
                    return Dir.X;
            }
            throw new ArgumentException(d.ToString());
        }

    }
}
