using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Sidi.IO;
using Sidi.Util;
using Sidi.Extensions;
using System.Text.RegularExpressions;

namespace Sidi.CommandLine
{
    [Usage("Value parsers for basic types")]
    public class BasicValueParsers
    {
        static CultureInfo cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();

        [Usage("Boolean value")]
        [Example("true")]
        [Example("false")]
        public static bool ParseBool(string stringRepresentation)
        {
            return bool.Parse(stringRepresentation);
        }

        [Usage("Integer value")]
        [Example("42")]
        public static int ParseInt(string stringRepresentation)
        {
            return int.Parse(stringRepresentation, cultureInfo);
        }

        [Usage("Floating point value")]
        [Example("123.456")]
        [Example("12e5")]
        public static double ParseDouble(string stringRepresentation)
        {
            return double.Parse(stringRepresentation, cultureInfo);
        }

        [Usage("String")]
        [Example("Hello")]
        public static string ParseString(string stringRepresentation)
        {
            return stringRepresentation;
        }

        class DateTimeParser
        {
            public DateTime Value;

            [Usage("Now")]
            public void Now()
            {
                Value = DateTime.Now;
            }

            [Usage("Tomorrow")]
            public void Tomorrow()
            {
                Value = DateTime.Today.AddDays(1);
            }

            [Usage("Today")]
            public void Today()
            {
                Value = DateTime.Today;
            }

            [Usage("Yesterday")]
            public void Yesterday()
            {
                Value = DateTime.Today.AddDays(-1);
            }
        }
        
        [Usage("Absolute time")]
        [Example("2013-04-10")]
        [Example("now")]
        [Example("today")]
        [Example("yesterday")]
        [Example("tomorrow")]
        public static DateTime ParseDateTime(IList<string> args)
        {
            DateTime v;
            if (DateTime.TryParse(args[0], out v))
            {
                args.PopHead();
                return v;
            }

            var tsp = new DateTimeParser();
            var p = new Parser()
            {
                Applications = new List<object> { tsp }
            };
            p.ParseSingleCommand(args);
            return tsp.Value;
        }

        class TimeSpanParser
        {
            public TimeSpan Value;

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
        }

        [Usage("Time span")]
        [Example("12:34:56")]
        [Example("00:00:01")]
        [Example("90 days")]
        [Example("10 seconds")]
        [Example("0.03 seconds")]
        [Example("3 years")]
        [Example("2 weeks")]
        public static TimeSpan ParseTimeSpan(IList<string> args)
        {
            try
            {
                var v = DateTime.Parse(args[0], cultureInfo).TimeOfDay;
                args.PopHead();
                return v;
            }
            catch
            {
                var tsp = new TimeSpanParser();
                var p = new Parser()
                {
                    Applications = new List<object>{tsp}
                };
                var args2 = args.Take(2).Reverse().ToList();
                p.ParseSingleCommand(args2);
                if (args2.Count == 0)
                {
                    args.PopHead();
                    args.PopHead();
                }
                return tsp.Value;
            }
        }

        class TimeIntervalParser
        {
            public TimeIntervalParser()
            {
            }

            public TimeInterval Value = TimeInterval.MaxValue;

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
        }
        
        [Usage("Time interval with begin and end time")]
        [Example("year 2013-01-01")]
        [Example("year today")]
        [Example("FinancialYear today")]
        [Example("last 90 days")]
        [Example("next 12 hours")]
        [Example("begin 2013-03-01 end 2013-04-10")]
        [Example("[18.04.2013 03:35:04, 18.04.2013 15:35:04[")]
        // [Example("begin yesterday end tomorrow")]
        public static TimeInterval ParseTimeInterval(IList<string> args)
        {
            if (args.First().StartsWith("["))
            {
                int lastArg = 0;
                for (; lastArg < args.Count; ++lastArg)
                {
                    if (args[lastArg].EndsWith("]"))
                    {
                        break;
                    }
                }
                var timeIntervalString = args.Take(lastArg).Join(" ");

                var re = new Regex(@"\[\s*(?<begin>[^,]+)\,\s*(?<end>[^\]]+)\s*\[");
                var m = re.Match(timeIntervalString);
                if (m.Success)
                {
                    var v = new TimeInterval(
                        ParseDateTime(new List<string>(){m.Groups["begin"].Value }), 
                        ParseDateTime(new List<string>(){m.Groups["end"].Value }));
                    for (int i = 0; i < lastArg; ++i)
                    {
                        args.PopHead();
                    }
                    return v;
                }
            }

            if (Regex.IsMatch(args.First(), @"^\[.*\]$"))
            {
                var value = TimeInterval.Parse(args.First());
                args.PopHead();
                return value;
            }

            var valueParser = new TimeIntervalParser();
            var p = new Parser()
            {
                Applications = new List<object> { valueParser }
            };
            while (args.Any())
            {
                if (args.First().Equals(Parser.ListTerminator))
                {
                    args.PopHead();
                    break;
                }
                p.ParseSingleCommand(args);
            }
            return valueParser.Value;
        }

        [Usage("File system path.")]
        [Example(@"C:\temp")]
        public static LPath ParseLPath(string stringRepresentation)
        {
            return LPath.Parse(stringRepresentation);
        }

        [Usage("List of file system paths, separated by \";\"")]
        [Example(@"C:\temp;C:\work")]
        public static PathList ParsePathList(string stringRepresentation)
        {
            return PathList.Parse(stringRepresentation);
        }
    }
}
