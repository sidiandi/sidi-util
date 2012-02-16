using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;

namespace Sidi.Visualization
{
    public class CushionPainter<T> where T : ITree
    {
        float h = 0.75f;
        float f = 0.75f;
        const float Ia = 40;
        const float Is = 215;
        float[] L = new float[] { 0.09759f, -0.19518f, 0.9759f };

        public CushionPainter(TreeMapControl<T> control)
        {
            this.control = control;
        }

        TreeMapControl<T> control;

        public Bitmap Render()
        {
            var rect = control.TreeLayout.Bounds;
            var bitmap = new Bitmap(
                (int)rect.Width,
                (int)rect.Height,
                PixelFormat.Format24bppRgb);
            var s = new float[2, 2];
            var surface = new float[4];

            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            try
            {
                Render(data, control.TreeLayout.LayoutTree, h, f, s);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        void Render(BitmapData bitmap, Tree<TreeMapLayout.Layout> t, float h, float f, float[,] s)
        {
            s = (float[,])s.Clone();
            var layout = (TreeMapLayout.Layout)t.Data;
            var r = layout.Rectangle;

            if (t.Parent != null)
            {
                for (int d = 0; d < 2; ++d)
                {
                    AddRidge(r[(int)d, (int)Bound.Min], r[(int)d, (int)Bound.Max], h,
                        ref s[(int)d, (int)0], ref s[(int)d, (int)1]);
                }
            }

            if (!t.Children.Any())
            {
                RenderCushion(bitmap, r, s, NodeColor((T)t.Data.TreeNode).ToArray());
            }
            else
            {
                foreach (var i in t.Children)
                {
                    Render(bitmap, i, h * f, f, s);
                }
            }
        }

        unsafe void RenderCushion(
            BitmapData bitmap,
            float[,] r,
            float[,] s,
            float[] color
            )
        {
            var ey = Math.Min((int)Math.Ceiling(r[(int)Dir.Y, (int)Bound.Max]), bitmap.Height);
            var by = (int)Math.Ceiling(r[(int)Dir.Y, (int)Bound.Min]);
            var ex = Math.Min((int)Math.Ceiling(r[(int)Dir.X, (int)Bound.Max]), bitmap.Width);
            var bx = (int)Math.Ceiling(r[(int)Dir.X, (int)Bound.Min]);
            for (int iy = by; iy < ey; ++iy)
            {
                byte* row = (byte*)bitmap.Scan0 + iy * bitmap.Stride + bx * 3;
                for (int ix = bx; ix < ex; ++ix)
                {
                    var nx = (2 * s[(int)Dir.X, 1] * (ix + 0.5) + s[(int)Dir.X, 0]);
                    var ny = -(2 * s[(int)Dir.Y, 1] * (iy + 0.5) + s[(int)Dir.Y, 0]);
                    var cosa = (nx * L[0] + ny * L[1] + L[2]) / Math.Sqrt(nx * nx + ny * ny + 1.0);
                    var intensity = Ia + Math.Max(0, Is * cosa);
                    *row = Util.ClipByte(intensity * color[0]);
                    ++row;
                    *row = Util.ClipByte(intensity * color[1]);
                    ++row;
                    *row = Util.ClipByte(intensity * color[2]);
                    ++row;
                }
            }
        }

        public Func<T, Color> NodeColor
        {
            set
            {
                nodeColor = value;
                InvalidateRenderCache();
                control.Invalidate();
            }

            get
            {
                return nodeColor;
            }
        }

        Func<T, Color> nodeColor = n => Color.White;

        Bitmap renderCache;
        object renderCacheTreeLayout;

        public void InvalidateRenderCache()
        {
            if (renderCache != null)
            {
                renderCache.Dispose();
                renderCache = null;
            }
        }
        
        public void Paint(PaintEventArgs e)
        {
            if (renderCache == null || !renderCache.Size.Equals(control.Size) || renderCacheTreeLayout != control.TreeLayout)
            {
                InvalidateRenderCache();
            }

            if (renderCache == null)
            {
                renderCache = Render();
                renderCacheTreeLayout = this.control.TreeLayout;
            }
            e.Graphics.DrawImage(renderCache, 0, 0);
        }

        void AddRidge(float x1, float x2, float h, ref float s1, ref float s2)
        {
            s1 = s1 + 4 * h * (x2 + x1) / (x2 - x1);
            s2 = s2 - 4 * h / (x2 - x1);
        }
    }
}
