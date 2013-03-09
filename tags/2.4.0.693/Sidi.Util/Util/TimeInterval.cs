using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Util
{
    public class TimeInterval
    {
        public TimeInterval(DateTime begin, TimeSpan duration)
        {
            this.begin = begin;
            this.end = begin + duration;
        }

        public TimeInterval(DateTime t0, DateTime t1)
        {
            this.begin = t0;
            this.end = t1;
        }

        public static TimeInterval Parse(string timeIntervalText)
        {
            return TimeIntervalParser.Parse(timeIntervalText);
        }

        public static TimeInterval MaxValue
        {
            get
            {
                return new TimeInterval(DateTime.MinValue, DateTime.MaxValue);
            }
        }

        DateTime begin;
        DateTime end;

        public DateTime Begin
        {
            get
            {
                return begin;
            }
        }

        public DateTime End
        {
            get
            {
                return end;
            }
        }

        public bool Intersects(TimeInterval r)
        {
            return !(r.Begin >= End || r.End <= Begin);
        }

        public bool Includes(TimeInterval r)
        {
            return !(r.Begin >= End || r.End < Begin);
        }

        /// <summary>
        /// Test if r is a subset of this
        /// </summary>
        /// <param name="r"></param>
        /// <returns>True if r is a subset of this, false otherwise</returns>
        public bool Contains(TimeInterval r)
        {
            return Begin <= r.Begin && r.End <= End;
        }

        public TimeInterval Intersect(TimeInterval r)
        {
            return new TimeInterval(
                Begin > r.Begin ? Begin : r.Begin,
                End < r.End ? End : r.End);
        }

        public TimeSpan Duration
        {
            get
            {
                return this.End - this.Begin;
            }
        }

        public IEnumerable<TimeInterval> Days
        {
            get
            {
                for (var d = Begin.Date; d < End; d = d.AddDays(1))
                {
                    yield return new TimeInterval(d, d.AddDays(1));
                }
            }
        }

        public IEnumerable<TimeInterval> Months
        {
            get
            {
                for (var d = new DateTime(Begin.Year, Begin.Month, 1); d < End; d = d.AddMonths(1))
                {
                    yield return new TimeInterval(d, d.AddMonths(1));
                }
            }
        }

        public DateTime Center
        {
            get
            {
                return new DateTime((Begin.Ticks + End.Ticks) / 2);
            }
        }

        public override string ToString()
        {
            return String.Format("[{0}, {1}[", Begin, End);
        }

        public override bool Equals(object obj)
        {
            var r = obj as TimeInterval;
            return r != null && Begin == r.Begin && End == r.End;
        }

        public override int GetHashCode()
        {
            return Begin.GetHashCode() * 251 + End.GetHashCode();
        }

        public static TimeInterval Year(DateTime time)
        {
            return new TimeInterval(new DateTime(time.Year, 1, 1), new DateTime(time.Year + 1, 1, 1));
        }

        public static TimeInterval Month(DateTime time)
        {
            var b = new DateTime(time.Year, time.Month, 1);
            return new TimeInterval(b, b.AddMonths(1));
        }

        public static TimeInterval Today
        {
            get
            {
                return Day(DateTime.Now);
            }
        }

        public static TimeInterval Day(DateTime time)
        {
            return new TimeInterval(time.Date, time.Date.AddDays(1));
        }
    }
}
