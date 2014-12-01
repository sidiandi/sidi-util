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
    [ValueParser(typeof(DateTimeParser))]
    [ValueParser(typeof(LPathParser))]
    [ValueParser(typeof(PathListParser))]
    [ValueParser(typeof(TimeIntervalParser))]
    [ValueParser(typeof(TimeSpanParser))]
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
    }
}
