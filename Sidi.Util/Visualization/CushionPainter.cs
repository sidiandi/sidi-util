using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Threading;
using Sidi.Extensions;

namespace Sidi.Visualization
{
    public class CushionPainter : IDisposable
    {
        double h = 0.75f;
        double f = 0.75f;
        const double Ia = 40;
        const double Is = 215;
        double[] L = new double[] { 0.09759f, -0.19518f, 0.9759f };

        public CushionPainter(TreeMap control)
        {
            this.control = control;

            var sb = Screen.PrimaryScreen.Bounds;
            var cachedTiles = sb.Width * sb.Height / tiles.Size.Width / tiles.Size.Height * 2;

            tileBitmaps = new Collections.LruCacheBackground<Tile, Bitmap>(cachedTiles, tile =>
                {
                    return Render(
                        tiles.Size, 
                        tile.Bounds);
                });

            tileBitmaps.EntryUpdated += (s, e) =>
                {
                    control.BeginInvoke(new Action(() => control.Invalidate()));
                };

            {
                var emptyTile = new Bitmap(tiles.Size.Width, tiles.Size.Height);
                using (var g = Graphics.FromImage(emptyTile))
                {
                    g.FillRectangle(new SolidBrush(Color.Black), 0, 0, emptyTile.Width, emptyTile.Height);
                }
                tileBitmaps.DefaultValueWhileLoading = emptyTile;
            }

        }

        TreeMap control;

        public Bitmap Render(Size bitmapSize, Bounds rect)
        {
            var bitmap = new Bitmap(bitmapSize.Width, bitmapSize.Height, PixelFormat.Format24bppRgb);

            //todo: rework
            this.transform = new System.Drawing.Drawing2D.Matrix(rect.ToRectangleF(), new[]{
                new PointF(0,0), 
                new PointF(bitmapSize.Width, 0),
                new PointF(0, bitmapSize.Height)
            }).ToMatrixD();

            this.rect = rect;

            var data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            
            try
            {
                if (this.layout != null)
                {
                    var surface = new double[4];
                    var s = new Bounds();
                    Render(data, layout.Root, h, f, s);
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        Layout FirstLeaf(Layout layout)
        {
            if (layout.Children.Any())
            {
                return FirstLeaf(layout.Children.First());
            }
            else
            {
                return layout;
            }
        }

        void Render(BitmapData bitmap, Layout layout, double h, double f, Bounds s)
        {
            var r = layout.Bounds;

            if (!r.Intersects(rect))
            {
                return;
            }

            if (layout.Parent != null)
            {
                for (Dimension d = Dimension.X; d <= Dimension.Y; ++d)
                {
                    double s0 = s[d, Bound.Min];
                    double s1 = s[d, Bound.Max];
                    AddRidge(
                        r[d, Bound.Min],
                        r[d, Bound.Max], h,
                        ref s0,
                        ref s1);

                    s[d, Bound.Min] = s0;
                    s[d, Bound.Max] = s1;
                }
            }

            var bitmapR = transform.Transform(r);
            if (!layout.Children.Any() || bitmapR.PixelsCovered <= 9)
            {
                RenderCushion(bitmap, bitmapR, r, s, GetColor(FirstLeaf(layout).Tree.Object).ToArray());
            }
            else
            {
                foreach (var i in layout.Children)
                {
                    Render(bitmap, i, h * f, f, s);
                }
            }

            /*
            if (!layout.Children.Any())
            {
                var bitmapR = transform.Transform(r);
                RenderCushion(bitmap, bitmapR, r, s, GetColor(layout.Tree.Object).ToArray());
            }
            else
            {
                foreach (var i in layout.Children)
                {
                    Render(bitmap, i, h * f, f, s);
                }
            }
             */
        }

        System.Windows.Media.Matrix transform;
        Bounds rect;
        
        unsafe void RenderCushion(
            BitmapData bitmap,
            Bounds bitmapR,
            Bounds r,
            Bounds s,
            double[] color
            )
        {
            var ey = Math.Min((int)Math.Ceiling(bitmapR[Dimension.Y, Bound.Max]), bitmap.Height);
            var by = Math.Max((int)Math.Ceiling(bitmapR[Dimension.Y, Bound.Min]), 0);
            var ex = Math.Min((int)Math.Ceiling(bitmapR[Dimension.X, Bound.Max]), bitmap.Width);
            var bx = Math.Max((int)Math.Ceiling(bitmapR[Dimension.X, Bound.Min]), 0);
            double yScale = (r[Dimension.Y, Bound.Max] - r[Dimension.Y, Bound.Min]) / (bitmapR[Dimension.Y, Bound.Max] - bitmapR[Dimension.Y, Bound.Min]);
            double xScale = (r[Dimension.X, Bound.Max] - r[Dimension.X, Bound.Min]) / (bitmapR[Dimension.X, Bound.Max] - bitmapR[Dimension.X, Bound.Min]);
 
            for (int iy = by; iy < ey; ++iy)
            {
                double y = r[Dimension.Y, Bound.Min] + ((double)iy + 0.5f - bitmapR[Dimension.Y, Bound.Min]) * yScale;  
                byte* row = (byte*)bitmap.Scan0 + iy * bitmap.Stride + bx * 3;
                for (int ix = bx; ix < ex; ++ix)
                {
                    double x = r[Dimension.X, Bound.Min] + ((double)ix + 0.5f - bitmapR[Dimension.X, Bound.Min]) * xScale;
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

        public Func<object, Color> GetColor
        {
            set
            {
                nodeColor = value;
                Invalidate();
            }

            get
            {
                return nodeColor;
            }
        }

        Func<object, Color> nodeColor = n => Color.White;

        LayoutManager layout;

        void Invalidate()
        {
            tileBitmaps.Clear();
            layout = control.LayoutManager;
            control.Invalidate();
        }

        public void Paint(PaintEventArgs e)
        {
            if (control.LayoutManager != layout)
            {
                Invalidate();
            }
            
            var transform = e.Graphics.Transform.Clone();
            e.Graphics.Transform = new Matrix();
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            
            foreach (var i in tiles.Get(transform, control.ClientRectangle))
            {
                var pts = new[] { i.Bounds.P0.ToPointF(), i.Bounds.P1.ToPointF() };
                transform.TransformPoints(pts);
                var intPts = pts.Select(x => new Point((int)x.X, (int)x.Y)).ToArray();
                // var destRect = Rectangle.FromLTRB(intPts[0].X, intPts[0].Y, intPts[1].X, intPts[1].Y);
                var destRect = RectangleF.FromLTRB(pts[0].X, pts[0].Y, pts[1].X, pts[1].Y);

                if (tileBitmaps.Contains(i))
                {
                    var t = tileBitmaps[i];
                    e.Graphics.DrawImage(t, destRect);
                }
                else
                {
                    tileBitmaps.Load(i);
                    var sourceRect = new Rectangle(0, 0, tiles.Size.Width, tiles.Size.Height);
                    for (var coarseTile = tiles.GetNextLevel(i, ref sourceRect); coarseTile != null; coarseTile = tiles.GetNextLevel(coarseTile, ref sourceRect))
                    {
                        if (tileBitmaps.Contains(coarseTile))
                        {
                            var t = tileBitmaps[coarseTile];
                            if (t != null)
                            {
                                e.Graphics.DrawImage(t, destRect, sourceRect, GraphicsUnit.Pixel);
                                goto end;
                            }
                        }
                    }
                    e.Graphics.DrawImage(tileBitmaps.DefaultValueWhileLoading, destRect);
                end: ;
                }
            }

            e.Graphics.Transform = transform;
        }

        Sidi.Collections.LruCacheBackground<Tile, Bitmap> tileBitmaps;
        Tiles tiles = new Tiles(new Size(0x100, 0x100));
        
        void AddRidge(double x1, double x2, double h, ref double s1, ref double s2)
        {
            s1 = s1 + 4 * h * (x2 + x1) / (x2 - x1);
            s2 = s2 - 4 * h / (x2 - x1);
        }

        private bool disposed = false;
            
        //Implement IDisposable.
        public void Dispose()
        {
          Dispose(true);
          GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
          if (!disposed)
          {
            if (disposing)
            {
               tileBitmaps.Dispose();
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
            disposed = true;
          }
        }

        // Use C# destructor syntax for finalization code.
        ~CushionPainter()
        {
          // Simply call Dispose(false).
          Dispose(false);
        }    
    
    }
}
