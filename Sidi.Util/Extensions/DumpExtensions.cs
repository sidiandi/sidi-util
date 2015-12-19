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
using log4net;
using System.Linq.Expressions;

namespace Sidi.Extensions
{
    /// <summary>
    /// Extension methods to ease display of data
    /// </summary>
    public static class DumpExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        /// <summary>
        /// Creates a ListFormat instance for the IEnumerable
        /// </summary>
        /// Use the generated ListFormat instance to display the IEnumerable in tabular form.
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="e">enumerable of items</param>
        /// <returns></returns>
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

        /// <summary>
        /// Dumps the content of a variable to the log
        /// </summary>
        /// <param name="log"></param>
        /// <param name="getter"></param>
        public static void Info(this ILog log, Expression<Func<object>> getter)
        {
            if (log.IsInfoEnabled)
            {
                log.Info(Dumper.Instance.ToString(getter));
            }
        }

        /// <summary>
        /// Dumps the content of a variable to the log
        /// </summary>
        /// <param name="log"></param>
        /// <param name="getter"></param>
        public static void Debug(this ILog log, Expression<Func<object>> getter)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(Dumper.Instance.ToString(getter));
            }
        }
    }
}
