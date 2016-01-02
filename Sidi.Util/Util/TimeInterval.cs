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
    /// This class can represent open and closed time intervals with its properties BeginIncluded and EndIncluded.
    [Serializable]
    public class TimeInterval
    {
        /// <summary>
        /// Constructs a TimeInterval with specified begin and end
        /// </summary>
        /// If duration is zero, an interval with begin and end included will be constructed.
        /// Otherwise an interval with begin included and end not excluded will be constructed.
        /// 
        /// \snippet Sidi.Util.Test\Util\TimeIntervalTests.cs TimeIntervalTestCtor
        /// <param name="begin"></param>
        /// <param name="end"></param>
        public TimeInterval(DateTime begin, DateTime end)
            : this(begin, end, true, begin.Equals(end))
        {
        }

        /// <summary>
        /// Constructs a TimeInterval with specified begin and end and specified specified inclusion of begin and end.
        /// </summary>
        /// <param name="begin">Begin time</param>
        /// <param name="end">End time</param>
        /// <param name="beginClosed">Indicates if begin is included.</param>
        /// <param name="endClosed">Indicates if end is included.</param>
        /// <exception cref="System.ArgumentException">Thrown when end is not greater than begin.</exception>
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

        /// <summary>
        /// Constructs a TimeInterval with specified begin and duration
        /// </summary>
        /// If duration is zero, an interval with begin and end included will be constructed.
        /// Otherwise an interval with begin included and end not excluded will be constructed.
        /// <param name="begin"></param>
        /// <param name="duration"></param>
        public TimeInterval(DateTime begin, TimeSpan duration)
            : this(begin, begin + duration)
        {
        }

        /// <summary>
        /// Constructs a TimeInterval with specified duration and end.
        /// </summary>
        /// If duration is zero, an interval with begin and end included will be constructed.
        /// Otherwise an interval with begin included and end not excluded will be constructed.
        /// <param name="duration"></param>
        /// <param name="end"></param>
        public TimeInterval(TimeSpan duration, DateTime end)
            : this(end - duration, end)
        {
        }

        /// <summary>
        /// Parses a text as a TimeInterval
        /// </summary>
        /// Can parse the string returned by ToString.
        /// 
        /// <example></example>
        /// <param name="timeIntervalText">Text to parse, e.g. </param>
        /// <returns></returns>
        public static TimeInterval Parse(string timeIntervalText)
        {
            return Sidi.CommandLine.TimeIntervalParser.Parse(timeIntervalText);
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
        /// End of the time interval.
        /// </summary>
        public DateTime End
        {
            get
            {
                return end;
            }
        }

        /// <summary>
        /// Indicates if Begin is included in the time interval.
        /// </summary>
        public bool BeginIncluded { get { return beginIncluded; } }

        /// <summary>
        /// Indicates if End is included in the time interval
        /// </summary>
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
            return object.Equals(this, GetEnvelope(new[] { this, value }));
        }

        /// <summary>
        /// Return the maximum extent of the specified intervals
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Throws when the enumerable contains no elements</exception>
        /// <param name="intervals"></param>
        /// <returns>The minimum time interval which contains all the specified time intervals</returns>
        public static TimeInterval GetEnvelope(IEnumerable<TimeInterval> intervals)
        {
            var e = intervals.GetEnumerator();
            if (!e.MoveNext())
            {
                throw new ArgumentOutOfRangeException("intervals", intervals, "At least one element required in the enumerable");
            }

            var x = e.Current;
            DateTime begin = x.Begin;
            DateTime end = x.End;
            bool beginClosed = x.beginIncluded;
            bool endClosed = x.endIncluded;

            for (; e.MoveNext();)
            {
                x = e.Current;
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
            return
                (BeginIncluded ? (Begin <= time) : (Begin < time)) &&
                (EndIncluded ? (time <= End) : (time < End));
        }

        /// <summary>
        /// Returns the intersection of this with the given time interval.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException" >Throws when intervals do not intersect</exception>
        /// <param name="x">Time interval to intersect</param>
        /// <returns>Intersection time interval</returns>
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

        /// <summary>
        /// The duration of the time interval
        /// </summary>
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

        /// <summary>
        /// Center of the time interval
        /// </summary>
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
            var hc = 0;
            var m = 23;
            hc += m * Begin.GetHashCode();
            hc += m * End.GetHashCode();
            hc += m * BeginIncluded.GetHashCode();
            hc += m * EndIncluded.GetHashCode();
            return hc;
        }

        /// <summary>
        /// Returns the year of the given time as time interval
        /// </summary>
        /// <param name="time">Time for which to return the year</param>
        /// <returns>The year time interval</returns>
        public static TimeInterval Year(DateTime time)
        {
            return new TimeInterval(new DateTime(time.Year, 1, 1), new DateTime(time.Year + 1, 1, 1));
        }

        /// <summary>
        /// Returns a time interval with a length of 1 year, starting with the specified start month
        /// </summary>
        /// Can be used to obtain tax or financial years with a begin other than January 1st.
        /// <param name="time">Time for which to return the year</param>
        /// <param name="startMonth">Start month of the year interval (1=January)</param>
        /// <returns>The year time interval</returns>
        public static TimeInterval Year(DateTime time, int startMonth)
        {
            var d = time.Month < startMonth ? -1 : 0;
            return new TimeInterval(
                new DateTime(time.Year + d, startMonth, 1),
                new DateTime(time.Year + d + 1, startMonth, 1));
        }

        /// <summary>
        /// Returns the month of the given time as time interval
        /// </summary>
        /// <param name="time">Time for which to return the month</param>
        /// <returns>The month time interval</returns>
        public static TimeInterval Month(DateTime time)
        {
            var b = new DateTime(time.Year, time.Month, 1);
            return new TimeInterval(b, b.AddMonths(1));
        }

        /// <summary>
        /// Returns today as time interval
        /// </summary>
        public static TimeInterval Today
        {
            get
            {
                return Day(DateTime.Now);
            }
        }

        /// <summary>
        /// Returns the day of the given time as time interval
        /// </summary>
        /// <param name="time">Time for which to return the day</param>
        /// <returns>Day time interval</returns>
        public static TimeInterval Day(DateTime time)
        {
            return new TimeInterval(time.Date, time.Date.AddDays(1));
        }
    }
}
