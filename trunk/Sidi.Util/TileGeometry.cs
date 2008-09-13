// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
