using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.CommandLine;

namespace Sidi.Util
{
    [Usage("Time interval")]
    public class TimeIntervalParser
    {
        public static TimeInterval Parse(string value)
        {
            var tip = new TimeIntervalParser();
            var p = new Parser(tip);
            p.Parse(Tokenizer.ToArray(value));
            return tip.Value;
        }

        public TimeIntervalParser()
        {
            Value = new TimeInterval(DateTime.Now, TimeSpan.Zero);
        }

        public TimeInterval Value { get; set; }

        [Usage("Current day as time interval")]
        public void Today()
        {
            Value = TimeInterval.Today;
        }

        [Usage("Current year")]
        public void CurrentYear()
        {
            Value = TimeInterval.Year(DateTime.Now);
        }

        [Usage("Year")]
        public void Year(int year)
        {
            Value = TimeInterval.Year(new DateTime(year, 1, 1));
        }

        [Usage("Specify begin time")]
        public void From(DateTime time)
        {
            Value = new TimeInterval(time, Value.End);
        }

        [Usage("Specify end time")]
        public void To(DateTime time)
        {
            Value = new TimeInterval(Value.Begin, time);
        }
    }
}
