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
using System.Text.RegularExpressions;
using System.Diagnostics;
using Sidi.IO;

namespace Sidi.Util
{
    public static class StringEx
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string ShortenMd5(this string text, int maxLength)
        {
            int md5StringLength = 16 * 2 + 1;

            if (!(maxLength > md5StringLength))
            {
                throw new ArgumentOutOfRangeException(String.Format("maxLength must be > {0}", md5StringLength));
            }

            var x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            if (text.Length <= maxLength)
            {
                return text;
            }

            string toEncode = text.Substring(maxLength - md5StringLength);
            return
                text.Substring(0, maxLength) + "." +
                x.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode)).HexString();
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
            return FileUtil.CatDir(new string[] { dir0 }.Concat(dirs).ToArray());
        }

        public static string Join<T>(this IEnumerable<T> enumerable, string separator)
        {
            return String.Join(separator, enumerable.Select(x => x.ToString()).ToArray());
        }

        public static string Join<T>(this IEnumerable<T> e)
        {
            StringWriter w = new StringWriter();
            foreach (var i in e)
            {
                w.WriteLine(i);
            }
            return w.ToString();
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
            StringWriter w = new StringWriter();
            StringReader r = new StringReader(text);
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
                while ((i.Length - s) > columns)
                {
                    int s1 = i.LastIndexOf(" ", s + columns, columns);
                    if (s1 < 0)
                    {
                        s1 = s + columns;
                    }
                    w.WriteLine(i.Substring(s, s1 - s));
                    s = s1 + 1;
                }
                w.Write(i.Substring(s));
            }
            return w.ToString();
        }

        public static string Indent(this string text, string prefix)
        {
            StringWriter w = new StringWriter();
            StringReader r = new StringReader(text);
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

        /// <summary>
        /// Replace the annoying "beep" unicode character
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Printable(this string text)
        {
            return Regex.Replace(text, @"\u2022", "_");
        }
        public static string EditInteractive(this string text)
        {
            string tf = null;
            try
            {
                tf = Path.GetTempFileName();
                File.WriteAllText(tf, text);

                Process p = new Process();

                p.StartInfo.FileName = FileUtil.CatDir(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Notepad++", "notepad++.exe");
                p.StartInfo.Arguments = "-multiInst -nosession " + tf.Quote();

                if (!File.Exists(p.StartInfo.FileName))
                {
                    p.StartInfo.FileName = "notepad.exe";
                    p.StartInfo.Arguments = tf.Quote();
                }

                p.Start();
                log.Info(p.DetailedInfo());
                p.WaitForExit();
                return File.ReadAllText(tf);
            }
            finally
            {
                try
                {
                    File.Delete(tf);
                }
                catch
                {
                }
            }
        }

        public static IEnumerable<string> Lines(this string text)
        {
            StringReader r = new StringReader(text);
            return r.ReadLines();
        }

        public static string GetSection(this string text, string sectionName)
        {
            var e = text.Lines();
            string sectionHead = "[" + sectionName + "]";
            e = e.SkipWhile(x => !x.StartsWith(sectionHead)).Skip(1);
            e = e.TakeWhile(x => !x.StartsWith("["));
            return e.JoinLines();
        }

    }
}
