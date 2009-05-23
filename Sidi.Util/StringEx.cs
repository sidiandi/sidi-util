// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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

namespace Sidi.Util
{
    public static class StringEx
    {
        public static string OneLine(this string text, int maxLen)
        {
            StringReader r = new StringReader(text);
            StringWriter w = new StringWriter();
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

        public static string CatDir(this string dir0, params string[] dirs)
        {
            foreach (string i in dirs)
            {
                dir0 = System.IO.Path.Combine(dir0, i);
            }
            return dir0;
        }

        public static string Join<T>(this IEnumerable<T> enumerable, string separator)
        {
            return String.Join(separator, enumerable.Select(x => x.ToString()).ToArray());
        }

        /// <summary>
        /// Converts a action that writes to a TextWriter to a string
        /// </summary>
        /// <param name="generator">Function that writes something to a TextWriter</param>
        /// <returns>Output string</returns>
        public static string ToString(this Action<TextWriter> generator)
        {
            TextWriter w = new StringWriter();
            generator(w);
            return w.ToString();
        }

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
    }
}
