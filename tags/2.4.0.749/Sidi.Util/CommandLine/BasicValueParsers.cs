using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Sidi.IO;

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

        [Usage("Absolute time")]
        [Example("2013-04-10")]
        public static DateTime ParseDateTime(string stringRepresentation)
        {
            return DateTime.Parse(stringRepresentation, cultureInfo);
        }

        [Usage("Time span")]
        [Example("12:34:56")]
        [Example("00:00:01")]
        public static TimeSpan ParseTimeSpan(string stringRepresentation)
        {
            return DateTime.Parse(stringRepresentation, cultureInfo).TimeOfDay;
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
