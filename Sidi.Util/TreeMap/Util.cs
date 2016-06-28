using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.TreeMap
{
    class Util
    {
        public static byte ClipByte(double x)
        {
            return (byte)Math.Max(0, Math.Min(byte.MaxValue, x));
        }

    }
}
