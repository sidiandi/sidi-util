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
