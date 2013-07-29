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
using Sidi.Extensions;

namespace Sidi.Visualization
{
    public static class Util
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        public class SplitPart<T>
        {
            public T[] Items;
            public double Sum;
        }
        
        /// <summary>
        /// Returns an position index to split scalars
        /// so that the two lists have approximately same sums
        /// </summary>
        /// <param name="scalars"></param>
        /// <returns></returns>
        public static SplitPart<T>[] Split<T>(this IEnumerable<T> list, Func<T, double> scalar)
        {
            double totalSize = 0.0;
            var items = list.ToList();
            // array of accumulated scalars
            var accumulated = items.Select(x => { totalSize += scalar(x); return totalSize; }).ToArray();
            double halfSize = totalSize * 0.5;
            
            // search to find minimum error
            int i = 1;
            var e0 = halfSize;
            for (; i < (accumulated.Length - 1); ++i)
            {
                var e = Math.Abs(accumulated[i] - halfSize);
                if (e < e0)
                {
                    e0 = e;
                }
                else
                {
                    break;
                }
            }

            return new []
            {
                new SplitPart<T>()
                {
                    Items = items.Take(i).ToArray(),
                    Sum = accumulated[i-1],
                },
                new SplitPart<T>()
                {
                    Items = items.Skip(i).ToArray(),
                    Sum = accumulated[accumulated.Length-1] - accumulated[i-1]
                }
            };
        }
    }
}
