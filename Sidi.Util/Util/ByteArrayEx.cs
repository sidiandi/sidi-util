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
using System.IO;

namespace Sidi.Extensions
{
    public static class IEnumerableByteExtensions
    {
        public static string HexString(this IEnumerable<byte> x)
        {
            using (var w = new StringWriter())
            {
                foreach (var i in x)
                {
                    w.Write(String.Format("{0:x2}", i));
                }
                return w.ToString();
            }
        }

        public static IEnumerable<byte> HexStringToBytes(string hexString)
        {
            for (int i = 0; i < hexString.Length; i += 2)
            {
                var s = hexString.Substring(i, 2);
                yield return Byte.Parse(s, System.Globalization.NumberStyles.HexNumber);
            }
        }
    }
}
