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
using System.Text;

namespace Sidi.Util
{
    [CLSCompliant(false)]
    public sealed class UnixTime
    {
        /// <summary>
        /// Private constructor to prevent the class from being instantiated.
        /// </summary>
        private UnixTime() { }

        public static DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0);

        // Time_t is an int representing the number of seconds since
        // Midnight UTC 1 Jan 1970 on the Gregorian Calendar.
        // [CLSCompliant(false)]
        public static DateTime DateTimeFromUnixTime(uint timeT)
        {
            return origin + new TimeSpan(timeT * TimeSpan.TicksPerSecond);
        }

        // [CLSCompliant(false)]
        public static uint UnixTimeFromDateTime(DateTime time)
        {
            long diff = time.Ticks - origin.Ticks;
            long seconds = diff / TimeSpan.TicksPerSecond;
            return (uint)seconds;
        }
    }
}
