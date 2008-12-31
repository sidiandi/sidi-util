// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Sidi.Util
{
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
