using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Treemapping
{
    public static class Extensions
    {
        public static float GetArea(this RectangleF rect)
        {
            return rect.Width * rect.Height;
        }

        public static float GetAspectRatio(this RectangleF rect)
        {
            float n;
            float d;
            if (rect.Width > rect.Height)
            {
                n = rect.Width;
                d = rect.Height;
            }
            else
            {
                n = rect.Height;
                d = rect.Width;
            }

            if (n == 0)
            {
                return 1.0f;
            }
            else if (d == 0)
            {
                return System.Single.MaxValue;
            }
            else
            {
                return n / d;
            }
        }
    }
}
