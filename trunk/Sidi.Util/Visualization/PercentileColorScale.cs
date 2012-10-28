using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Sidi.Visualization
{
    public class PercentileColorScale
    {
        Bins bins;
        ColorScale colorScale;

        public PercentileColorScale(IEnumerable<IComparable> list, Color[] colorScale)
        {
            bins = new Bins(list);
            this.colorScale = new ColorScale(0.0, 1.0, colorScale);
        }

        public Color GetColor(IComparable item)
        {
            var p = bins.Percentile(item);
            return colorScale.ToColor(p);
        }
    }
}
