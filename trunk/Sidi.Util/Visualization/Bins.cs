using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Visualization
{
    public class Bins
    {
        public Bins(IEnumerable<IComparable> values)
        {
            Lut = values
                .OrderBy(x => x)
                .ToArray();
        }

        IComparable[] Lut;

        public double Percentile(IComparable x)
        {
            return (double) Lut.BinarySearch(i => i.CompareTo(x) > 0) / (double)Lut.Length;
        }
    }
}
