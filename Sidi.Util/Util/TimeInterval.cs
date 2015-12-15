using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sidi.Util
{
    /// <summary>
    /// Time interval with begin time and end time
    /// </summary>
    /// The time interval includes the begin time, but does not include the end time.
    [Serializable]
    public class TimeInterval
    {
        /// <summary>
        /// Constructs a TimeInterval with specified begin and duration
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="duration"></param>
        public TimeInterval(DateTime begin, TimeSpan duration)
            : this(begin, begin + duration)
        {
        }

        /// <summary>
        /// Constructs a TimeInterval with specified duration and end.
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="end"></param>
        public TimeInterval(TimeSpan duration, DateTime end)
            : this(end - duration, end)
        {
        }

        /// <summary>
        /// Constructs a begin closed, end open TimeInterval with specified begin and end
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        public TimeInterval(DateTime begin, DateTime end)
            : this(begin, end, true, false)
        {
        }

        public TimeInterval(DateTime begin, DateTime end, bool beginClosed, bool endClosed)
        {
            if (beginClosed && endClosed)
            {
                if (!(begin <= end))
                {
                    throw new ArgumentOutOfRangeException("end", end, "end must be greater or equal than begin");
                }
            }
            else
            {
                if (!(begin < end))
                {
                    throw new ArgumentOutOfRangeException("end", end, "end must be greater than begin");
                }
            }

            this.begin = begin;
            this.end = end;
            this.beginIncluded = beginClosed;
            this.endIncluded = endClosed;
        }

        public static TimeInterval Parse(string timeIntervalText)
        {
            return new Sidi.CommandLine.Parser().ParseValue<TimeInterval>(timeIntervalText);
        }

        /// <summary>
        /// Time interval with maximum possible extent.
        /// </summary>
        public static TimeInterval MaxValue
        {
            get
            {
                return new TimeInterval(DateTime.MinValue, DateTime.MaxValue);
            }
        }

        DateTime begin;
        bool beginIncluded;

        DateTime end;
        bool endIncluded;

        /// <summary>
        /// Begin of the time interval
        /// </summary>
        public DateTime Begin
        {
            get
            {
                return begin;
            }
        }

        /// <summary>
        /// End of the time interval. The end is not contained in the time interval.
        /// </summary>
        public DateTime End
        {
            get
            {
                return end;
            }
        }
        
        public bool BeginIncluded { get { return beginIncluded; } }

        public bool EndIncluded { get { return endIncluded; } }

        /// <summary>
        /// Returns a value indicating if the intersection of this and a specified TimeInterval is not empty.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if value intersects with this, false otherwise</returns>
        public bool Intersects(TimeInterval value)
        {
            if (value.End < Begin || value.Begin > this.End)
            {
                return false;
            }

            if (value.End == Begin)
            {
                return value.endIncluded && beginIncluded;
            }

            if (value.Begin == End)
            {
                return value.beginIncluded && endIncluded;
            }

            return true;
        }

        /// <summary>
        /// Returns a value indicating if a specified TimeInterval is a subset of this
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if value is a subset of this, false otherwise</returns>
        public bool Contains(TimeInterval value)
        {
            return object.Equals(this, Envelope(this, value));
        }

        public static TimeInterval Envelope(params TimeInterval[] intervals)
        {
            var x = intervals[0];
            DateTime begin = x.Begin;
            DateTime end = x.End;
            bool beginClosed = x.beginIncluded;
            bool endClosed = x.endIncluded;

            for (int i = 1; i < intervals.Length; ++i)
            {
                x = intervals[i];
                if (x.Begin < begin)
                {
                    begin = x.Begin;
                    beginClosed = x.beginIncluded;
                }
                else if (x.Begin == begin)
                {
                    beginClosed = x.beginIncluded | beginClosed;
                }

                if (x.End > end)
                {
                    end = x.End;
                    endClosed = x.endIncluded;
                }
                else if (x.End == end)
                {
                    endClosed = x.endIncluded | endClosed;
                }
            }

            return new TimeInterval(begin, end, beginClosed, endClosed);
        }

        /// <summary>
        /// Test if this time interval contains time
        /// </summary>
        /// Attention: the time interval does include its begin time, but not its end time
        /// <param name="time">Time to be checked</param>
        /// <returns>True if time is in the time interval, false otherwise</returns>
        public bool Contains(DateTime time)
        {
            return Begin <= time && time < End;
        }

        public TimeInterval Intersect(TimeInterval x)
        {
            DateTime begin = Begin;
            DateTime end = End;
            bool beginClosed = this.beginIncluded;
            bool endClosed = this.endIncluded;

            if (x.Begin > begin)
            {
                begin = x.Begin;
                beginClosed = x.beginIncluded;
            }
            else if (x.Begin == begin)
            {
                beginClosed = x.beginIncluded & beginClosed;
            }

            if (x.End < end)
            {
                end = x.End;
                endClosed = x.endIncluded;
            }
            else if (x.End == end)
            {
                endClosed = x.endIncluded & endClosed;
            }

            return new TimeInterval(begin, end, beginClosed, endClosed);
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
            return String.Format("{0}{1}, {2}{3}", 
                beginIncluded ? "[" : "]",
                Begin, 
                End,
                endIncluded ? "]" : "["
                );
        }

        public override bool Equals(object obj)
        {
            var r = obj as TimeInterval;
            return r != null &&
                Begin == r.Begin &&
                End == r.End &&
                beginIncluded == r.beginIncluded &&
                endIncluded == r.endIncluded;
        }

        public override int GetHashCode()
        {
            return Begin.GetHashCode() * 251 + End.GetHashCode();
        }

        public static TimeInterval Year(DateTime time)
        {
            return new TimeInterval(new DateTime(time.Year, 1, 1), new DateTime(time.Year + 1, 1, 1));
        }

        public static TimeInterval Year(DateTime time, int startMonth)
        {
            var d = time.Month < startMonth ? -1 : 0;
            return new TimeInterval(
                new DateTime(time.Year + d, startMonth, 1),
                new DateTime(time.Year + d + 1, startMonth, 1));
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
