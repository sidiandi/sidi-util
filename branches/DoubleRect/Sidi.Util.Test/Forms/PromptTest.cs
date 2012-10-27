using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Diagnostics;
using Sidi.Extensions;

namespace Sidi.Forms
{
    [TestFixture]
    public class PromptTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test, Explicit("interactive")]
        public void Edit()
        {
            var text = "Hello, World";
            var result = Prompt.EditInteractive(text);
            log.Info(result);
        }

        [Test, Explicit("interactive")]
        public void Choose()
        {
            var p = Process.GetProcesses();
            Prompt.ChooseOne(p.ListFormat().AddColumn("Name", x => x.ProcessName).AddColumn("id", x => x.Id));
        }
    }
}
