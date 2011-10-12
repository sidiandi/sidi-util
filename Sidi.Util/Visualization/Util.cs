using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Visualization
{
    static class Util
    {
        public static byte ClipByte(double x)
        {
            return (byte)Math.Max(0, Math.Min(byte.MaxValue, x));
        }

        public static int BinarySearch<T>(this T[] c, Func<T, bool> condition)
        {
            var i0 = 0;
            var i1 = c.Length;
            int i = (i0 + i1) / 2;
            for (; i0 < i; i = (i0 + i1) / 2)
            {
                if (condition(c[i]))
                {
                    i1 = i;
                }
                else
                {
                    i0 = i;
                }
            }
            return i1;
        }
    }
}
