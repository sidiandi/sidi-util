// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sidi.CommandLine.Test
{
    [TestFixture]
    class ShowUserInterfaceTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test, Explicit("interactive")]
        public void UserInterface()
        {
            var p = ParserTest.ParserWithAllTestApps();
            p.Parse(new string[] { "ui" });
        }

        [Test, Explicit("UI")]
        public void ActionControl()
        {
            var ui = new ShowUserInterface(null);
            var p = ParserTest.ParserWithAllTestApps();
            var c = ui.ToDialog(p.GetAction("Greet"));
            c.ShowAndExecute();
        }

        [Test, Explicit("UI")]
        public void Manual()
        {
            var ui = new ShowUserInterface(null);
            var p = ParserTest.ParserWithAllTestApps();
            p.Run(new []{"Manual"});
        }

        [Test, Explicit("ui")]
        public void UI2()
        {
            var p = ParserTest.ParserWithAllTestApps();
            new ShowUserInterface2(p).UserInterface();
        }

        [Test, Explicit("UI")]
        public void SubCommandUi()
        {
            var a = new Sidi.CommandLine.Test.ParserTest.TestAppWithStringList();
            var p = new Parser(a);
            new ShowUserInterface(p).GraphicalUserInterface();
        }

        public class TestLog
        {
            public TestLog()
            {
                Test = null;
            }

            [Usage("Test logging")]
            public void DoLog()
            {
                log.Debug(DateTime.Now);
                log.Info(DateTime.Now);
                log.Warn(DateTime.Now);
                log.Error(DateTime.Now);
            }

            [SubCommand]
            public TestLog Test { set; get; }
        }

        [Test]
        public void Logging()
        {
            var a = new TestLog();
            foreach (var level in new[]{"off", "error", "warn", "debug", "all" })
            {
                Parser.Run(a, new[] { "LogLevel", level, "DoLog" });
                Parser.Run(a, new[] { "Test", "DoLog" });
            }
        }
    }
}
