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
using System.Text;
using System.IO;
using System.Reflection;
using System.Linq;
using Sidi.Util;

namespace Sidi.Extensions
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
            catch (TargetInvocationException e)
            {
                return "Exception: {0}".F(e.InnerException.Message);
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

        public static ListFormat<T> ListFormat<T>(this IEnumerable<T> e)
        {
            return new ListFormat<T>(e);
        }

        public static void PrintTable<T>(this IEnumerable<T> e, TextWriter o, params Func<T, object>[] columns)
        {
            var lf = e.ListFormat();
            foreach (var f in columns)
            {
                lf.AddColumn(String.Empty, f);
            }
            lf.RenderText(o);
        }
    }
}
