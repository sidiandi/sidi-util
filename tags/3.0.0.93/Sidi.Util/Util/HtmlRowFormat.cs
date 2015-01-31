using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.Net;

namespace Sidi.Util
{
    public class HtmlRowFormat<T>
    {
        public HtmlRowFormat()
        {
            Header = (list) => h.tr
                (
                    list.Columns
                    .Where(c => !c.GetHtmlCellFormat().HasDedicatedRow)
                    .Select(c => h.th
                    (
                        c.Name
                    ))
                );

            RowClass = (row, index) => String.Format("row{0}", index % 2);

            Format = (row, index, list) =>
            {
                var detailColumnColSpan = list.Columns.Where(c => !c.GetHtmlCellFormat().HasDedicatedRow).Count();

                var mainRow = h.tr
                (
                    h.class_(RowClass(row, index)),
                    list.Columns.Where(c => !c.GetHtmlCellFormat().HasDedicatedRow)
                    .Select(c => h.td
                        (
                            c.GetHtmlCellFormat().Format(row, index, c)
                        ))
                );

                if (list.Columns.Any(c => c.GetHtmlCellFormat().HasDedicatedRow))
                {
                    return new[] { mainRow }
                        .Concat(
                        list.Columns.Where(c => c.GetHtmlCellFormat().HasDedicatedRow)
                        .Select(c => h.tr(
                            h.class_(RowClass(row, index)),
                            h.td
                            (
                                h.colspan(detailColumnColSpan),
                                c.Name, ": ", c.GetHtmlCellFormat().Format(row, index, c)
                            ))))
                        // .Concat(new[] { Header(list) })
                        ;
                }
                else
                {
                    return mainRow;
                }
            };
        }

        Sidi.Net.HtmlGenerator h = HtmlGenerator.Instance;

        public Func<T, int, ListFormat<T>, object> Format;

        public Func<T, int, object> RowClass;

        public Func<ListFormat<T>, object> Header;

        public string TableClass = "ListFormat";
    }

}
