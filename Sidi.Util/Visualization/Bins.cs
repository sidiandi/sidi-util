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

        public double Percentile(IComparable item)
        {
            return (double) Lut.BinarySearch(i => i.CompareTo(item) > 0) / (double)Lut.Length;
        }
    }
}
