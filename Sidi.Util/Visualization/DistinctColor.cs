using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Sidi.Visualization
{
    public class DistinctColor
    {
        Dictionary<object, Color> valueToColor;

        public DistinctColor()
        {
            valueToColor = new Dictionary<object, Color>();
        }

        Color GetColor(int n)
        {
            var hsl = new HSLColor(Color.Red);
            hsl.Hue += Fill(n);
            return hsl;
        }

        double Fill(int n)
        {
            double x = 0.0;
            double increment = 1.0;

            for (; n > 0; n = n >> 1)
            {
                increment /= 2;
                if ((n & 1) != 0)
                {
                    x += increment;
                }
            }
            return x;
        }

        public Color ToColor(object value)
        {
            Color color;
            if (!valueToColor.TryGetValue(value, out color))
            {
                var n = valueToColor.Count + 1;
                color = GetColor(n);
                valueToColor[value] = color;
            }
            return color;
        }
    }
}
