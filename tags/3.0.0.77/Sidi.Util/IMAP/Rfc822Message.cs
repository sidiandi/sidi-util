// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using Sidi.Util;
using Sidi.Extensions;

namespace Sidi.Imap
{
    public class Rfc822DateTime
    {
        public DateTime Date;
        public TimeSpan TimeZone;

        static readonly string[] months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        public override string ToString()
        {
                var t = Date;
                return String.Format(
                    "{0:D2}-{1}-{2:D4} {3:D2}:{4:D2}:{5:D2} {6}{7:D2}{8:D2}",
                    t.Day,
                    Rfc822DateTime.months[t.Month-1],
                    t.Year,
                    t.Hour,
                    t.Minute,
                    t.Second,
                    TimeZone.TotalSeconds < 0 ? "-" : "+",
                    Math.Abs(TimeZone.Hours),
                    Math.Abs(TimeZone.Minutes));
        }
        
        public static Rfc822DateTime Parse(string date)
        {
            var p = new ArgParser(date);

            var dayRegex = new Regex("Mon|Tue|Wed|Thu|Fri|Sat|Sun");
            var monthRegex = new Regex(months.Join("|"));

            if (p.IsNext(dayRegex))
            {
                var weekday = p.Get(dayRegex);
                p.Consume(",");
            }
            p.SkipSpace();
            var day = p.GetNumber();
            p.SkipSpace();
            var monthName = p.Get(monthRegex);
            var month = 1 + Array.IndexOf(months, monthName);
            p.SkipSpace();
            var year = p.GetNumber();
            p.SkipSpace();

            var hour = p.GetNumber();
            p.Consume(":");
            var minute = p.GetNumber();
            var second = 0;
            if (p.ConsumeIfNext(":"))
            {
                second = p.GetNumber();
            }

            var zoneName = new string[]{"UT", "GMT", "EST", "EDT", "CST", "CDT", "MST", "MDT", "PST", "PDT", "1ALPHA"};
            var zoneOffset = new int[]{ 0   , 0    , -5,    -4,    -6,    -5,    -7,    -6,    -8,    -7,    -1};
            var zoneRegex = new Regex(zoneName.Join("|"));


            int timeZone = 0; // in seconds
            p.SkipSpace();
            if (p.IsNext(zoneRegex))
            {
                var zone = p.Get(zoneRegex);
                timeZone = zoneOffset[Array.IndexOf(zoneName, zone)] * 3600;
            }
            else
            {
                int sign = 0;
                if (p.ConsumeIfNext("-"))
                {
                    sign = -1;
                }
                else
                {
                    p.Consume("+");
                    sign = 1;
                }

                timeZone = sign * (60 * p.GetNumber(2) + p.GetNumber(2)) * 60;
            }

            return new Rfc822DateTime()
            {
                Date = new DateTime(year, month, day, hour, minute, second),
                TimeZone = TimeSpan.FromSeconds(timeZone)
            };
        }
    }

    public class Rfc822Message
    {
        public Rfc822Message()
        {
        }

        string GetHeaderValue(string key)
        {
            return Headers
                .First(kvp => kvp.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                .Value;
        }

        string GetHeaderValue(MethodBase m)
        {
            return GetHeaderValue(m.Name.Substring("get_".Length).Replace("_", "-"));
        }

        public string Header
        {
            get
            {
                return FormatHeaders(Headers);
            }
        }

        public string HeaderFields(string[] fields)
        {
            return FormatHeaders(Headers.Where(x => fields.Any(f => f.Equals(x.Key, StringComparison.InvariantCultureIgnoreCase))));
        }

        public string HeaderFieldsNot(string[] fields)
        {
            return FormatHeaders(Headers.Where(x => !fields.Any(f => f.Equals(x.Key, StringComparison.InvariantCultureIgnoreCase))));
        }

        const string headerEnd = "\r\n";

        static string ToString(KeyValuePair<string, string> headerItem)
        {
            return String.Format("{0}: {1}", headerItem.Key, headerItem.Value);
        }

        static string FormatHeaders(IEnumerable<KeyValuePair<string, string>> headers)
        {
            return headers.Select(x => ToString(x)).Join() + headerEnd;
        }

        public string Subject { get { return GetHeaderValue(MethodBase.GetCurrentMethod()); } }
        public Rfc822DateTime Date { get { return Rfc822DateTime.Parse(GetHeaderValue(MethodBase.GetCurrentMethod())); } }
        public string From { get { return GetHeaderValue(MethodBase.GetCurrentMethod()); } }
        public string To { get { return GetHeaderValue(MethodBase.GetCurrentMethod()); } }
        public string Message_ID { get { return GetHeaderValue(MethodBase.GetCurrentMethod()); } }

        public string Content_Type { get { return GetHeaderValue(MethodBase.GetCurrentMethod()); } }
        public string Content_Transfer_Encoding { get { return GetHeaderValue(MethodBase.GetCurrentMethod()); } }
        public int Size { get { return Text.Length; } }

        public static Rfc822Message Parse(TextReader r)
        {
            var p = new ArgParser(String.Empty, r, null);
            var m = new Rfc822Message() { Headers = new List<KeyValuePair<string, string>>() };
            p.NextLine();
            while (!String.IsNullOrEmpty(p.Rest))
            {
                m.Headers.Add(Field(p));
            }
            m.Text = p.input.ReadToEnd();
            return m;
        }

        static KeyValuePair<string, string> Field(ArgParser p)
        {
            var key = p.Get(new Regex(@"[\w-]+"));
            p.Get(new Regex(@"\s*"));
            p.Consume(@":");
            using (var value = new StringWriter())
            {
                value.Write(p.Rest);
                p.NextLine();
                while (p.IsNext(new Regex(@"\s+")))
                {
                    value.WriteLine();
                    value.Write(p.Rest);
                    p.NextLine();
                }
                return new KeyValuePair<string, string>(key, value.ToString().Trim());
            }
        }

        public List<KeyValuePair<string, string>> Headers { get; set; }
        public string Text { get; set; }
    }
}
