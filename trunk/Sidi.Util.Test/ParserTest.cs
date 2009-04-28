// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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

namespace Sidi.Util.Test
{
    [TestFixture]
    public class ParserTest
    {
        #region "Custom Trace Listener"
        MyListener listener = new MyListener();

        internal class MyListener : TraceListener
        {
            public override void Write(string message)
            {
                Console.Write(message);
            }


            public override void WriteLine(string message)
            {
                Console.WriteLine(message);
            }
        }
        #endregion


        [SetUp()]
        public void SetUp()
        {
            //Setup our custom trace listener
            if (!Trace.Listeners.Contains(listener))
            {
                Trace.Listeners.Add(listener);
            }

            //TODO - Setup your test objects here
        }

        [TearDown()]
        public void TearDown()
        {
            //Remove our custom trace listener
            if (Trace.Listeners.Contains(listener))
            {
                Trace.Listeners.Remove(listener);
            }

            //TODO - Tidy up your test objects here
        }

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

            public string Result;
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

            Sidi.CommandLine.Parser.Run(app, new string[] { "--n", "Andreas", "say" });
            Assert.AreEqual("Hello, Andreas", app.Result);

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
        }
        
        [Test]
        public void ArgList()
        {
            Parser parser = new Parser(new TestAppWithStringList());
            parser.Parse(new string[] { "Action", "arg1", "arg2", "arg3" });
        }
    }
}