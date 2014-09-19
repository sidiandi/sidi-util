using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Util
{
    public class MetricPrefix : IFormatProvider, ICustomFormatter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public object GetFormat(Type formatType)
        {
            if (formatType.Equals(typeof(ICustomFormatter)))
            {
                return this;
            }
            else
            {
                return null;
            }
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (object.Equals(format, "M"))
            {
                return arg.ToDouble().MetricPrefix();
            }

            var formattable = arg as IFormattable;
            if (formattable != null)
            {
                return formattable.ToString(format, formatProvider);
            }
            else
            {
                return arg.ToString();
            }
        }

        public static IFormatProvider Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new MetricPrefix();
                }
                return s_instance;
            }
        }
        static MetricPrefix s_instance = null;
    }

    public static class MetricPrefixExtension
    {
        public class Prefix
        {
            public Prefix(string name, string symbol, int exponent, string englishWord)
            {
                this.Name = name;
                this.Symbol = symbol;
                this.Exponent = exponent;
                this.EnglishWord = englishWord;
            }

            public string Name;
            public string Symbol;
            public double Exponent;
            public string EnglishWord;

            public double GetBase(double x)
            {
                return x * Math.Pow(10, -Exponent);
            }
        };

        static Prefix[] prefixes = new Prefix[]
        {
            new Prefix("yotta", "Y", 24, "septillion"),
            new Prefix("zetta", "Z", 21, "sextillion"),
            new Prefix("exa", "E", 18, "quintillion"),
            new Prefix("peta", "P", 15, "quadrillion"),
            new Prefix("tera", "T", 12, "trillion"),
            new Prefix("giga", "G", 9, "billion"),
            new Prefix("mega", "M", 6, "million"),
            new Prefix("kilo", "k", 3, "thousand"),
            new Prefix(String.Empty, String.Empty, 0, String.Empty),
            new Prefix("milli", "m", -3, "thousandth"),
            new Prefix("micro", "µ", -6, "millionth"),
            new Prefix("nano", "n", -9, "billionth"),
            new Prefix("pico", "p", -12, "trillionth"),
            new Prefix("femto", "f", -15, "quadrillionth"),
            new Prefix("atto", "a", -18, "quintillionth"),
            new Prefix("zepto", "z", -21, "sextillionth"),
            new Prefix("yocto", "y", -24, "septillionth"),
        };

        static Prefix GetBestPrefix(double x)
        {
            var exp = Math.Log10(x);
            if (exp >= prefixes.First().Exponent + 3.0)
            {
                return null;
            }
            foreach (var i in prefixes)
            {
                if (exp >= i.Exponent)
                {
                    return i;
                }
            }
            return null;
        }

        public static string MetricPrefix(this double x)
        {
            var abs = Math.Abs(x);
            var prefix = GetBestPrefix(abs);
            if (prefix == null)
            {
                return x.ToString("G3");
            }
            return String.Format("{2}{0}{1}", prefix.GetBase(abs).ToFixedDigits(3), prefix.Symbol, Math.Sign(x) < 0 ? "-" : String.Empty);
        }

        public static string ToFixedDigits(this double x, int digits)
        {
            return x.ToString("F" + ((digits - 1 - (int)Math.Log10(x))).ToString());
        }

        /// <summary>
        /// converts an object to a double. Supported types: double, int, uint, long, ulong
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double ToDouble(this object x)
        {
            if (x == null)
            {
                return Double.NaN;
            }

            if (x is double)
            {
                return (double)x;
            }
            else if (x is Int16)
            {
                return (double)(Int16)x;
            }
            else if (x is UInt16)
            {
                return (double)(UInt16)x;
            }
            else if (x is Int32)
            {
                return (double)(Int32)x;
            }
            else if (x is UInt32)
            {
                return (double)(UInt32)x;
            }
            else if (x is Int64)
            {
                return (double)(Int64)x;
            }
            else if (x is UInt64)
            {
                return (double)(UInt64)x;
            }
            else if (x is string)
            {
                return Double.Parse((string)x);
            }
            return (double)x;
        }
    }
}
