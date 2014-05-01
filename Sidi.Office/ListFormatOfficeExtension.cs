using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;
using Sidi.Util;
using Sidi.IO;
using Sidi.Office;
using Microsoft.Office.Interop.Excel;
using System.Collections;

namespace Sidi.Extensions
{
    public static class ListFormatOfficeExtension
    {
        public static void WriteExcelSheet<T>(this ListFormat<T> listFormat, LPath excelFile)
        {
            using (var ap = ExcelProvider.GetActiveOrNew())
            {
                // Start a new workbook in Excel.
                var excel = ap.Application;
                excel.Visible = true;

                var books = (Excel.Workbooks)excel.Workbooks;
                var book = (Excel._Workbook)(books.Add(System.Reflection.Missing.Value));

                // Add data to cells in the first worksheet in the new workbook.
                var sheets = (Excel.Sheets)book.Worksheets;
                var sheet = (Excel.Worksheet)sheets[1];

                int row = 1;
                int column = 1;

                foreach (var c in listFormat.Columns)
                {
                    sheet.Cells[row, column++] = c.Name;
                }

                int index = 0;
                foreach (var i in listFormat.Data)
                {
                    ++row;
                    column = 1;
                    foreach (var c in listFormat.Columns)
                    {
                        sheet.Cells[row, column++] = c.GetText(i, index++);
                    }
                }
            }
        }

        public static void Write<T>(this ListFormat<T> listFormat, Range range)
        {
            int row = 1;
            int column = 1;

            var a = 
                new[] { listFormat.Columns.Select(x => (object)x.Name) }
                .Concat(listFormat.Data.Select((data, index) => 
                    listFormat.Columns.Select(c => c.GetObject(data, index))))
                .ToExcelValues();

            var listRange = (Range) range.Range[range.Cells[row, column], range.Cells[row + listFormat.Data.Count() - 1, column + listFormat.Columns.Count - 1]];
            listRange.Value = a;
            var listObject = range.Worksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange, listRange, Type.Missing, XlYesNoGuess.xlYes);
            listRange.Columns.AutoFit();
        }
    }
}
