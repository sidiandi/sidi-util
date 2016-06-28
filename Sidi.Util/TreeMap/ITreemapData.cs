using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.TreeMap
{
    public interface ITreemapData
    {
        double Size { get; }
        string Label { get; }
        System.Drawing.Color Color { get; }
    }
}
