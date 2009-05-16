// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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

namespace Sidi.Forms
{
    public static class PointExtensions
    {
        public static Size Sub(this Point p1, Point p2)
        {
            return new Size(p1.X - p2.X, p1.Y - p2.Y);
        }

        static public bool Contains(this Rectangle c, Rectangle r)
        {
            return
                c.Left <= r.Left && r.Right <= c.Right &&
                c.Top <= r.Top && r.Bottom <= c.Bottom;
        }

        static public Point Clip(this Point p, Rectangle r)
        {
            return new Point(
                Math.Max(Math.Min(p.X, r.Right), r.Left),
                Math.Max(Math.Min(p.Y, r.Bottom), r.Top)
                );
        }
    }
}
