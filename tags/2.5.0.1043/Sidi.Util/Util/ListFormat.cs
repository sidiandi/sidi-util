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
using Sidi.Extensions;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Sidi.Util
{
    public class ListFormat<T>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ListFormat(IEnumerable<T> data)
        {
            this.Data = data;
        }

        public IEnumerable<T> Data { get; private set; }

        public class Column
        {
            public Column()
            {
                MaxWidth = 1000;
                Width = -1;
                AutoWidth = true;
            }

            public string GetText(T x, int index)
            {
                try
                {
                    return f(index, x).SafeToString();
                }
                catch
                {
                    return String.Empty;
                }
            }

            public object GetObject(T x, int index)
            {
                try
                {
                    return f(index, x);
                }
                catch
                {
                    return null;
                }
            }

            public string Name { get; private set; }
            Func<int, T, object> f;

            public Column(string name, Func<int, T, object> f) : this()
            {
                this.f = f;
                Name = name;
            }

            public Column(string name, Func<T, object> f)
                : this()
            {
                this.f = (i, x) => f(x);
                Name = name;
            }

            public int MaxWidth { get; set; }
            public int Width { set; get; }
            public bool AutoWidth { get; set; }
        }

        public ListFormat<T> AddColumn(string caption, Func<T, object> f)
        {
            Columns.Add(new Column(caption, (i,x) => f(x)));
            return this;
        }

        public ListFormat<T> AddColumn(string caption, Func<int, T, object> f)
        {
            Columns.Add(new Column(caption, f));
            return this;
        }

        string GuessColumnName(Func<T, object> f)
        {
            var il = f.Method.GetMethodBody().GetILAsByteArray();
            if (il.Length >= 6 && 0x6f == il[1])
            {
                var mdt = BitConverter.ToInt32(il, 2);
                var m = f.Method.Module.ResolveMethod(mdt);
                var name = m.Name;
                name = Regex.Replace(name, @"^get_", String.Empty);
                return name;
            }

            return f.Method.Name;
        }

        public ListFormat<T> Add(params Func<T, object>[] f)
        {
            foreach (var c in f)
            {
                var m = c.Method;
                Columns.Add(new Column(GuessColumnName(c), c));
            }
            return this;
        }

        public ListFormat<T> Add(params string[] propertyOrFieldNames)
        {
            foreach (var caption in propertyOrFieldNames)
            {
                var p = typeof(T).GetProperty(caption);
                if (p != null)
                {
                    var noArgs = new object[] { };
                    Columns.Add(new Column(caption, x => p.GetValue(x, noArgs)));
                    continue;
                }

                var f = typeof(T).GetField(caption);
                if (f != null)
                {
                    Columns.Add(new Column(caption, x => f.GetValue(x)));
                    continue;
                }

                throw new ArgumentOutOfRangeException(caption);
            }
            return this;
        }

        public ListFormat<T> AllPublic()
        {
            return AllProperties().AllFields();
        }

        public ListFormat<T> Property(params string[] properties)
        {
            foreach (var name in properties)
            {
                var member = typeof(T).GetProperty(name);
                if (member == null)
                {
                    throw new ArgumentOutOfRangeException("properties", name);
                }
                AddColumn(member.Name, item => member.GetValue(item, new object[] { }));
            }
            return this;
        }

        public ListFormat<T> AllProperties()
        {
            return Property(typeof(T).GetProperties().Select(x => x.Name).ToArray());
        }

        public ListFormat<T> Field(params string[] fields)
        {
            foreach (var m in fields.Select(f => typeof(T).GetField(f)))
            {
                var member = m;
                AddColumn(m.Name, item => member.GetValue(item));
            }
            return this;
        }

        public ListFormat<T> AllFields()
        {
            return Field(typeof(T).GetFields().Select(x => x.Name).ToArray());
        }

        public IList<Column> Columns = new List<Column>();
        public string ColumnSeparator = "|";

        public void RenderText()
        {
            RenderText(Console.Out);
        }

        public ListFormat<T> DefaultColumns()
        {
            if (!Columns.Any())
            {
                AddColumn("#", (i, x) => i);
                AddColumn(typeof(T).Name, x => x);
            }
            return this;
        }

        public void RenderText(TextWriter o)
        {
            DefaultColumns();

            var rows = Data
                .Select((item, index) => Columns.Select(x => x.GetText(item, index)).ToArray())
                .ToList();

            // determine column widths
            for (int c = 0; c < Columns.Count; ++c)
            {
                if (Columns[c].AutoWidth)
                {
                    Columns[c].Width = Math.Max(Columns[c].Width, Columns[c].Name.Length);
                }
            }
            foreach (var i in rows)
            {
                for (int c = 0; c < Columns.Count; ++c)
                {
                    if (Columns[c].AutoWidth)
                    {
                        Columns[c].Width = Math.Max(Columns[c].Width, i[c].Length);
                    }
                }
            }
            for (int c = 0; c < Columns.Count; ++c)
            {
                if (Columns[c].AutoWidth)
                {
                    Columns[c].Width = Math.Min(Columns[c].Width, Columns[c].MaxWidth);
                }
            }

            // print
            var columnFormat = Columns.Select(x => String.Format("{{0,-{0}}}{1}", x.Width, ColumnSeparator)).ToArray();

            // separator
            RenderMultiLine(o, Columns.Select(x => new String('-', x.Width)).ToArray(), columnFormat);

            // header
            RenderMultiLine(o, Columns.Select(x => x.Name).ToArray(), columnFormat);

            // separator
            RenderMultiLine(o, Columns.Select(x => new String('-', x.Width)).ToArray(), columnFormat);

            // rows
            foreach (var rowData in rows)
            {
                RenderMultiLine(o, rowData, columnFormat);
            }
        }

        public override string ToString()
        {
            using (var w = new StringWriter())
            {
                w.WriteLine();
                RenderText(w);
                return w.ToString();
            }
        }

        public void RenderDetails(TextWriter o)
        {
            DefaultColumns();

            var columnWidth = Columns.Max(x => x.Name.Length);

            var rows = Data
                .Select((item, index) => Columns.Select(x => x.GetText(item, index)).ToArray());

            var rowFormat = String.Format("{{0,-{0}}}: {{1}}", columnWidth);
            foreach (var i in rows)
            {
                for (int c = 0; c < Columns.Count; ++c)
                {
                    o.WriteLine(rowFormat, Columns[c].Name, i[c]);
                }
                o.WriteLine();
            }
        }

        void RenderMultiLine(TextWriter o, string[] rowData, string[] columnFormat)
        {
            // calculate number of required text rows
            int textRows = 0;
            for (int c = 0; c < Columns.Count; ++c)
            {
                textRows = Math.Max(textRows, (rowData[c].Length - 1) / Columns[c].Width + 1);
            }

            for (int tr = 0; tr < textRows; ++tr)
            {
                for (int c = 0; c < Columns.Count; ++c)
                {
                    var text = rowData[c].SafeSubstring(tr * Columns[c].Width, Columns[c].Width);
                    o.Write(String.Format(columnFormat[c], text));
                }
                o.WriteLine();
            }
        }

        public Chart Chart()
        {
            var c = new Chart();
            var ca = new ChartArea();
            c.ChartAreas.Add(ca);
            c.Legends.Add(new Legend());

            var x = this.Columns[0];
            var xValues = Data.Select(x.GetObject).ToList();

            foreach (var y in this.Columns.Skip(1))
            {
                var series = new Series(y.Name)
                {
                    ChartType = SeriesChartType.Line,
                };
                series.Points.DataBindXY(xValues,
                    this.Data.Select(y.GetObject).ToList());
                c.Series.Add(series);
            }
            
            return c;
        }

        public Chart Bubbles()
        {
            var c = new Chart();
            var ca = new ChartArea();
            c.ChartAreas.Add(ca);
            c.Legends.Add(new Legend());

            var xValues = Columns[0];
            var yValue = Columns[1];
            var bubbleSize = Columns[2];
            
            {
                var series = new Series(yValue.Name)
                {
                    ChartType = SeriesChartType.Bubble,
                    YValuesPerPoint = 2,
                    XValueType = ChartValueType.DateTime,
                    
                };
                series.Points.DataBindXY(
                    this.Data.Select(xValues.GetObject).ToList(),
                    this.Data.Select(yValue.GetObject).ToList(),
                    this.Data.Select(bubbleSize.GetObject).ToList()
                    );
                c.Series.Add(series);
            }

            return c;
        }
    }
}
