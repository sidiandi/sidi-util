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
using Excel = Microsoft.Office.Interop.Excel;
using Sidi.Extensions;

namespace Sidi.Util
{
    public class ListFormat<T>
    {
        public ListFormat(IEnumerable<T> data)
        {
            this.Data = data;
        }

        public IEnumerable<T> Data { get; private set; }

        public class Column
        {
            public string GetText(T x)
            {
                try
                {
                    return f(x).SafeToString();
                }
                catch
                {
                    return String.Empty;
                }
            }

            public string Name { get; private set; }
            Func<T, object> f;

            public Column(string name, Func<T, object> f)
            {
                this.f = f;
                Name = name;
            }
        }

        public ListFormat<T> AddColumn(string caption, Func<T, object> f)
        {
            Columns.Add(new Column(caption, f));
            return this;
        }

        public ListFormat<T> Add(params Func<T, object>[] stringifiers)
        {
            foreach (var f in stringifiers)
            {
                AddColumn(String.Empty, f);
            }
            return this;
        }

        public ListFormat<T> Property(params string[] propertyNames)
        {
            foreach (var caption in propertyNames)
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

        public ListFormat<T> PropertyColumns()
        {
            Property(typeof(T).GetProperties().Select(x => x.Name).ToArray());
            return this;
        }

        public IList<Column> Columns = new List<Column>();
        public string ColumnSeparator = "|";
        public int MaxColumnWidth = 60;

        public void RenderText()
        {
            RenderText(Console.Out);
        }

        public ListFormat<T> DefaultColumns()
        {
            if (!Columns.Any())
            {
                AddColumn(typeof(T).Name, x => x.ToString());
            }
            return this;
        }

        public void WriteExcelSheet(string excelFile)
        {
            // Start a new workbook in Excel.
            var excel = new Excel.Application();
            excel.Visible = true;

            var books = (Excel.Workbooks)excel.Workbooks;
            var book = (Excel._Workbook)(books.Add(System.Reflection.Missing.Value));

            // Add data to cells in the first worksheet in the new workbook.
            var sheets = (Excel.Sheets)book.Worksheets;
            var sheet = (Excel.Worksheet)sheets[1];

            int row = 1;
            int column = 1;

            foreach (var c in Columns)
            {
                sheet.Cells[row, column++] = c.Name;
            }

            foreach (var i in Data)
            {
                ++row;
                column = 1;
                foreach (var c in Columns)
                {
                    try
                    {
                        sheet.Cells[row, column++] = c.GetText(i);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void RenderText(TextWriter o)
        {
            DefaultColumns();

            var rows = Data
                .Select(i => Columns.Select(x => x.GetText(i)).ToArray())
                .ToList();

            int[] w = new int[Columns.Count];
            for (int i = 0; i < w.Length; ++i)
            {
                w[i] = Math.Min(MaxColumnWidth, Math.Max(w[i], Columns[i].Name.Length));
                foreach (var r in rows)
                {
                    w[i] = Math.Min(MaxColumnWidth, Math.Max(w[i], r[i].Length));
                }
            }

            // header
            {
                for (int c = 0; c < Columns.Count; ++c)
                {
                    o.Write(String.Format("{0,-" + w[c] + "}", Columns[c].Name));
                    o.Write(ColumnSeparator);
                }
                o.WriteLine();
            }

            
            foreach (var i in rows)
            {
                for (int c = 0; c < Columns.Count; ++c)
                {
                    o.Write(String.Format("{0,-" + w[c] + "}", i[c]));
                    o.Write(ColumnSeparator);
                }
                o.WriteLine();
            }
        }
    }
}
