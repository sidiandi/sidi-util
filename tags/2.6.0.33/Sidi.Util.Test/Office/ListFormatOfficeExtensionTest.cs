using Sidi.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Extensions;
using Microsoft.Office.Interop.Excel;
using NUnit.Framework;

namespace Sidi.Office.Test
{
    [TestFixture]
    class ListFormatOfficeExtensionTest : Sidi.Test.TestBase
    {
        [Test, Explicit("Opens Excel sheet")]
        public void WriteList()
        {
            var sampleData = Paths.Temp.Info.GetChildren();

            using (var p = ExcelProvider.GetActiveOrNew())
            {
                p.Application.Visible = true;
                var sheet = p.Application.ProvideActiveWorkbook().CreateSheet();
                sampleData
                    .ListFormat().AllPublic()
                    .Write(sheet.Cells);

                p.KeepAlive();
            }
        }

    }
}
