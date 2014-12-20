using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Extensions;

namespace Sidi.Office
{
    public static class ExcelExtensions
    {
        public static Worksheet GetActiveWorksheet(this Application application)
        {
            return ((Worksheet)application.ActiveWorkbook.ActiveSheet);
        }

        public static Worksheet CreateSheet(this Workbook workbook)
        {
            return (Worksheet)workbook.Worksheets.Add();
        }

        public static Workbook ProvideActiveWorkbook(this Application application)
        {
            var w = application.ActiveWorkbook;
            if (w == null)
            {
                w = application.Workbooks.Add();
            }
            return w;
        }

        public static object[,] ToExcelValues(this IEnumerable<IEnumerable<object>> e)
        {
            var rows = e.Count();
            var columns = e.First().Count();

            var a = (object[,])Array.CreateInstance(typeof(object), new[] { rows, columns }, new[] { 1, 1 });

            int r = 1;
            foreach (var rowData in e)
            {
                int c = 1;
                foreach (var data in rowData)
                {
                    a[r, c] = data.ToExcelValue();
                    ++c;
                }
                ++r;
            }
            return a;
        }

        public static object ToExcelValue(this object data)
        {
            if (data == null || data is string || data is double || data is int || data is DateTime || data is Boolean)
            {
                return data;
            }
            else
            {
                return data.SafeToString();
            }
        }

    }
}
