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
using System.ComponentModel;
using System.IO;
using Sidi.Util;

namespace Sidi.CommandLine
{
    [TestFixture]
    public class ParserTest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ParserTest()
        {
            log4net.Config.BasicConfigurator.Configure();
        }

        [SetUp()]
        public void SetUp()
        {
        }

        [TearDown()]
        public void TearDown()
        {
        }

        public enum Fruit
        {
            Apple,
            Orange,
            Pear,
        };

        [Usage("App usage")]
        class TestApp
        {
            [Usage("sample action")]
            public void SayHello()
            {
                Result = "Hello, " + Name;
                Console.WriteLine(Result);
            }

            [Usage("Determines to whom to say hello.")]
            public string Name
            {
                get { return m_name; }
                set { m_name = value; }
            }
            public string m_name = "John Doe";

            [Usage("Determines how often to say hello.")]
            public int Times
            {
                get { return m_Times; }
                set { m_Times = value; }
            }
            public int m_Times = 1;


            [Usage("Determines when to say hello.")]
            public DateTime Time
            {
                get { return m_Time; }
                set { m_Time = value; }
            }

            public DateTime m_Time;

            [Usage("Demonstrates enums")]
            public Fruit Fruit { set; get; }

            public string Result;
        }

        [Usage("Minimal app")]
        public class MinimalTestApp
        {
        }

        [Test]
        public void Enum()
        {
            TestApp t = new TestApp();
            Parser.Run(t, new string[] { "--Fruit", "Apple" });
            Assert.AreEqual(t.Fruit, Fruit.Apple);

            Parser.Run(t, new string[] { "--Fruit", "Orange" });
            Assert.AreEqual(t.Fruit, Fruit.Orange);

            var usage = StringEx.ToString(new Parser(t).WriteUsage);
            Assert.IsTrue(usage.Contains(Fruit.Apple.ToString()));
        }

        [Test]
        public void ShowUsage()
        {
            Parser p = new Parser(new TestApp());
            var usage = StringEx.ToString(x => p.WriteUsage(x));
            Assert.IsTrue(usage.Contains("Options"));
            Assert.IsTrue(usage.Contains("Actions"));
        }

        [Test]
        public void ShowUsage2()
        {
            Parser p = new Parser(new MinimalTestApp());
            var usage = StringEx.ToString(x => p.WriteUsage(x));
            Assert.IsFalse(usage.Contains("Options"));
            Assert.IsFalse(usage.Contains("Actions"));
        }

        [Test]
        public void Info()
        {
            TestApp app = new TestApp();
            Parser p = new Parser(app);
            string i = p.Info;
            Console.WriteLine(i);
            Assert.IsTrue(i.Contains(app.GetType().Name));
        }

        [Test()]
        public void TestAssertion()
        {
            TestApp app = new TestApp();

            Sidi.CommandLine.Parser.Run(app, new string[] { "SayHello" });
            Assert.AreEqual("Hello, John Doe", app.Result);

            Sidi.CommandLine.Parser.Run(app, new string[] { "--Name", "Andreas", "SayHello" });
            Assert.AreEqual("Hello, Andreas", app.Result);

            Sidi.CommandLine.Parser.Run(app, new string[] { "--Times", "10", "--Name", "Andreas", "SayHello" });
            Assert.AreEqual("Hello, Andreas", app.Result);

            Sidi.CommandLine.Parser.Run(app, new string[] { "--Name", "Andreas", "SayHello", "--Name", "Ernie", "SayHello" });
            Assert.AreEqual("Hello, Ernie", app.Result);
        }

        [Test()]
        public void TestGui()
        {
            TestApp app = new TestApp();
            Sidi.CommandLine.Parser.Run(app, new string[] { });
        }

        [Test]
        public void FuzzySearch()
        {
            TestApp app = new TestApp();
            Sidi.CommandLine.Parser.Run(app, new string[] { "--n", "Andreas", "SayHello" });
            Assert.AreEqual("Hello, Andreas", app.Result);

            app = new TestApp();
            Sidi.CommandLine.Parser.Run(app, new string[] { "--n", "Andreas", "sh" });
            Assert.AreEqual("Hello, Andreas", app.Result);

            app = new TestApp(); 
            Sidi.CommandLine.Parser.Run(app, new string[] { "--n", "Andreas", "say" });
            Assert.AreEqual("Hello, Andreas", app.Result);

            app = new TestApp(); 
            Sidi.CommandLine.Parser.Run(app, new string[] { "-na", "Andreas", "say" });
            Assert.AreEqual("Hello, Andreas", app.Result);
        }

        [Test]
        public void DateTimeArg()
        {
            TestApp app = new TestApp();

            Sidi.CommandLine.Parser.Run(app, new string[] { "--time", "2008-05-11 11:12:00" });
            Assert.IsTrue(app.Time == new DateTime(2008, 5, 11, 11, 12, 0));
        }

        [Test, ExpectedException(ExceptionType = typeof(CommandLineException))]
        public void NotUnique()
        {
            TestApp app = new TestApp();
            Parser parser = new Parser(app);
            // --t is not unique, since it could mean --Time or --Times
            parser.Parse(new string[] { "--t", "2008-05-11 11:12:00" });
        }

        [System.ComponentModel.Description("Test app that uses the Description attribute")]
        class TestAppWithDescription
        {
            [System.ComponentModel.Description("some action")]
            public void SomeAction()
            {
            }

            [System.ComponentModel.Description("some option - marker_for_test_aoifq7ft48q")]
            public string SomeOption { get; set; }
        }

        [Test]
        public void TestDescription()
        {
            Parser parser = new Parser(new TestAppWithDescription());
            parser.Parse(new string[] { "--SomeOption", "hello", "SomeAction" });

            StringWriter w = new StringWriter();
            parser.PrintSampleScript(w);
            Assert.IsTrue(w.ToString().Contains("marker_for_test_aoifq7ft48q"));
        }

        [Usage("")]
        public class TestAppWithStringList
        {
            [Usage("process all following arguments")]
            public void Action(List<string> args)
            {
                args.Clear();
            }

            public void Add(int x, int y)
            {
            }

            [Usage("adds")]
            public void Add(double x, double y)
            {
            }

            [Usage("adds")]
            public void SubtractInt(int x, int y)
            {
            }

            [Usage("adds")]
            public void SubtractDouble(int x, int y)
            {
            }
        }
        
        [Test]
        public void ArgList()
        {
            Parser parser = new Parser(new TestAppWithStringList());
            parser.Parse(new string[] { "Action", "arg1", "arg2", "arg3" });
        }

        [Test]
        public void Ambiguous()
        {
            Parser parser = new Parser(new TestAppWithStringList());
            parser.Parse(new string[] { "Add", "1.0", "1.0" });
        }

        [Test]
        public void Ambiguous2()
        {
            Parser.Run(new TestAppWithStringList(), new string[] { "Subtract", "1", "1" });
        }

        [Test]
        public void NotEnoughParameters()
        {
            Parser.Run(new TestAppWithStringList(), new string[] { "Add", "1" });
        }

        [Usage("Multiline test")]
        public class TestMultiLineUsage
        {
            [Usage("multiline\r\nline 1\r\nline2\r\nline3")]
            public void A()
            {
            }

            [Usage("very long line that should be wrapped. very long line that should be wrapped. very long line that should be wrapped. very long line that should be wrapped. very long line that should be wrapped. very long line that should be wrapped. very long line that should be wrapped. very long line that should be wrapped. very long line that should be wrapped. very long line that should be wrapped. very long line that should be wrapped. very long line that should be wrapped. ")]
            public void B()
            {
            }
        }

        [Test]
        public void ShowMultiLineUsage()
        {
            string u = StringEx.ToString(new Parser(new TestMultiLineUsage()).WriteUsage);
            Console.WriteLine(u);
        }

        [Test]
        public void IsMatch()
        {
            Parser p = new Parser(new TestMultiLineUsage());
            Assert.IsTrue(p.IsMatch("rfl", "RunFromLocal"));
            Assert.IsFalse(p.IsMatch("rfl", "RunFromlocal"));
            Assert.IsTrue(p.IsMatch("runfr", "RunFromLocal"));
            Assert.IsTrue(p.IsMatch("runfromlocal", "RunFromLocal"));
            Assert.IsTrue(p.IsMatch("rufro", "RunFromLocal"));
            Assert.IsTrue(p.IsMatch("rfrlocal", "RunFromLocal"));
            Assert.IsFalse(p.IsMatch("rfrlocale", "RunFromLocal"));
        }
    }
}
