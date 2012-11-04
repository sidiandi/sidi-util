// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

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
