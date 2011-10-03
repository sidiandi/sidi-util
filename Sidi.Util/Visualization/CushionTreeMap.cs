using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Sidi.Visualization
{
    /// <summary>
    /// http://www.win.tue.nl/~vanwijk/ctm.pdf
    /// </summary>
    public class CushionTreeMap<T>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        float h = 0.5f;
        float f = 0.75f;
        const float Ia = 40;
        const float Is = 215;
        float[] L = new float[] { 0.09759f, -0.19518f, 0.9759f };

        public CushionTreeMap(ITree<T> root)
        {
            this.root = root;
            DoLayout = 
                // Stripes
                Squares
                ;
        }

        ITree<T> root;
        ITree<Layout> layoutTree;

        public ITree<Layout> LayoutTree
        {
            get
            {
                return layoutTree;
            }
        }

        public void UpdateLayoutTree(float[,] rect)
        {
            layoutTree = UpdateLayoutTreeRecursive(null, root, rect);
        }

        ITree<Layout> UpdateLayoutTreeRecursive(ITree<Layout> parent, ITree<T> tree, float[,] rect)
        {
            var layoutTree = new Tree<Layout>();
            var layout = new Layout(tree);
            layoutTree.Data = layout;
            layoutTree.Parent = parent;
            layout.Rectangle = (float[,]) rect.Clone();
            layoutTree.Size = tree.Size;

            if (tree.Children != null && tree.Children.Count > 0 && tree.Size > 0 && layout.Rectangle.CoversPixel())
            {
                var lc = new LayoutContext();
                lc.Layout = tree.Children.Select(x => new Layout(x)).ToArray();
                lc.Rectangle = (float[,])rect.Clone();
                DoLayout(lc);
                layoutTree.Children = lc.Layout
                    .Select(x => UpdateLayoutTreeRecursive(layoutTree, x.TreeNode, x.Rectangle))
                    .ToList();
            }

            return layoutTree;
        }

        public ITree<Layout> GetLayoutAt(float[] p)
        {
            return GetLayoutAt(layoutTree, p);
        }

        ITree<Layout> GetLayoutAt(ITree<Layout> t, float[] p)
        {
            if (t != null && t.Data.Rectangle.Contains(p))
            {
                foreach (var i in t.Children)
                {
                    var il = GetLayoutAt(i, p);
                    if (il != null)
                    {
                        return il;
                    }
                }
                return t;
            }
            else
            {
                return null;
            }
        }

        public void Render(
            Bitmap bitmap,
            System.Drawing.RectangleF r)
        {
            var ar = r.ToArray();
            UpdateLayoutTree(ar);
            var s = new float[2, 2];
            var surface = new float[4];
            Render(bitmap, layoutTree, h, f, s);
        }

        void Render(Bitmap bitmap, ITree<Layout> t, float h, float f, float[,] s)
        {
            s = (float[,])s.Clone();
            var layout = (Layout)t.Data;
            var r = layout.Rectangle;
            
            if (t.Parent != null)
            {
                for (int d = 0; d < 2; ++d)
                {
                    AddRidge(r[(int)d, (int)Bound.Min], r[(int)d, (int)Bound.Max], h,
                        ref s[(int)d, (int)0], ref s[(int)d, (int)1]);
                }
            }

            if (t.Children.Count == 0)
            {
                RenderCushion(bitmap, r, s);
            }
            else
            {
                foreach (var i in t.Children)
                {
                    Render(bitmap, i, h * f, f, s);
                }
            }
        }

        public class LayoutContext
        {
            public float[,] Rectangle;
            public Dir Direction;
            public Layout[] Layout;
        }

        public class Layout
        {
            public Layout(ITree<T> tree)
            {
                TreeNode = tree;
                Rectangle = new float[2, 2];
            }

            public float[,] Rectangle;
            public ITree<T> TreeNode;
        }

        public Action<LayoutContext> DoLayout;

        static float Width(float[,] r, Dir d)
        {
            return r[(int)d, (int)Bound.Max] - r[(int)d, (int)Bound.Min];
        }

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

            float m = Width(c.Rectangle, d) / c.Layout.First().TreeNode.Parent.Size;
            float x = c.Rectangle[(int)d, (int)Bound.Min];
            foreach (var tc in c.Layout)
            {
                var r = tc.Rectangle;
                r[(int)d, (int)Bound.Min] = x;
                x += tc.TreeNode.Size * m;
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
                s += c[i].TreeNode.Size;
            }
            return s;
        }

        static float Area(float[,] r)
        {
            var w = r[0, 1] - r[0, 0];
            var h = r[1, 1] - r[1, 0];
            return w * h;
        }

        static float GetWorstAspectRatio(Layout[] layout, int b, int e, float sizeToPix, float h)
        {
            var war = 1.0f;

            if (h == 0.0f)
            {
                throw new ArgumentException("h");
            }

            for (int i= b; i< e; ++i)
            {
                var w = layout[i].TreeNode.Size * sizeToPix / h;
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
            if (Width(r, Dir.X) > Width(r, Dir.Y))
            {
                d = Dir.Y;
            }
            else
            {
                d = Dir.X;
            }

            float aspectRatio = float.MaxValue;
            var width = Width(r, d);
            if (width < float.Epsilon)
            {
                for (int i = b; i < e; ++i)
                {
                    layout[i].Rectangle = (float[,]) r.Clone();
                }
                return;
            }
            var pixPerSize = Area(r) / SizeSum(layout, b, e);
            var pixPerHeight = pixPerSize / width;
            var rowSize = 0.0f;
            float newRowSize;
            int rowEnd;
            float h = 0.0f;
            float newH;
            float newAspectRatio;
            for (rowEnd = b + 1; rowEnd <= e; ++rowEnd)
            {
                newRowSize = rowSize + layout[rowEnd - 1].TreeNode.Size;
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
                var w = layout[i].TreeNode.Size * widthPerSize;
                x += w;
                if (float.IsNaN(x))
                {
                    throw new Exception();
                }
                layout[i].Rectangle[(int)d, (int)Bound.Max] = x;
                layout[i].Rectangle[(int)od, (int)Bound.Min] = rowRect[(int)od, (int)Bound.Min];
                layout[i].Rectangle[(int)od, (int)Bound.Max] = rowRect[(int)od, (int)Bound.Max];
            }

            // avoid rounding errors
            layout[rowEnd - 1].Rectangle[(int)d, (int)Bound.Max] = r[(int)d, (int)Bound.Max];

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

            float ca = 0.0f;
            for (int i = b; i < e; ++i)
            {
                ca += Area(layout[i].Rectangle);
            }
            var pa = Area(r);
            if (Math.Abs(pa - ca) > 0.1)
            {
                // throw new Exception();
            }
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

        void RenderCushion(
            Bitmap bitmap,
            float[,] r,
            float[,] s
            )
        {
            var ey = (int)Math.Ceiling(r[(int)Dir.Y, (int)Bound.Max]);
            var by = (int)Math.Ceiling(r[(int)Dir.Y, (int)Bound.Min]);
            var ex = (int)Math.Ceiling(r[(int)Dir.X, (int)Bound.Max]);
            var bx = (int)Math.Ceiling(r[(int)Dir.X, (int)Bound.Min]);
            for (int iy = by; iy < ey; ++iy)
            {
                for (int ix = bx; ix < ex; ++ix )
                {
                    var nx = (2 * s[(int)Dir.X, 1] * (ix + 0.5) + s[(int)Dir.X, 0]);
                    var ny = -(2 * s[(int)Dir.Y, 1] * (iy + 0.5) + s[(int)Dir.Y, 0]);
                    var cosa = (nx * L[0] + ny * L[1] + L[2]) / Math.Sqrt(nx * nx + ny * ny + 1.0);
                    var intensity = Math.Max(0, Math.Min(255, (int)(Ia + Math.Max(0, Is * cosa))));
                    var c = Color.FromArgb(intensity, intensity, intensity);
                    bitmap.SetPixel(ix, iy, c);
                }
            }
        }

        void AddRidge(float x1, float x2, float h, ref float s1, ref float s2)
        {
            s1 = s1 + 4*h*(x2+x1)/(x2-x1);
            s2 = s2 - 4*h/(x2-x1);
        }
    }
}
