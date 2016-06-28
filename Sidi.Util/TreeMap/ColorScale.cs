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

namespace Sidi.TreeMap
{
    /// <summary>
    /// Interpolates a scalar to a color
    /// </summary>
    public class ColorScale
    {
        public static Color[] GetBlueRed()
        {
            int lutLength = 256;
            return GetColorScale(lutLength, Color.Blue, Color.Red);
        }

        public static Color[] GetGreenYellowRed()
        {
            int lutLength = 256;
            return
                GetColorScale(lutLength / 2, Color.Green, Color.Yellow).Concat(
                GetColorScale(lutLength / 2, Color.Yellow, Color.Red)).ToArray();
        }

        public static Color[] GetColorScale(int length, Color c0, Color c1)
        {
            return Enumerable
                .Range(0, length)
                .Select(i => Color.FromArgb(255,
                    Util.ClipByte(c0.R + (double) i / (double) length * (c1.R - c0.R)),
                    Util.ClipByte(c0.G + (double) i / (double) length * (c1.G - c0.G)),
                    Util.ClipByte(c0.B + (double) i / (double) length * (c1.B - c0.B))))
                .ToArray();
        }

        public ColorScale(double x0, double x1, Color[] colors)
        {
            Lut = colors;
            m = (double)Lut.Length / (x1 - x0);
            this.x0 = x0;
        }

        Color[] Lut;

        double m;
        double x0;

        public Color ToColor(double x)
        {
            var i = (x - x0) * m;
            if (i < 0)
            {
                return Lut[0];
            }
            else if (i >= Lut.Length)
            {
                return Lut[Lut.Length - 1];
            }
            else
            {
                return Lut[(int)i];
            }
        }
        
        double Interpolate(double x0, double x1, double y0, double y1, double x)
        {
            return (x - x0) / (x1 - x0) * (y1 - y0) + y0;
        }

        public static Func<X, Color> Distinct<X, Y>(IEnumerable<X> data, Func<X, Y> distinctBy)
        {
            var dc = new Sidi.Visualization.DistinctColor();
            return t => dc.ToColor(distinctBy(t));
        }

        public static Func<X, Color> Percentile<X, Y>(IEnumerable<X> data, Func<X, Y> selector) where Y : IComparable
        {
            var cs = new Sidi.Visualization.PercentileColorScale(data.Select(selector).Cast<IComparable>(), GetBlueRed());
            return x => cs.GetColor(selector(x));
        }
    }
}
