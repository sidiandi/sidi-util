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
using System.Text;
using System.IO;
using System.Reflection;
using System.Linq;

namespace Sidi.Util
{
    public static class DumpExtensions
    {
        public static string GetString(this PropertyInfo info, object x)
        {
            try
            {
                return info.GetValue(x, new object[]{}).ToString();
            }
            catch (Exception e)
            {
                return "Exception: {0}".F(e.Message);
            }
        }

        public static string GetString(this FieldInfo info, object x)
        {
            try
            {
                return info.GetValue(x).ToString();
            }
            catch (Exception e)
            {
                return "Exception: {0}".F(e.Message);
            }
        }

        public static void DumpProperties(this object x, TextWriter o)
        {
            var list = new List<KeyValuePair<string, Func<string>>>();

            list.AddRange(
                x.GetType().GetProperties().Select(
                    i => new KeyValuePair<string, Func<string>>(i.Name, () => i.GetString(x))));

            list.AddRange(
                x.GetType().GetFields().Select(
                    i => new KeyValuePair<string, Func<string>>(i.Name, () => i.GetString(x))));

            list.Sort(list.Comparer(i => i.Key));
            list.PrintTable(o, i => i.Key, i => i.Value());
        }

        static int MaxColumnWidth = 20;
            
        public static void PrintTable<T>(this IEnumerable<T> e, TextWriter o, params Func<T, string>[] columns)
        {
            var rows = new List<string[]>();
            foreach (var i in e)
            {
                rows.Add(columns.Select(x => x(i)).ToArray());
            }

            int[] w = new int[columns.Length];
            for (int i = 0; i < w.Length; ++i)
            {
                foreach (var r in rows)
                {
                    w[i] = Math.Min(MaxColumnWidth, Math.Max(w[i], r[i].Length));
                }
            }

            foreach (var i in rows)
            {
                for (int c = 0; c < columns.Length; ++c)
                {
                    o.Write(String.Format("{0,-" + w[c] + "}", i[c]));
                    o.Write("|");
                }
                o.WriteLine();
            }
        }
    }
}
