using Sidi.Net;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Extensions
{
    public static class ListFormatHtmlExtension
    {
        public static Sidi.Util.HtmlCellFormat<T> GetHtmlCellFormat<T>(this ListFormat<T>.Column column)
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
            return h.style(h.type("text/css"), h.Raw(@"
.ListFormat
{
	width:100%; 
	border-collapse:collapse; 
}
	
.ListFormat td
{ 
	padding:7px; border:black 1px solid;
	vertical-align:top;
}

/* provide some minimal visual accomodation for IE8 and below */
.ListFormat tr
{
	background: white;
}

/*  Define the background color for all the ODD background rows  */
.ListFormat .row1
{ 
	background: lightgray;
}

/*  Define the background color for all the EVEN background rows  */
.ListFormat .row0
{
    background: white;
}
"));
        }
    }
}
