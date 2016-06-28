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
using System.Windows.Media;

namespace Sidi.TreeMap
{
    class Tiles
    {
        public Tiles(Size tileSize)
        {
            Size = tileSize;
        }

        public Size Size { get; private set; }

        public RectangleD GetVisibleWorldRect(RectangleD screenRectangle, Matrix worldToScreenTransform)
        {
            var im = worldToScreenTransform;
            im.Invert();
            return im.Transform(screenRectangle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worldToScreenTransform"></param>
        /// <param name="screenRect"></param>
        /// <returns>All tiles that are required to cover the screenRect area on the screen.</returns>
        public IEnumerable<Tile> Get(Matrix worldToScreenTransform, Rectangle screenRect)
        {
            var screenToWorld = worldToScreenTransform;
            screenToWorld.Invert();

            var worldRect = screenToWorld.Transform(screenRect);

            int level = GetLevel((double)this.Size.Width * screenToWorld.M11);

            var worldTileSize = Math.Pow(2.0, level);
            var screenTileSize = worldTileSize * worldToScreenTransform.M11;

            var x0 = (int)Math.Floor(worldRect.Left / worldTileSize);
            var y0 = (int)Math.Floor(worldRect.Top / worldTileSize);
            for (int y = y0; y * worldTileSize < worldRect.Bottom; ++y)
            {
                for (int x = x0; x * worldTileSize < worldRect.Right; ++x)
                {
                    var tile = new Tile()
                    {
                        Level = level,
                        X = x,
                        Y = y,
                        WorldRect = RectangleD.FromLTRB(
                            x * worldTileSize,
                            y * worldTileSize,
                            (x + 1) * worldTileSize,
                            (y + 1) * worldTileSize),
                    };
                    tile.ScreenRect = worldToScreenTransform.Transform(tile.WorldRect);
                    yield return tile;
                }
            }
        }

        static int GetLevel(double x)
        {
            return (int)Math.Ceiling(Math.Log(x) / Math.Log(2));
        }

        internal Tile GetNextLevel(Tile i, ref Rectangle sourceRect)
        {
            sourceRect = new Rectangle(
                sourceRect.X + (i.X & 1) * sourceRect.Width / 2,
                sourceRect.Y + (i.Y & 1) * sourceRect.Height / 2,
                sourceRect.Width / 2,
                sourceRect.Height / 2
                );

            if (sourceRect.Width < 32)
            {
                return null;
            }

            return new Tile()
            {
                Level = i.Level + 1,
                X = i.X / 2,
                Y = i.Y / 2
            };
        }
    }
}
