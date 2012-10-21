using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sidi.Visualization
{
    class Tiles
    {
        public Tiles(Size tileSize)
        {
            Size = tileSize;
        }

        public Size Size { get; private set; }

        public RectangleF GetVisibleWorldRect(Rectangle screenRectangle, Matrix worldToScreenTransform)
        {
            var im = worldToScreenTransform.Clone();
            im.Invert();
            var pts = new PointF[]
            {
                new PointF(screenRectangle.Left, screenRectangle.Top),
                new PointF(screenRectangle.Left, screenRectangle.Bottom),
                new PointF(screenRectangle.Right, screenRectangle.Top),
                new PointF(screenRectangle.Right, screenRectangle.Bottom)
            };
            im.TransformPoints(pts);

            return ContainingRect(pts);
        }

        public static RectangleF ContainingRect(PointF[] pts)
        {
            var r = pts.Max(x => x.X);
            var l = pts.Min(x => x.X); ;
            var b = pts.Max(x => x.Y);
            var t = pts.Min(x => x.Y);

            return RectangleF.FromLTRB(l, t, r, b);
        }

        public IEnumerable<Tile> Get(Matrix worldToScreenTransform, Rectangle screenRect)
        {
            var bounds = GetVisibleWorldRect(screenRect, worldToScreenTransform);

            var tileCount = new Size(screenRect.Width / Size.Width, screenRect.Height / Size.Height);

            int level = GetLevel(Math.Min(bounds.Width / tileCount.Width, bounds.Height / tileCount.Height));

            var worldTileSize = (float)Math.Pow(2.0, level);
            var x0 = (int)Math.Floor(bounds.X / worldTileSize);
            var y0 = (int)Math.Floor(bounds.Y / worldTileSize);
            for (int y = y0; y * worldTileSize < bounds.Bottom; ++y)
            {
                for (int x = x0; x * worldTileSize < bounds.Right; ++x)
                {
                    yield return new Tile()
                    {
                        Level = level,
                        X = x,
                        Y = y,
                        P0 = new PointF(x * worldTileSize, y * worldTileSize),
                        P1 = new PointF((x + 1) * worldTileSize, (y + 1) * worldTileSize),
                    };
                }
            }
        }

        static int GetLevel(double x)
        {
            return (int)Math.Ceiling(Math.Log(x) / Math.Log(2));
        }
    }
}
