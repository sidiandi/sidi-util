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
using System.Diagnostics;
using Sidi.IO;
using Sidi.Extensions;
using Microsoft.CSharp;
using System.CodeDom;

namespace Sidi.Extensions
{
    public static class StringExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static System.Security.Cryptography.MD5CryptoServiceProvider serviceProvider = new System.Security.Cryptography.MD5CryptoServiceProvider();
        
        public static string ShortenMd5(this string text, int maxLength)
        {
            int md5StringLength = 16 * 2 + 1;

            if (!(maxLength > md5StringLength))
            {
                throw new ArgumentOutOfRangeException(String.Format("maxLength must be > {0}", md5StringLength));
            }

                if (text.Length <= maxLength)
                {
                    return text;
                }

                string toEncode = text.Substring(maxLength - md5StringLength);
                return
                    text.Substring(0, maxLength) + "." +
                    serviceProvider.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode)).HexString();
        }

        public static string Unquote(this string text)
        {
            if (text.StartsWith("\"") && text.EndsWith("\""))
            {
                return text.Substring(1, text.Length - 2);
            }
            else
            {
                return text;
            }
        }

        public static string OneLine(this string text, int maxLen)
        {
            using (var r = new StringReader(text))
            using (var w = new StringWriter())
            {
                for (string line = r.ReadLine(); ; )
                {
                    w.Write(line);
                    line = r.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    w.Write(" ");
                }
                string result = w.ToString();
                if (result.Length > maxLen)
                {
                    return result.Substring(0, maxLen);
                }
                else
                {
                    return result;
                }
            }
        }

        public static string JoinLines(this IEnumerable<string> lines)
        {
            bool first = true;
            StringBuilder s = new StringBuilder();
            foreach (string i in lines)
            {
                if (!String.IsNullOrEmpty(i))
                {
                    if (!first)
                    {
                        s.AppendLine();
                    }
                    s.Append(i);
                    first = false;
                }
            }
            return s.ToString();
        }

        public static string Quote(this string s)
        {
            return "\"" + s + "\"";
        }

        public static string Join<T>(this IEnumerable<T> enumerable, string separator)
        {
            return String.Join(separator, enumerable.SafeSelect(x => x.SafeToString()).ToArray());
        }

        public static string Join<T>(this IEnumerable<T> e)
        {
            return e.Join("\r\n");
        }

        /// <summary>
        /// Converts a action that writes to a TextWriter to a string
        /// </summary>
        /// <param name="generator">Function that writes something to a TextWriter</param>
        /// <returns>Output string</returns>
        public static string ToString(this Action<TextWriter> generator)
        {
            using (var w = new StringWriter())
            {
                generator(w);
                return w.ToString();
            }
        }

        /// <summary>
        /// A shorter way to write String.Format
        /// </summary>
        /// <param name="formatString"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string F(this string formatString, params object[] args)
        {
            return String.Format(formatString, args);
        }

        public static IEnumerable<string> ReadLines(this TextReader reader)
        {
            for (; ; )
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                yield return line;
            }
        }

        public static string Wrap(this string text, int columns)
        {
            using (var w = new StringWriter())
            using (var r = new StringReader(text))
            {
                bool first = true;
                foreach (string i in r.ReadLines())
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        w.WriteLine();
                    }

                    int s = 0;
                    var prefix = Regex.Match(i, @"^\s*").Value;
                    while ((i.Length - s) > columns)
                    {
                        int s1 = i.LastIndexOf(" ", s + columns, columns);
                        if (s1 < 0)
                        {
                            s1 = s + columns;
                        }
                        if (s == 0)
                        {
                            w.WriteLine(i.Substring(s, s1 - s));
                        }
                        else
                        {
                            w.WriteLine(prefix + i.Substring(s, s1 - s));
                        }
                        s = s1 + 1;
                    }
                    if (s == 0)
                    {
                        w.Write(i.Substring(s));
                    }
                    else
                    {
                        w.Write(prefix + i.Substring(s));
                    }
                }
                return w.ToString();
            }
        }

        public static string Indent(this string text, string prefix)
        {
            using (var w = new StringWriter())
            using (var r = new StringReader(text))
            {
                bool first = true;
                foreach (string i in r.ReadLines())
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        w.WriteLine();
                    }
                    w.Write(prefix + i);
                }
                return w.ToString();
            }
        }

        /// <summary>
        /// Replace the annoying "beep" unicode character
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Printable(this string text)
        {
            return Regex.Replace(text, @"\u2022", "_");
        }

        public static IEnumerable<string> Lines(this string text)
        {
            using (var r = new StringReader(text))
            {
                for (string line = r.ReadLine(); line != null; line = r.ReadLine())
                {
                    yield return line;
                }
            }
        }

        public static string GetSection(this string text, string sectionName)
        {
            var e = text.Lines();
            string sectionHead = "[" + sectionName + "]";
            e = e.SkipWhile(x => !x.StartsWith(sectionHead)).Skip(1);
            e = e.TakeWhile(x => !x.StartsWith("["));
            return e.JoinLines();
        }

        public static string ToLiteral(this string input)
        {
            using (var writer = new StringWriter())
            using (var provider = new CSharpCodeProvider())
            {
                provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                return writer.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Converts to a string, even if ToString() raises an exception or x is null.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string SafeToString(this object x)
        {
            try
            {
                if (x == null)
                {
                    return String.Empty;
                }
                else
                {
                    return x.ToString();
                }
            }
            catch (Exception ex)
            {
                log.Warn(ex);
                return String.Empty;
            }
        }

        public static string SafeSubstring(this string x, int startIndex, int length)
        {
            var l = x.Length;
            length = Math.Min(length, l - startIndex);
            if (length <= 0)
            {
                return String.Empty;
            }
            return x.Substring(startIndex, length);
        }

        public static IEnumerable<string> SplitFixedWidth(this string x, int width)
        {
            for (int i = 0; i < x.Length; i += width)
            {
                yield return x.SafeSubstring(i, width);
            }
        }
    }
}
