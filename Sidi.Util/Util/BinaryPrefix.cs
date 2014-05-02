// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
        public object GetFormat(Type formatType)
        {
            if (typeof(ICustomFormatter).Equals(formatType)) return this;
            return null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            return arg.ToDouble().BinaryPrefix();
        }

        public static IFormatProvider Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new BinaryPrefix();
                }
                return s_instance;
            }
        }
        static BinaryPrefix s_instance = null;
    }

    public static class BinaryPrefixExtension
    {
        static string[] prefixes = new string[] { String.Empty, "Ki", "Mi", "Gi", "Ti", "Pi", "Ei", "Zi", "Yi" };

        public static string BinaryPrefix(this double x)
        {
            var b = (double)(1 << 10);
            if (x < b)
            {
                return x.ToString("F0");
            }
            else
            {
                var e = Math.Log(x) / Math.Log(b);
                int index = (int)(e);
                var Prefix = prefixes[index];
                var Value = x / Math.Pow(b, index);
                return String.Format("{0:F} {1}", Value, Prefix);
            }
        }
    }
}
