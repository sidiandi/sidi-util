using Sidi.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Util
{
    public class HtmlCellFormat<T>
    {
        public Func<T, int, ListFormat<T>.Column, object> Format = (r, i, c) => c.GetText(r, i);
    }

    public class HtmlRowFormat<T>
    {
        public HtmlRowFormat()
        {
            Format = (row, index, list) => h.tr
                    (
                        list.Columns.Select(c => h.td
                            (
                                c.Tag<HtmlCellFormat<T>>().Format(row, index, c)
                            ))
                    );
        }
        
        HtmlGenerator h = new HtmlGenerator();

        public Func<T, int, ListFormat<T>, object> Format;
    }

    public static class ListFormatHtmlExtension
    {
        public static object Table<T>(this HtmlGenerator h, ListFormat<T> list)
        {
            return h.table
            (
                h.th
                (
                    list.Columns.Select(c => h.td
                    (
                        c.Name
                    ))
                ),
                list.Data.Select((row, index) => list.Tag<HtmlRowFormat<T>>().Format(row, index, list))
            );
        }
    }
}
