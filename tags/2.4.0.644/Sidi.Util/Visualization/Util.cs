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
