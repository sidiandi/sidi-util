using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Sidi.TreeMap
{
    internal class CushionPainter
    {
        double h = 0.75f;
        double f = 0.75f;
        const double Ia = 40;
        const double Is = 215;
        double[] L = new double[] { 0.09759f, -0.19518f, 0.9759f };
        public int MinimalCushionSizeInPixels = 9;

        public CushionPainter()
        {
        }

        public System.Drawing.Bitmap Render(
            ITree<Layout> tree,
            RectangleD renderArea,
            System.Drawing.Size bitmapSize)
        {
            var bitmap = new Bitmap(bitmapSize.Width, bitmapSize.Height, PixelFormat.Format24bppRgb);

            // transform from tree to bitmap coordinate system
            var transform = renderArea.CreateTransform(new[]
            {
                new System.Windows.Point(0,0),
                new System.Windows.Point(bitmapSize.Width, 0),
                new System.Windows.Point(0, bitmapSize.Height)
            });

            var pixels = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            try
            {
                var s = new RectangleD();
                Render(tree, renderArea, transform, h, f, s, pixels);
            }
            finally
            {
                bitmap.UnlockBits(pixels);
            }

            return bitmap;
        }

        public void Render(
            ITree<Layout> tree,
            RectangleD renderArea,
            Matrix transform,
            double h,
            double f,
            RectangleD s,
            BitmapData pixels)
        {
            if (!renderArea.Intersects(tree.Data.Rectangle))
            {
                return;
            }

            for (Dimension d = Dimension.X; d <= Dimension.Y; ++d)
            {
                var s0 = s[d, Bound.Min];
                var s1 = s[d, Bound.Max];
                AddRidge(
                    tree.Data.Rectangle[d, Bound.Min],
                    tree.Data.Rectangle[d, Bound.Max], h,
                    ref s0,
                    ref s1);

                s[d, Bound.Min] = s0;
                s[d, Bound.Max] = s1;
            }

            var treeBoundsBitmap = transform.Transform(tree.Data.Rectangle);
            if (tree.IsLeaf() || treeBoundsBitmap.PixelsCovered <= MinimalCushionSizeInPixels)
            {
                RenderCushion(pixels, tree.Data.Rectangle, treeBoundsBitmap, s, tree.GetFirstLeaf().Data.Color.ToArray());
            }
            else
            {
                foreach (var i in tree.Children)
                {
                    Render(
                        i,
                        renderArea,
                        transform,
                        h * f,
                        f,
                        s,
                        pixels);
                }
            }
        }

        void AddRidge(double x1, double x2, double h, ref double s1, ref double s2)
        {
            s1 = s1 + 4 * h * (x2 + x1) / (x2 - x1);
            s2 = s2 - 4 * h / (x2 - x1);
        }

        unsafe void RenderCushion(
            BitmapData bitmap,
            RectangleD cushionRectangle,
            RectangleD cushionRectangleBitmap,
            RectangleD s,
            double[] color
            )
        {
            var ey = Math.Min((int)Math.Ceiling(cushionRectangleBitmap[Dimension.Y, Bound.Max]), bitmap.Height);
            var by = Math.Max((int)Math.Ceiling(cushionRectangleBitmap[Dimension.Y, Bound.Min]), 0);
            var ex = Math.Min((int)Math.Ceiling(cushionRectangleBitmap[Dimension.X, Bound.Max]), bitmap.Width);
            var bx = Math.Max((int)Math.Ceiling(cushionRectangleBitmap[Dimension.X, Bound.Min]), 0);
            double yScale = (cushionRectangle[Dimension.Y, Bound.Max] - cushionRectangle[Dimension.Y, Bound.Min]) / (cushionRectangleBitmap[Dimension.Y, Bound.Max] - cushionRectangleBitmap[Dimension.Y, Bound.Min]);
            double xScale = (cushionRectangle[Dimension.X, Bound.Max] - cushionRectangle[Dimension.X, Bound.Min]) / (cushionRectangleBitmap[Dimension.X, Bound.Max] - cushionRectangleBitmap[Dimension.X, Bound.Min]);

            for (int iy = by; iy < ey; ++iy)
            {
                double y = cushionRectangle[Dimension.Y, Bound.Min] + ((double)iy + 0.5f - cushionRectangleBitmap[Dimension.Y, Bound.Min]) * yScale;
                byte* row = (byte*)bitmap.Scan0 + iy * bitmap.Stride + bx * 3;
                for (int ix = bx; ix < ex; ++ix)
                {
                    double x = cushionRectangle[Dimension.X, Bound.Min] + ((double)ix + 0.5f - cushionRectangleBitmap[Dimension.X, Bound.Min]) * xScale;
                    var nx = (2 * s[Dimension.X, Bound.Max] * (x) + s[Dimension.X, Bound.Min]);
                    var ny = -(2 * s[Dimension.Y, Bound.Max] * (y) + s[Dimension.Y, Bound.Min]);
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
    }
}
