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
