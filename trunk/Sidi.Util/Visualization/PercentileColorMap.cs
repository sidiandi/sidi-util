using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Sidi.Visualization
{
    public class PercentileColorMap
    {
        Bins bins;
        ColorScale colorMap;

        public PercentileColorMap(IEnumerable<IComparable> list)
        {
            bins = new Bins(list);
            colorMap = ColorScale.BlueRed(0.0, 1.0);
        }

        public Color GetColor(IComparable item)
        {
            var p = bins.Percentile(item);
            return colorMap.ToColor(p);
        }
    }
}
