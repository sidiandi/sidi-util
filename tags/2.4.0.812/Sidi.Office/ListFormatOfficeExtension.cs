using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;
using Sidi.Util;

namespace Sidi.Extension
{
    public static class ListFormatOfficeExtension
    {
        public static void WriteExcelSheet<T>(this ListFormat<T> listFormat, string excelFile)
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

            foreach (var c in listFormat.Columns)
            {
                sheet.Cells[row, column++] = c.Name;
            }

            foreach (var i in listFormat.Data)
            {
                ++row;
                column = 1;
                foreach (var c in listFormat.Columns)
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
    }
}
