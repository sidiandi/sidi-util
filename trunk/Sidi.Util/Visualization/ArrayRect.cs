using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sidi.Visualization
{
    static class ArrayRectExtensions
    {
        const double cf = 1.0 / 256.0;

        public static double[] ToArray(this Color color)
        {
            return new double[]
            {
                color.B * cf,
                color.G * cf,
                color.R * cf
            };
        }

        public static bool CoversPixel(this double[,] rectangle)
        {
            return ((int)rectangle[0, 0] != (int)rectangle[0, 1])
                && ((int)rectangle[1, 0] != (int)rectangle[1, 1]);
        }
    }
}
