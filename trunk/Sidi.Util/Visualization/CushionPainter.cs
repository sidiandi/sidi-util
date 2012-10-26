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
    public class CushionPainter
    {
        float h = 0.75f;
        float f = 0.75f;
        const float Ia = 40;
        const float Is = 215;
        float[] L = new float[] { 0.09759f, -0.19518f, 0.9759f };

        public CushionPainter(TreeMapControl control)
        {
            this.control = control;

            var sb = Screen.PrimaryScreen.Bounds;
            var cachedTiles = sb.Width * sb.Height / tiles.Size.Width / tiles.Size.Height * 2;

            tileBitmaps = new Collections.LruCacheBackground<Tile, Bitmap>(cachedTiles, tile =>
                {
                    return Render(tiles.Size, RectangleF.FromLTRB(tile.P0.X, tile.P0.Y, tile.P1.X, tile.P1.Y));
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

        TreeMapControl control;

        public Bitmap Render(Size bitmapSize, RectangleF rect)
        {
            var bitmap = new Bitmap(bitmapSize.Width, bitmapSize.Height, PixelFormat.Format24bppRgb);

            this.transform = new Matrix(rect, new[]{
                new PointF(0,0), 
                new PointF(bitmapSize.Width, 0),
                new PointF(0, bitmapSize.Height)
            });
            this.rect = rect.ToArray();

            var data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            
            try
            {
                if (this.layout != null)
                {
                    var surface = new float[4];
                    var s = new float[2, 2];
                    Render(data, layout.Root, h, f, s);
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        void Render(BitmapData bitmap, TreeMapLayout.Layout layout, float h, float f, float[,] s)
        {
            var r = layout.Rectangle;

            if (!r.Intersects(rect))
            {
                return;
            }

            s = s.Copy();

            if (layout.Parent != null)
            {
                for (int d = 0; d < 2; ++d)
                {
                    AddRidge(r[(int)d, (int)Bound.Min], r[(int)d, (int)Bound.Max], h,
                        ref s[(int)d, (int)0], ref s[(int)d, (int)1]);
                }
            }

            if (!layout.Children.Any())
            {
                var bitmapR = r.Copy();
                transform.Transform(bitmapR);
                RenderCushion(bitmap, bitmapR, r, s, NodeColor(layout.Tree.Object).ToArray());
            }
            else
            {
                foreach (var i in layout.Children)
                {
                    Render(bitmap, i, h * f, f, s);
                }
            }
        }

        Matrix transform;
        float[,] rect;
        
        unsafe void RenderCushion(
            BitmapData bitmap,
            float[,] bitmapR,
            float[,] r,
            float[,] s,
            float[] color
            )
        {
            var ey = Math.Min((int)Math.Ceiling(bitmapR[(int)Dir.Y, (int)Bound.Max]), bitmap.Height);
            var by = Math.Max((int)Math.Ceiling(bitmapR[(int)Dir.Y, (int)Bound.Min]), 0);
            var ex = Math.Min((int)Math.Ceiling(bitmapR[(int)Dir.X, (int)Bound.Max]), bitmap.Width);
            var bx = Math.Max((int)Math.Ceiling(bitmapR[(int)Dir.X, (int)Bound.Min]), 0);
            float yScale = (r[(int)Dir.Y, (int)Bound.Max] - r[(int)Dir.Y, (int)Bound.Min]) / (bitmapR[(int)Dir.Y, (int)Bound.Max] - bitmapR[(int)Dir.Y, (int)Bound.Min]);
            float xScale = (r[(int)Dir.X, (int)Bound.Max] - r[(int)Dir.X, (int)Bound.Min]) / (bitmapR[(int)Dir.X, (int)Bound.Max] - bitmapR[(int)Dir.X, (int)Bound.Min]);
 
            for (int iy = by; iy < ey; ++iy)
            {
                float y = r[(int)Dir.Y, (int)Bound.Min] + ((float)iy + 0.5f - bitmapR[(int)Dir.Y, (int)Bound.Min]) * yScale;  
                byte* row = (byte*)bitmap.Scan0 + iy * bitmap.Stride + bx * 3;
                for (int ix = bx; ix < ex; ++ix)
                {
                    float x = r[(int)Dir.X, (int)Bound.Min] + ((float)ix + 0.5f - bitmapR[(int)Dir.X, (int)Bound.Min]) * xScale;
                    var nx = (2 * s[(int)Dir.X, 1] * (x) + s[(int)Dir.X, 0]);
                    var ny = -(2 * s[(int)Dir.Y, 1] * (y) + s[(int)Dir.Y, 0]);
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

        public Func<object, Color> NodeColor
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

        TreeMapLayout layout;

        void Invalidate()
        {
            tileBitmaps.Clear();
            layout = control.Layout;
            control.Invalidate();
        }

        public void Paint(PaintEventArgs e)
        {
            if (control.Layout != layout)
            {
                Invalidate();
            }
            
            var transform = e.Graphics.Transform.Clone();
            e.Graphics.Transform = new Matrix();
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            
            foreach (var i in tiles.Get(transform, control.ClientRectangle))
            {
                var pts = new[] { i.P0, i.P1 };
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
                                break;
                            }
                        }
                    }
                }
            }

            e.Graphics.Transform = transform;
        }

        Sidi.Collections.LruCacheBackground<Tile, Bitmap> tileBitmaps;
        Tiles tiles = new Tiles(new Size(0x100, 0x100));
        
        void AddRidge(float x1, float x2, float h, ref float s1, ref float s2)
        {
            s1 = s1 + 4 * h * (x2 + x1) / (x2 - x1);
            s2 = s2 - 4 * h / (x2 - x1);
        }
    }
}
