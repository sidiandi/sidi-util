using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Extensions;

namespace Sidi.CommandLine
{
    [Usage("Time span")]
    [Example("12:34:56")]
    [Example("00:00:01")]
    [Example("90 days")]
    [Example("10 seconds")]
    [Example("0.03 seconds")]
    [Example("3 years")]
    [Example("2 weeks")]
    public class TimeSpanParser : ValueContainer<TimeSpan>, CommandLineHandler2
    {
        [Usage("Seconds")]
        public void Seconds(double x)
        {
            Value = TimeSpan.FromSeconds(x);
        }

        [Usage("Minutes")]
        public void Minutes(double x)
        {
            Value = TimeSpan.FromMinutes(x);
        }

        [Usage("Hours")]
        public void Hours(double x)
        {
            Value = TimeSpan.FromHours(x);
        }

        [Usage("Days")]
        public void Days(double x)
        {
            Value = TimeSpan.FromDays(x);
        }

        [Usage("Weeks")]
        public void Weeks(double x)
        {
            Value = TimeSpan.FromDays(x * 7);
        }

        [Usage("Years")]
        public void Years(double x)
        {
            Value = TimeSpan.FromDays(x * 365.25);
        }

        public void BeforeParse(IList<string> args, Parser parser)
        {
        }

        public void UnknownArgument(IList<string> args, Parser parser)
        {
            if (args.Count >= 2)
            {
                var argCount = 2;
                var rev = args.Take(argCount).Reverse().ToList();
                try
                {
                    if (parser.HandleParserItem(rev))
                    {
                        for (int i = 0; i < argCount; ++i)
                        {
                            args.PopHead();
                        }
                        return;
                    }
                }
                catch
                {
                }
            }

            TimeSpan v;
            if (TimeSpan.TryParse(args.First(), out v))
            {
                this.Value = v;
                args.PopHead();
                return;
            }

        }
    }
}
