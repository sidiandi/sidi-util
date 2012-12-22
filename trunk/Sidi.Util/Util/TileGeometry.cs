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
using System.Text;
using System.Drawing;

namespace Sidi.Util
{
    public class TileGeometry
    {
        Rectangle r;
        Size grid;

        public TileGeometry(int minCount, Rectangle a_r)
        {
            r = a_r;
            Size a = r.Size;

            float ix = (float)Math.Sqrt(GraMath.Area(r.Size) / (double)Math.Max(1, minCount));

            Size n = new Size
            (
                Math.Max(1, (int)((float)a.Width / ix)),
                Math.Max(1, (int)((float)a.Height / ix))
            );

            for (; ; )
            {
                SizeF itemSize = new SizeF((float)a.Width / (float)n.Width, (float)a.Height / (float)n.Height);
                if (GraMath.Area(n) >= minCount)
                {
                    break;
                }
                if (itemSize.Width > itemSize.Height)
                {
                    ++n.Width;
                }
                else
                {
                    ++n.Height;
                }
            }
            grid = n;
        }

        public Rectangle Tile(int index)
        {
            int y = index / grid.Width;
            int x = index - (y * grid.Width);
            Rectangle t = GraMath.BoundsRect(
                r.Left + r.Width * x / grid.Width,
                r.Left + r.Width * (x + 1) / grid.Width,
                r.Top + r.Height * y / grid.Height,
                r.Top + r.Height * (y + 1) / grid.Height);
            return t;
        }

        public static void Tile(Size a, int minCount, out Size n, out SizeF itemSize)
        {
            float ix = (float)Math.Sqrt(GraMath.Area(a) / (double)Math.Max(1, minCount));

            n = new Size
            (
                Math.Max(1, (int)((float)a.Width / ix)),
                Math.Max(1, (int)((float)a.Height / ix))
            );

            for (; ; )
            {
                itemSize = new SizeF((float)a.Width / (float)n.Width, (float)a.Height / (float)n.Height);
                if (n.Width * n.Height >= minCount)
                {
                    break;
                }
                if (itemSize.Width > itemSize.Height)
                {
                    ++n.Width;
                }
                else
                {
                    ++n.Height;
                }
            }
        }
    }
}
