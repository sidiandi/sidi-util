using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sidi.Extensions;
using Sidi.Util;

namespace Sidi.CommandLine
{
    [Usage("Time interval")]
    [Example("year 2013-01-01")]
    [Example("year today")]
    [Example("FinancialYear today")]
    [Example("last 90 days")]
    [Example("next 12 hours")]
    [Example("(begin 2013-03-01 end 2013-04-10)")]
    [Example("[18.04.2013 03:35:04, 18.04.2013 15:35:04[")]
    public class TimeIntervalParser : ValueContainer<TimeInterval>, CommandLineHandler2
    {
        public TimeIntervalParser()
        {
            Value = TimeInterval.MaxValue;
        }

        [Usage("Time interval that ends now and starts in the past")]
        public void Last(TimeSpan duration)
        {
            Value = new TimeInterval(duration, DateTime.Now);
        }

        [Usage("Time interval that begins now and ends in the future")]
        public void Next(TimeSpan duration)
        {
            Value = new TimeInterval(DateTime.Now, duration);
        }

        [Usage("Define begin of time interval")]
        public void Begin(DateTime time)
        {
            Value = new TimeInterval(time, Value.End);
        }

        [Usage("Define end of time interval")]
        public void End(DateTime time)
        {
            Value = new TimeInterval(Value.Begin, time);
        }

        [Usage("Whole year")]
        public void Year(DateTime time)
        {
            Value = TimeInterval.Year(time);
        }

        [Usage("Whole financial year")]
        public void FinancialYear(DateTime time)
        {
            Value = TimeInterval.Year(time, 9);
        }

        public void BeforeParse(IList<string> args, Parser parser)
        {
            if (args.First().StartsWith("["))
            {
                string text = String.Empty;
                for (; ; )
                {
                    var n = args.PopHead();
                    if (n.EndsWith("["))
                    {
                        text += " " + n.Substring(0, n.Length - 1);
                        break;
                    }
                    else
                    {
                        text += " " + n;
                    }
                }
                var p = Regex.Split(text, @"\s*[\[\],]\s*");

                Value = new TimeInterval(
                    parser.ParseValue<DateTime>(p[1]),
                    parser.ParseValue<DateTime>(p[2]));
            }
        }

        public void UnknownArgument(IList<string> args, Parser parser)
        {
        }
    }
}
