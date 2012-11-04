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
using Sidi.Util;

namespace Sidi.IO
{
    public class CopyProgress
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CopyProgress(
            LPath source,
            LPath destination)
        {
            this.Source = source;
            this.Destination = destination;
            this.Start = DateTime.Now;
            this.Time = this.Start;
        }

        public DateTime Start { get; private set; }
        public DateTime Time { get; private set; }

        public DateTime End
        {
            get
            {
                return DateTime.Now + RemainingTime;
            }
        }

        public TimeSpan Elapsed
        {
            get
            {
                return Time - Start;
            }
        }

        public double Speed
        {
            get
            {
                var speed = SafeDiv(TotalBytesTransferred, Elapsed.TotalSeconds);
                return speed;
            }
        }

        public double PercentComplete
        {
            get
            {
                return SafeDiv((double)TotalBytesTransferred, (double)TotalFileSize) * 100.0;
            }
        }

        public TimeSpan RemainingTime
        {
            get
            {
                var seconds = Elapsed.TotalSeconds * SafeDiv((double)TotalFileSize - (double)TotalBytesTransferred, (double) TotalBytesTransferred);
                if (seconds < 1 || seconds > TimeSpan.MaxValue.TotalSeconds)
                {
                    return TimeSpan.FromHours(1);
                }
                else
                {
                    return TimeSpan.FromSeconds(seconds);
                }
            }
        }

        static double SafeDiv(double n, double d)
        {
            var r = n / d;
            if (Double.IsNaN(r))
            {
                return 0;
            }
            else
            {
                return r;
            }
        }

        public long TotalFileSize { get; private set; }
        public long TotalBytesTransferred { get; private set; }

        public void Update(long totalBytesTransferred, long totalFileSize)
        {
            TotalFileSize = totalFileSize;
            TotalBytesTransferred = totalBytesTransferred;
            Time = DateTime.Now;
        }

        public LPath Source;
        public LPath Destination;

        static BinaryPrefix binaryPrefix = new BinaryPrefix();

        public string Message
        {
            get
            {
                return String.Format("{0:F0}% complete ({5} of {6}, {3:hh\\:mm\\:ss} remaining, {4}): {1} -> {2}",
                    PercentComplete,
                    Source,
                    Destination,
                    RemainingTime,
                    String.Format(binaryPrefix, "{0:B}B/s", (long)Speed),
                    String.Format(binaryPrefix, "{0:B}B", TotalBytesTransferred),
                    String.Format(binaryPrefix, "{0:B}B", TotalFileSize)
                    );
            }
        }
    }

}
