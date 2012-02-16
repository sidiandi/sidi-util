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

        public DistinctColor(IEnumerable<object> values)
        {
            var distinct = values.Distinct().ToList();
            distinct.Sort();

            var hsl = new HSLColor(Color.Red);
            int n= 0;
            double m = 360.0 / (double) distinct.Count;
            valueToColor = new Dictionary<object, Color>(distinct.Count);
            foreach (var i in distinct)
            {
                hsl.Hue = (double)n * m;
                valueToColor[i] = hsl;
                ++n;
            }
        }

        public Color ToColor(object value)
        {
            return valueToColor[value];
        }
    }
}
