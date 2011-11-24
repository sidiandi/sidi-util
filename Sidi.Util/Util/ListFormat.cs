using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sidi.Util
{
    public class ListFormat<T>
    {
        public ListFormat(IEnumerable<T> data)
        {
            this.data = data;
        }

        IEnumerable<T> data;

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

        public ListFormat<T> AddColumn(string caption)
        {
            var p = typeof(T).GetProperty(caption);
            var noArgs = new object[]{};
            Columns.Add(new Column(caption, x => p.GetValue(x, noArgs)));
            return this;
        }

        public ListFormat<T> PropertyColumns()
        {
            foreach (var p in typeof(T).GetProperties())
            {
                AddColumn(p.Name);
            }
            return this;
        }

        public IList<Column> Columns = new List<Column>();
        public string ColumnSeparator = "|";
        public int MaxColumnWidth = 60;

        public void RenderText(TextWriter o)
        {
            var rows = data
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
