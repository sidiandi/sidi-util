using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Sidi.Visualization
{
    /// <summary>
    /// Interpolates a scalar to a color
    /// </summary>
    public class ColorMap
    {
        public static ColorMap BlueRed(double x0, double x1)
        {
            int lutLength = 256;
            return new ColorMap(x0, x1, GetColorScale(lutLength, Color.Blue, Color.Red));
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

        public ColorMap(double x0, double x1, Color[] colors)
        {
            Lut = colors;
            m = (double)Lut.Length / (x1 - x0);
            this.x0 = x0;
        }

        Color[] Lut;

        public class ControlPoint
        {
            public ControlPoint(double value, Color color)
            {
                Value = value;
                Color = color;
            }
            public double Value;
            public Color Color;
        }

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
    }
}
