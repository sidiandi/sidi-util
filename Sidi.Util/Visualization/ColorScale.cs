using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Sidi.Visualization
{
    class ColorScale
    {
        public static Color ToColor(string x)
        {
            if (x.Length > 1)
            {
                x = x.Substring(1);
            }
            else
            {
                x = String.Empty;
            }

            var ext = SortPos(x);
            var hsl = new HSLColor(Color.Red);
            hsl.Hue = 360.0 * ext;
            return hsl;
        }

        static double SortPos(string x)
        {
            double f = 1.0;
            double y = 0.0;
            foreach (var c in x.ToLower())
            {
                f /= (double)('z' - 'a');
                y += (double)(c - 'a') * f;
            }
            return y;
        }
    }
}
