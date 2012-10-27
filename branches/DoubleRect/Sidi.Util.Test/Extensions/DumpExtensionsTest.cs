using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;

namespace Sidi.Extensions
{
    [TestFixture]
    public class DumpExtensionsTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Test()
        {
            var files = new DirectoryInfo(TestFile(".")).GetFiles();
            var w = new StringWriter();
            files.PrintTable(w, f => f.Name, f => f.LastWriteTimeUtc, f => f.Length);
            log.Info(w.ToString());    
        }

        [Test]
        public void DumpProperties()
        {
            Process p = Process.GetCurrentProcess();
            p.DumpProperties(Console.Out);
        }
    }
}
