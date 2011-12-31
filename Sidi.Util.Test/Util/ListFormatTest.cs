using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using Sidi.Extensions;

namespace Sidi.Util
{
    [TestFixture]
    public class ListFormatTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Test()
        {
            var data = new DirectoryInfo(TestFile(".")).GetFiles();

            {
                data.ListFormat()
                    .AddColumn("Name", f => f.Name)
                    .AddColumn("Date", f => f.LastWriteTime)
                    .RenderText(Console.Out);
            }

            {
                data.ListFormat()
                    .Property("Name", "LastWriteTime")
                    .RenderText(Console.Out);
            }

            {
                data.ListFormat()
                    .PropertyColumns()
                    .RenderText(Console.Out);
            }
        }
    }
}
