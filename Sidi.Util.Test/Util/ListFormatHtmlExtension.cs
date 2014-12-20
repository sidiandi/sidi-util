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
        public bool HasDedicatedRow = false;
    }

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
        
        HtmlGenerator h = new HtmlGenerator();

        public Func<T, int, ListFormat<T>, object> Format;

        public Func<T, int, object> RowClass;

        public Func<ListFormat<T>, object> Header;

        public string TableClass = "ListFormat";
    }

    public static class ListFormatHtmlExtension
    {
        public static HtmlCellFormat<T> GetHtmlCellFormat<T>(this ListFormat<T>.Column column)
        {
            return column.Tag<HtmlCellFormat<T>>();
        }

        public static object Table<T>(this HtmlGenerator h, ListFormat<T> list)
        {
            var rf = list.Tag<HtmlRowFormat<T>>();
            return h.table
            (
                h.class_(rf.TableClass),
                rf.Header(list),
                list.Data.Select((row, index) => rf.Format(row, index, list))
            );
        }

        public static object DetailsTable<T>(this HtmlGenerator h, ListFormat<T> list)
        {
            var rf = list.Tag<HtmlRowFormat<T>>();
            return h.table
            (
                h.class_(rf.TableClass),
                h.tr(h.th("Property"), h.th("Value")),
                list.Data.SelectMany((row, index) => list.Columns
                    .Select(c => h.tr(
                        h.class_(list.Tag<HtmlRowFormat<T>>().RowClass(row, index)),
                        h.td(c.Name), 
                        h.td(c.GetHtmlCellFormat().Format(row, index, c))))
                    )
            );
        }

        public static object TableStyle(this HtmlGenerator h)
        {
            return h.style(h.type("text/css"), @"

    .ListFormat{
		width:100%; 
		border-collapse:collapse; 
	}
	.ListFormat td{ 
		padding:7px; border:black 1px solid;
	}
	/* provide some minimal visual accomodation for IE8 and below */
	.ListFormat tr{
		background: white;
	}
	/*  Define the background color for all the ODD background rows  */
	.ListFormat .row1 { 
		background: lightgray;
	}
	/*  Define the background color for all the EVEN background rows  */
	.ListFormat .row0{
		background: white;
	}
");
        }
    }
}
