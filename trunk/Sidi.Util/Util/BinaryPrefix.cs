using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sidi.Util
{
    public class BinaryPrefix : IFormatProvider, ICustomFormatter
    {
        static string[] prefixes = new string[] { String.Empty, "Ki", "Mi", "Gi", "Ti", "Pi", "Ei", "Zi", "Yi" };

        string Prefix;
        double Value;

        public override string ToString()
        {
            return String.Format("{0:F} {1}", Value, Prefix);
        }

        public object GetFormat(Type formatType)
        {
            if (typeof(ICustomFormatter).Equals(formatType)) return this;
            return Thread.CurrentThread.CurrentCulture.GetFormat(formatType);
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if ("B".Equals(format))
            {
                double x = 1;
                if (arg is System.Int32)
                {
                    x = (double)(int)arg;
                }
                else if (arg is long)
                {
                    x = (double)(long)arg;
                }
                var b = (double)(1 << 10);
                if (x < b)
                {
                    return arg.ToString();
                }
                else
                {
                    var e = Math.Log(x) / Math.Log(b);
                    int index = (int)(e);
                    Prefix = prefixes[index];
                    Value = x / Math.Pow(b, index);
                    return String.Format("{0:F} {1}", Value, Prefix);
                }
            }
            else
            {
                return arg.ToString();
            }
        }
    }
}
