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
using System.Drawing.Drawing2D;

namespace Sidi.Visualization
{
    class Tile
    {
        public Bounds Bounds;
        public int Level;
        public int X;
        public int Y;

        public override bool Equals(object obj)
        {
            var r = obj as Tile;
            return r != null && Level == r.Level && X == r.X && Y == r.Y;
        }

        public override int GetHashCode()
        {
            return (Level ^ X ^ Y).GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("Level {0}: ({1},{2})", Level, X, Y);
        }
    }
}
