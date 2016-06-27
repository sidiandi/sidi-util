using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Treemapping
{
    public interface ITreemapData
    {
        double Size { get; }
        string Label { get; }
        System.Drawing.Color Color { get; }
    }
}
