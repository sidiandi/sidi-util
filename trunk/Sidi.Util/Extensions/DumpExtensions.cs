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
            bool singleInstance = false;
            object[] list;
            if (x is System.Collections.IEnumerable && !(x is string))
            {
                list = ((System.Collections.IEnumerable)x).Cast<object>().ToArray();
                x = list[0];
            }
            else
            {
                list = new object[] { x };
                singleInstance = true;
            }

            var lf = list.ListFormat();

            lf.AddColumn("ToString", _ => _.ToString());
            lf.AddColumn("Type", _ => _.GetType());

            foreach (var i in x.GetType().GetProperties())
            {
                var property = i;
                lf.AddColumn(i.Name, y => property.GetValue(y, new object[]{}));
            }
            foreach (var i in x.GetType().GetFields())
            {
                var field = i;
                lf.AddColumn(i.Name, y => field.GetValue(y));
            }

            if (singleInstance)
            {
                lf.RenderDetails(o, x);
            }
            else
            {
                lf.RenderDetails(o);
            }
        }

        public static string DumpProperties(this object x)
        {
            using (var w = new StringWriter())
            {
                x.DumpProperties(w);
                return w.ToString();
            }
        }

        public static ListFormat<T> ListFormat<T>(this IEnumerable<T> e)
        {
            return new ListFormat<T>(e);
        }

        public static ListFormat<T> ListFormat<T>(this IEnumerable<T> e, params Func<T,object>[] columns)
        {
            var lf = new ListFormat<T>(e);
            lf.Add(columns);
            return lf;
        }

        public static ListFormat<IGrouping<TKey, TSource>> ListCount<TSource, TKey>(this IEnumerable<IGrouping<TKey, TSource>> groups)
        {
            return groups.ListFormat()
                .AddColumn("Key", x => x.Key)
                .AddColumn("Count", x => x.Count());
        }

        public static ListFormat<KeyValuePair<string, int>> ListCountPercent<TSource, TKey>(this IEnumerable<IGrouping<TKey, TSource>> groups)
        {
            var kv = groups.Select(x => new KeyValuePair<string, int>(x.Key.ToString(), x.Count()))
                .ToList();

            var total = kv.Sum(x => x.Value);
            kv.Insert(0, new KeyValuePair<string, int>("Total", total));

            var pf = 100.0 / (double) total;

            return kv
                .ListFormat()
                .AddColumn("Key", x => x.Key)
                .AddColumn("Count", x => x.Value)
                .AddColumn("Percent", x => String.Format("{0:F2}%", pf * x.Value));
        }
    }
}
