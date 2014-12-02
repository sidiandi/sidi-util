using Microsoft.Office.Interop.Excel;
using NUnit.Framework;
using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Office
{
    [TestFixture]
    public class ExcelProviderTest : TestBase
    {
        [Test, Explicit("interactive")]
        public void GetExcelInstance()
        {
            using (var p = ExcelProvider.GetActiveOrNew())
            {
            }
        }

        [Test, Explicit("interactive")]
        public void Nested()
        {
            var text = "hello, test";

            using (var p = ExcelProvider.GetActiveOrNew())
            {
                p.Application.Visible = true;
                p.Application.Workbooks.Add();
                p.ActiveWorksheet.Cells[1, 1].Value = text; 

                using (var p1 = ExcelProvider.GetActiveOrNew())
                {
                    p.Application.Visible = false;
                    Assert.AreEqual(text, p1.ActiveWorksheet.Cells[1, 1].Value);
                }
            }

            Assert.IsNull(ExcelProvider.GetActive());
        }

        [Test, Explicit("interactive")]
        public void GetNew()
        {
            var text = "hello, test";

            using (var p = ExcelProvider.GetNew())
            {
                p.Application.Visible = true;
                p.Application.Workbooks.Add();
                p.ActiveWorksheet.Cells[1, 1].Value = text;

                using (var p1 = ExcelProvider.GetNew())
                {
                    Assert.IsNull(p1.ActiveWorksheet);
                }
            }

            Assert.IsNull(ExcelProvider.GetActive());
        }

        [Test, Explicit("interactive")]
        public void GetActiveSheet()
        {
            var text = "hello, test";

            using (var p = ExcelProvider.GetNew())
            {
                p.Application.Visible = true;
                p.Application.Workbooks.Add();
                p.ActiveWorksheet.Cells[1, 1].Value = text;

                using (var p1 = ExcelProvider.GetActive())
                {
                    Assert.IsNotNull(p1.ActiveWorksheet);
                }
            }

            Assert.IsNull(ExcelProvider.GetActive());
        }
    }
}
