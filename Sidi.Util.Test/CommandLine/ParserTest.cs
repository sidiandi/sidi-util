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
using Sidi.Util;
using System.Linq;
using System.Threading;
using System.Net;
using Sidi.IO.Long;

namespace Sidi.CommandLine.Test
{
    [TestFixture]
    public class ParserTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        public class TestApp
        {
            [Usage("sample action")]
            public void SayHello()
            {
                Result = "Hello, " + Name;
                for (int i = 0; i < Times; ++i)
                {
                    Console.WriteLine(Result);
                }
            }

            [Usage("Say hello to")]
            public void Greet(string name, string greeting, int times)
            {
                Console.WriteLine("Hello, {0}", name);
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
            [cm.Category("Advanced")]
            public Fruit Fruit { set; get; }

            public string Result;
        }

        [Usage("Minimal app")]
        public class MinimalTestApp
        {
        }

        public static Parser ParserWithAllTestApps()
        {
            return new Parser(
                new TestApp(),
                new TestMultiLineUsage(),
                new MinimalTestApp(),
                new TestAppWithDescription(),
                new TestAppWithStringList());
        }

        [Test]
        public void Enum()
        {
            TestApp t = new TestApp();
            Parser.Run(t, new string[] { "Fruit", "Apple" });
            Assert.AreEqual(t.Fruit, Fruit.Apple);

            Parser.Run(t, new string[] { "Fruit", "Orange" });
            Assert.AreEqual(t.Fruit, Fruit.Orange);

            Parser.Run(t, new string[] { "--Fruit", "Pear" });
            Assert.AreEqual(t.Fruit, Fruit.Pear);

            var usage = StringEx.ToString(new Parser(t).WriteUsage);
            Assert.IsTrue(usage.Contains(Fruit.Apple.ToString()));
        }

        [Test]
        public void ShowUsage()
        {
            Parser p = new Parser(new TestApp());
            var usage = StringEx.ToString(x => p.WriteUsage(x));
            log.Info(usage);
            Assert.IsTrue(usage.Contains("Options"));
            Assert.IsTrue(usage.Contains("Actions"));
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

        [Test]
        public void Help()
        {
            TestApp app = new TestApp();
            Parser.Run(app, new string[] { "help", "SayHello" });
        }

        [Test()]
        public void TestAssertion()
        {
            TestApp app = new TestApp();

            Sidi.CommandLine.Parser.Run(app, new string[] { "SayHello" });
            Assert.AreEqual("Hello, John Doe", app.Result);

            Sidi.CommandLine.Parser.Run(app, new string[] { "--Name", "Bert", "SayHello" });
            Assert.AreEqual(helloBert, app.Result);

            Sidi.CommandLine.Parser.Run(app, new string[] { "--Times", "10", "--Name", "Bert", "SayHello" });
            Assert.AreEqual(helloBert, app.Result);

            Sidi.CommandLine.Parser.Run(app, new string[] { "--Name", "Bert", "SayHello", "--Name", "Ernie", "SayHello" });
            Assert.AreEqual("Hello, Ernie", app.Result);
        }

        string helloBert = "Hello, Bert";

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
            Sidi.CommandLine.Parser.Run(app, new string[] { "--n", "Bert", "SayHello" });
            Assert.AreEqual(helloBert, app.Result);

            app = new TestApp(); 
            Sidi.CommandLine.Parser.Run(app, new string[] { "--n", "Bert", "say" });
            Assert.AreEqual(helloBert, app.Result);

            app = new TestApp(); 
            Sidi.CommandLine.Parser.Run(app, new string[] { "-na", "Bert", "say" });
            Assert.AreEqual(helloBert, app.Result);
        }

        [Test]
        public void DateTimeArg()
        {
            TestApp app = new TestApp();

            Sidi.CommandLine.Parser.Run(app, new string[] { "--time", "2008-05-11 11:12:00" });
            Assert.IsTrue(app.Time == new DateTime(2008, 5, 11, 11, 12, 0));
        }

        [Test, ExpectedException(ExpectedException = typeof(CommandLineException))]
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

            var w = new System.IO.StringWriter();
            parser.PrintSampleScript(w);
            Assert.IsTrue(w.ToString().Contains("marker_for_test_aoifq7ft48q"));
        }

        [Usage("")]
        public class TestAppWithStringList
        {
            [Usage("process all following arguments")]
            public void Action(params string[] args)
            {
                foreach (var a in args)
                {
                    Console.WriteLine("argument: {0}", a);
                }
            }

            public int Add(int x, int y)
            {
                return x + y;
            }

            [Usage("adds a list of numbers")]
            public double AddList(params double[] list)
            {
                var result = list.Aggregate(0.0, (x, y) => x + y);
                return result;
            }

            [Usage("adds")]
            public double Add(double x, double y)
            {
                return x + y;
            }

            [Usage("adds")]
            public int SubtractInt(int x, int y)
            {
                return x - y;
            }

            [Usage("subtracts")]
            public double Subtract(double x, double y)
            {
                return x - y;
            }

            [Usage("Writes lots of text to Console.Out")]
            public void MuchText(int lines)
            {
                for (int i = 0; i < lines; ++i)
                {
                    Console.WriteLine("This is line {0} of a long text", i);
                    Thread.Sleep(100);
                }
            }

            [Usage("Directory list")]
            public void Files(string dir)
            {
                var e = new Sidi.IO.Long.FileEnum();
                e.AddRoot(new Sidi.IO.Long.Path(dir));
                foreach (var f in e.Depth())
                {
                    Console.WriteLine(f.FullName.NoPrefix);
                }
            }

            [SubCommand]
            public TestApp Test { set; get; }
        }

        [Test]
        public void List()
        {
            var a = new TestAppWithStringList();
            var p = new Parser(a);
            p.Parse(new string[]{ "AddList", "1", "2", "3"});
        }

        [Test]
        public void List2()
        {
            var a = new TestAppWithStringList();
            var p = new Parser(a);
            p.Parse(new string[] { "AddList" });
        }

        [Test]
        public void MultiCommand()
        {
            var a = new TestAppWithStringList();
            var p = new Parser(a);
            p.Parse(new string[] { "Add", "1", "1", "Subtract", "3", "1" });
        }

        [Test]
        public void WrongParameterCountMessage()
        {
            var p = new Parser(new TestAppWithStringList());
            try
            {
                p.Parse(new string[] { "Add", "1" });
            }
            catch (CommandLineException e)
            {
                log.Info(e.Message);
                Assert.IsTrue(e.Message.Contains(p.GetAction("Add").Usage));
            }
        }

        [Test]
        public void WrongParameterType()
        {
            var p = new Parser(new TestAppWithStringList());
            try
            {
                p.Parse(new string[] { "Add", "1", "car"});
            }
            catch (CommandLineException e)
            {
                log.Info(e.Message);
                Assert.IsTrue(e.Message.Contains(p.GetAction("Add").Usage));
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
            public void A(int a, int b, int c)
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
            Assert.IsTrue(p.IsMatch("red", "RemoveEmptyDirectories"));
            Assert.IsTrue(p.IsMatch("reed", "RemoveEmptyDirectories"));
            Assert.IsTrue(p.IsMatch("remd", "RemoveEmptyDirectories"));
            Assert.IsFalse(p.IsMatch("remod", "RemoveEmptyDirectories"));
            Assert.IsTrue(p.IsMatch("remoed", "RemoveEmptyDirectories"));
        }

        [Test]
        public void IteratorSemantics()
        {
            var s = "sidi";
            var a = s.GetEnumerator();
            a.MoveNext();
            var b = (CharEnumerator)a.Clone();
            a.MoveNext();
            Assert.AreNotEqual(a.Current, b.Current);
        }

        public class PreferencesTestApplication
        {
            [Usage("Your name")]
            [Persistent]
            public string Name { set; get; }

            [Usage("secret password")]
            [Password]
            [Persistent]
            public string Password { set; get; }

            [Usage("other option, is null by default")]
            [Persistent]
            public string AnotherOption { set; get; }
        }

        [Test]
        public void Preferences()
        {
            var a = new PreferencesTestApplication();

            var p = new Parser(a);

            log.Info(p.PreferencesKey);

            a.Name = "Donald";
            a.Password = "Secret";

            p.StorePreferences();

            var b = new PreferencesTestApplication();

            var pb = new Parser(b);

            var k = pb.GetPreferencesKey(pb.Options.First());
            log.Info(k);

            pb.LoadPreferences();
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Password, b.Password);
        }

        [Test, Explicit("interactive")]
        public void PasswordUi()
        {
            Parser.Run(new PreferencesTestApplication(), new string[] { "ui" });
        }

        [Test]
        public void HidePassword()
        {
            var a = new PreferencesTestApplication();
            var p = "24985624856";
            a.Password = p;
            var w = new System.IO.StringWriter();
            new Parser(a).WriteUsage(w);
            Assert.IsFalse(w.ToString().Contains(a.Password), w.ToString());
        }

        [Usage("Ambiguous option/action")]
        public class AmbiguousOption
        {
            public string arg;

            [Usage("run action")]
            public void Run(string arg)
            {
                this.arg = arg;
            }

            [Usage("run mode option")]
            public int RunMode { get; set; }
        }

        [Test]
        public void AmbiguousOptionAction()
        {
            var a = new AmbiguousOption();
            var arg = "hello";

            var p = new Parser(a);

            p.Parse(new string[] { "run", arg });
            Assert.AreEqual(arg, a.arg);
        }

        [Test, ExpectedException(typeof(CommandLineException))]
        public void Ambiguous3()
        {
            var a = new AmbiguousOption();
            var p = new Parser(a);
            p.Parse(new string[] { "ru" });
        }

        [Test]
        public void WithOptionPrefix()
        {
            var a = new AmbiguousOption();
            var p = new Parser(a);
            p.Parse(new string[] { "--ru", "1"});
        }

        [Test, Explicit("interactive")]
        public void Serve()
        {
            var p = new Parser(new TestAppWithStringList());
            p.Parse(new string[] { "WebServer", "Run", "Browse" });
        }

        [Test, Explicit("interactive")]
        public void Serve2()
        {
            var p = new Parser(new TestAppWithStringList());
            var ws = new WebServer(p);
            ws.StartServer();
            try
            {
                var wc = new WebClient();
                string result;

                result = wc.DownloadString(ws.Prefix);
                Assert.IsTrue(result.Contains("Test"));
                log.Info(result);

                result = wc.DownloadString(ws.Prefix + "Add?x=122&y=1");
                Assert.IsTrue(result.Contains("123"));

                result = wc.DownloadString(ws.Prefix + "Test/SayHello");
                Assert.IsTrue(result.Contains("Doe"), result);
                log.Info(result);
            }
            finally
            {
                ws.StopServer();
            }
        }

        [Test, Explicit("interactive")]
        public void ExternalWebServer()
        {
            var p = new Parser(new TestAppWithStringList());
            var ws = new WebServer(p);

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(ws.Prefix);

            listener.Start();
            var serverThread = new Thread(new ThreadStart(() =>
                {
                    var c = listener.GetContext();
                    ws.Handle(c);
                    Thread.Sleep(100);
                    listener.Stop();
                }));
            serverThread.Start();

            try
            {
                var wc = new WebClient();
                string result;

                result = wc.DownloadString(ws.Prefix + "Add?x=122&y=1");
                Assert.IsTrue(result.Contains("123"));
            }
            finally
            {
                serverThread.Join();
            }
        }

        [Test]
        public void SubParser()
        {
            var a = new TestAppWithStringList();
            var p = new Parser(a);
            p.Parse(new string[] { "Action", "1", "2", "3", ";", "AddList", "1", "2", "3", ";" });
            p.AddSubParser(new Parser(a));
            p.Parse(new string[] { "TestAppWithStringList", "Action", "1", "2", "3", ";", ";", "AddList", "1", "2", "3", ";" });
            p.WriteUsage(Console.Out);
        }

        [Test]
        public void ParseValues()
        {
            var ln = (Path) Parser.ParseValue(@"C:\temp", typeof(Path));
            Assert.AreEqual(new Path(@"C:\temp"), ln);
        }

        class MySubCommand
        {
            [Usage("Name"), Persistent]
            public string Name { get; set; }
        }

        [Usage("Tests persistence of options in subcommands")]
        class MyApp
        {
            [SubCommand]
            public MySubCommand Sub;
        }

        [Test]
        public void PersistenceInSubcommands()
        {
            var name = "Donald";
            Parser.Run(new MyApp(), new string[]{"Sub", "Name", name});

            var a = new MyApp();
            Parser.Run(a, new string[] {});
            Assert.AreEqual(name, a.Sub.Name);
        }
    }
}
