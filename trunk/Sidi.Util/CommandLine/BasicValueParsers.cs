using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Sidi.IO;
using Sidi.Util;
using Sidi.Extensions;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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

        [Usage("Parse file system paths, e.g. C:\\temp\\hello.txt")]
        public class LPathParser : ValueContainer<LPath>, CommandLineHandler2
        {
            [Usage("Prompt for file")]
            public void Ask()
            {
                var dlg = new OpenFileDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Value = dlg.FileName;
                }
                else
                {
                    throw new Exception("Canceled by user");
                }
            }

            [Usage("Use directory currently open in Windows Explorer")]
            public void Current()
            {
                Value = new Shell().GetOpenDirectory();
            }

            [Usage("Use file that is currently selected in Windows Explorer")]
            public void Selected()
            {
                Value = new Shell().SelectedFiles.First();
            }

            [Usage("Use clipboard content")]
            public void Paste()
            {
            }

            public void BeforeParse(IList<string> args, Parser p)
            {
                p.Prefix[typeof(CommandLine.Action)] = new[] { ":" };
            }

            public void UnknownArgument(IList<string> args, Parser p)
            {
                Value = LPath.Parse(args.PopHead());
            }
        }

        [Usage("Parse file system path lists, e.g. C:\\temp\\hello.txt;C:\\temp\\world.txt")]
        public class PathListParser : ValueContainer<PathList>, CommandLineHandler2
        {
            [Usage("Prompt for files")]
            public void Ask()
            {
                var dlg = new OpenFileDialog();
                dlg.Multiselect = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Value = new PathList(dlg.FileNames.Select(x => new LPath(x)));
                }
                else
                {
                    throw new Exception("Canceled by user");
                }
            }

            [Usage("Use file that is currently selected in Windows Explorer")]
            public void Selected()
            {
                Value = new PathList(new Shell().SelectedFiles);
            }

            [Usage("Use clipboard content")]
            public void Paste()
            {
                Value = PathList.ReadClipboard();
            }

            public void BeforeParse(IList<string> args, Parser p)
            {
            }

            public void UnknownArgument(IList<string> args, Parser p)
            {
                if (Value == null)
                {
                    Value = new PathList();
                }
                Value.Add(p.ParseValue<LPath>(args));
            }
        }

        [Usage("Parse date and time values")]
        public class DateTimeParser : ValueContainer<DateTime>, CommandLineHandler
        {
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

            public void BeforeParse(IList<string> args)
            {
            }

            public void UnknownArgument(IList<string> args)
            {
                Value = DateTime.Parse(args.PopHead());
            }
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
                var p = Parser.SingleSource(tsp);
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

        [Usage("Parse time intervals, e.g. [18.04.2013 03:35:04, 18.04.2013 15:35:04[")]
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
        
        /*
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
            var p = Parser.SingleSource(valueParser);
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
        */
    }
}
