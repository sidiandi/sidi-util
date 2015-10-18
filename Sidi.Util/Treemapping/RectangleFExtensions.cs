using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Treemapping
{
    public static class RectangleFExtensions
    {
        public static bool Intersects(this Rectangle a, RectangleF b)
        {
            return !(
                    a.Right < b.Left ||
                    a.Bottom < b.Top ||
                    b.Right < a.Left ||
                    b.Bottom < a.Top
                );
        }
    }
}
