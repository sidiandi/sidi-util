using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sidi.CommandLine.Test
{
    [TestFixture]
    class ShowUserInterfaceTest
    {
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
            var c = ui.ToDialog(p.GetAction("SayHelloTo"));
            System.Windows.Forms.Application.Run(c);
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
            new ShowUserInterface(p).UserInterface();
        }
    }
}
