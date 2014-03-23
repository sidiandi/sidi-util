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

#region "Mandatory NUnit Imports"
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using NUnit.Framework;
#endregion

//Test Specific Imports
//TODO - Add imports your going to test here
using Sidi.CommandLine;
using cm = System.ComponentModel;
using System.IO;
using Sidi.Util;
using System.Linq;
using Sidi.Test;

namespace Sidi.CommandLine.Test
{
    [TestFixture]
    public class SubCommandTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Usage("Persistent settings")]
        public class Settings
        {
            [Usage("Name"), Persistent]
            public string Name { get; set; }
        }
        
        [Usage("Test app")]
        public class App
        {
            [SubCommand]
            public Math Math;

            [SubCommand, Persistent]
            public Settings Settings;

            [SubCommand]
            public Settings NonPersistentSettings;

            [SubCommand, Persistent(Global = true)]
            public Settings GlobalSettings;
        }

        [Usage("Test app")]
        public class AnotherAppThatUsesGlobalSettings
        {
            [SubCommand, Persistent(Global = true)]
            public Settings GlobalSettings;
        }

        [Usage("Mathematical operations")]
        public class Math
        {
            [Usage("adds a and b")]
            public int Add(int a, int b)
            {
                return a + b;
            }
            [Usage("multiply a and b")]
            public int Multiply(int a, int b)
            {
                return a * b;
            }
        }

        [Test]
        public void Sub()
        {
            var app = new App();
            var p = new Parser(app);
            p.Parse(new string[] { "Math", "Add", "1", "1", "Multiply", "10", "10"});
        }

        [Test]
        public void HelpMessage()
        {
            var app = new App();
            var p = new Parser(app);
            var w = new StringWriter();
            p.WriteUsage(w);
            log.Info(w.ToString());
            Assert.IsTrue(w.ToString().Contains("Math"));
        }

        [Test]
        public void HelpMessage2()
        {
            var app = new App();
            var p = new Parser(app);
            var w = new StringWriter();
            var consoleOut = Console.Out;
            Console.SetOut(w);
            p.Parse(new string[] { "Math" });
            Console.SetOut(consoleOut);
            log.Info(w.ToString());
            Assert.IsTrue(w.ToString().Contains("adds a and b"));
        }

        [Test]
        public void MultipleSubcommands()
        {
            var app = new App();
            var p = new Parser(app);
            p.Parse(new string[] { "Math", "Add", "1", "1", ";", "Math", "Multiply", "2", "2" });
            p.Parse(new string[] { "Math", "Add", "1", "1", "Multiply", "2", "2" });
        }

        [Test]
        public void Persistence()
        {
            var a0 = new App();
            Parser.Run(a0, new[] { "Settings", "Name", "Andreas", ";", 
                "NonPersistentSettings", "Name", "B", ";",
                "GlobalSettings", "Name", "GlobalName" });
            Assert.AreEqual("Andreas", a0.Settings.Name);
            Assert.AreEqual("B", a0.NonPersistentSettings.Name);

            var a1 = new App();

            var parser = new Parser(a1);
            var settingsSubCommand = parser.SubCommands.ToList()[1];
            Assert.AreEqual("Settings", settingsSubCommand.Name);
            Assert.IsTrue(settingsSubCommand.IsPersistent);

            parser.LoadPreferences();
            parser.Parse(new string[] { "Settings", "Name", "Andreas", ";", "NonPersistentSettings", "help" });
            Assert.AreEqual("Andreas", a1.Settings.Name);
            Assert.IsNull(a1.NonPersistentSettings.Name);

            var another = new AnotherAppThatUsesGlobalSettings();
            Parser.Run(another, new string[] { "GlobalSettings" });
            Assert.AreEqual("GlobalName", another.GlobalSettings.Name);
        }
    }
}
