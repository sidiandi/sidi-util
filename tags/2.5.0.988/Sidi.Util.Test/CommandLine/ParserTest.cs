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

using Sidi.CommandLine;
using cm = System.ComponentModel;
using Sidi.Util;
using System.Linq;
using System.Threading;
using System.Net;
using Sidi.IO;
using Sidi.Extensions;
using System.IO;
using Sidi.Test;

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

            [Usage("Test interference with double value parser")]
            public void Double()
            {
            }

            [Usage("Say hello to")]
            public void Greet(string name, string greeting, int times)
            {
                for (int i = 0; i < times; ++i)
                {
                    Console.WriteLine("Hello, {0}", name);
                }               
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

            [Usage("Throws an exception")]
            public int ThrowException()
            {
                string a = null;
                return a.Length;
            }

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
        public void MainApplication()
        {
            var app = new TestApp();
            var p = new Parser(app);
            Assert.AreEqual(5, p.ItemSources.Count);
            Assert.AreEqual(app, p.MainSource.Instance);
        }

        [Test]
        public void Double()
        {
            var p = new Parser(new TestApp());
            p.Parse(new []{"Do" });
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

            var usage = StringExtensions.ToString(new Parser(t).WriteUsage);
            Assert.IsTrue(usage.Contains(Fruit.Apple.ToString()));
        }

        [Test]
        public void ShowUsage()
        {
            Parser p = new Parser(new TestApp());
            var usage = StringExtensions.ToString(x => p.WriteUsage(x));
            log.Info(usage);
            Assert.IsTrue(usage.Contains("SayHello"));

            Assert.IsTrue(p.Info.StartsWith("TestApp - App usage"));
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

            var w = new System.IO.StringWriter();
            parser.PrintSampleScript(w);
            log.Info(w.ToString());
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
                var e = new Sidi.IO.Find()
                {
                    Root = new Sidi.IO.LPath(dir)
                };

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
            p.ShowUsage();
            p.Parse(new string[]{ "AddList", "[", "1", "2", "3", "]"});
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
        public void ParseBraces()
        {
            var a = new TestAppWithStringList();
            var p = new Parser(a);
            var args = new List<string>() { "Add", "1", "1", "(Subtract", "3", "1", "Add", "1", "1)", "Add", "1", "1" };
            p.ParseSingleCommand(args);
            Assert.AreEqual("(Subtract", args.First());
            p.ParseBraces(args);
            Assert.AreEqual("Add", args.First());
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
                Assert.Fail();
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
            string u = StringExtensions.ToString(new Parser(new TestMultiLineUsage()).WriteUsage);
            Console.WriteLine(u);
        }

        [Test]
        public void IsMatch()
        {
            Assert.IsTrue(Parser.IsMatch("rfl", "RunFromLocal"));
            Assert.IsFalse(Parser.IsMatch("rfl", "RunFromlocal"));
            Assert.IsTrue(Parser.IsMatch("runfr", "RunFromLocal"));
            Assert.IsTrue(Parser.IsMatch("runfromlocal", "RunFromLocal"));
            Assert.IsTrue(Parser.IsMatch("rufro", "RunFromLocal"));
            Assert.IsTrue(Parser.IsMatch("rfrlocal", "RunFromLocal"));
            Assert.IsFalse(Parser.IsMatch("rfrlocale", "RunFromLocal"));
            Assert.IsTrue(Parser.IsMatch("red", "RemoveEmptyDirectories"));
            Assert.IsTrue(Parser.IsMatch("reed", "RemoveEmptyDirectories"));
            Assert.IsTrue(Parser.IsMatch("remd", "RemoveEmptyDirectories"));
            Assert.IsFalse(Parser.IsMatch("remod", "RemoveEmptyDirectories"));
            Assert.IsTrue(Parser.IsMatch("remoed", "RemoveEmptyDirectories"));
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

        [Usage("options")]
        public class SomeOptions
        {
            [Usage("user"), Persistent(ApplicationSpecific = true)]
            public string LocalOption;

            [Usage("global"), Persistent]
            public string GlobalOption;
        }

        [Usage("Tests preferences")]
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

            [SubCommand]
            public PreferencesTestApplication Test;

            [SubCommand]
            public SomeOptions Options = new SomeOptions();
        }

        [Usage("Tests preferences")]
        public class PreferencesTestApplication2
        {
            [SubCommand]
            public SomeOptions Options = new SomeOptions();
        }

        [Usage("Tests preferences")]
        public class PreferencesTestApplication3
        {
            [SubCommand, Persistent(ApplicationSpecific=true)]
            public SomeOptions Options = new SomeOptions();
        }

        [Test]
        public void Preferences()
        {
            var a = new PreferencesTestApplication();
            var p = new Parser(a);

            a.Name = "Donald";
            a.Password = "Secret";
            a.Options.LocalOption = System.IO.Path.GetRandomFileName();
            a.Options.GlobalOption = System.IO.Path.GetRandomFileName();

            p.StorePreferences();

            var b = new PreferencesTestApplication2();
            var pb = new Parser(b);
            pb.LoadPreferences();
            Assert.AreNotEqual(a.Options.LocalOption, b.Options.LocalOption);
            Assert.AreEqual(a.Options.GlobalOption, b.Options.GlobalOption);

            var c = new PreferencesTestApplication3();
            var pc = new Parser(c);
            pc.LoadPreferences();
            Assert.AreNotEqual(a.Options.LocalOption, c.Options.LocalOption);
            Assert.AreEqual(a.Options.GlobalOption, c.Options.GlobalOption);

            var d = new PreferencesTestApplication2();
            var pd = new Parser(c);
            pd.Profile = "new-profile";
            pd.LoadPreferences();
            Assert.AreNotEqual(a.Options.LocalOption, d.Options.LocalOption);
            Assert.AreNotEqual(a.Options.GlobalOption, d.Options.GlobalOption);
        }

        [Test]
        public void Preferences2()
        {
            Parser.Run(new PreferencesTestApplication(), new string[] { });
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
            p.Prefix[typeof(Option)] = new string[] { "--" };
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
        public void ParseValues()
        {
            var p = new Parser();
            var ln = (LPath) p.ParseValue(Tokenizer.ToList(@"C:\temp"), typeof(LPath));
            Assert.AreEqual(new LPath(@"C:\temp"), ln);
        }

        [Usage("Some sub command")]
        class MySubCommand
        {
            [Usage("Name"), Persistent]
            public string Name { get; set; }
        }

        [Usage("Tests persistence of options in subcommands")]
        class MyApp
        {
            [SubCommand]
            public MySubCommand Sub = null;
        }

        [Usage("Tests persistence of options in subcommands")]
        class MyOtherApp
        {
            [SubCommand]
            public MySubCommand Sub = null;
        }

        [Test]
        public void SubCommand()
        {
            var p = Parser.SingleSource(new MyApp());
            p.AddDefaultUserInterface();
            p.Parse(new[] { "Sub", "Name", "Donald" });
        }

        [Test]
        public void RunSubCommand()
        {
            Parser.Run(new MyApp(), new[] { "Sub", "Name", "Donald" });
        }

        [Test]
        public void PersistenceInSubcommands()
        {
            var name = System.IO.Path.GetRandomFileName();
            Parser.Run(new MyApp(), new string[]{"Sub", "Name", name});

            var a = new MyApp();
            var p = new Parser(a);
            p.LoadPreferences();
            p.Parse(new string[] { "Sub" });
            Assert.AreEqual(name, a.Sub.Name);

            var b = new MyOtherApp();
            var pb = new Parser(b);
            pb.LoadPreferences();
            pb.Parse(new string[] { "Sub" });
            Assert.AreEqual(name, b.Sub.Name);
        }

        class FieldTest
        {
            [Usage("test field 1")]
            public int a = 1;

            [Usage("test field 2")]
            public int b = 2;
        }

        [Test]
        public void Fields()
        {
            var ft = new FieldTest();
            Assert.AreEqual(1, ft.a);
            Parser.Run(ft, new string[] { "a", "2" });
            Assert.AreEqual(2, ft.a);
        }

        [Test]
        public void UnknownArgument()
        {
            var ft = new FieldTest();
            var p = new Parser(ft);
            var args = new List<string>(){ "unknown" };
            try
            {
                p.Parse(args);
            }
            catch (Sidi.CommandLine.CommandLineException)
            {
            }
            Assert.AreEqual(1, args.Count);
        }

        public class DefaultParameterTest
        {
            [Usage("Say Hello")]
            public void Hello(string name = "A")
            {
                this.Name = name;
            }

            public string Name { get; private set; }
        }

        [Test]
        public void DefaultParameters()
        {
            var t = new DefaultParameterTest();
            Parser.Run(t, new[] { "Hello", "B" });
            Assert.AreEqual("B", t.Name);

            Parser.Run(t, new[] { "Hello" });
            Assert.AreEqual("A", t.Name);
        }

        [Test]
        public void LogLevelIsNotGlobal()
        {
            Parser.Run(new MyApp(), new string[] { "LogLevel", "INFO" });
            Parser.Run(new TestApp(), new string[] { "LogLevel", "ALL" });
            
            var p = new Parser(new MyApp());
            p.Run(new string[] { });
            Assert.AreEqual("INFO", p.LogOptions.LogLevel.ToString());
        }

        [Test]
        public void LogOptions()
        {
            var p = new Parser(new MyApp());
            p.LogOptions.LogFile = true;
            Assert.AreEqual(true, p.LogOptions.LogFile);
            p.LogOptions.LogFile = false;
            Assert.AreEqual(false, p.LogOptions.LogFile);
        }

        [Usage("Tests handling of unknown commands")]
        public class CommandLineHandlerTestApp : CommandLineHandler
        {
            public int beforeParseCalls = 0;

            public void BeforeParse(IList<string> args)
            {
                beforeParseCalls++;
            }

            public void UnknownArgument(IList<string> args)
            {
                unknownArgument = args.PopHead();
            }

            public string unknownArgument;

            [Usage("Command")]
            public void DoSomething()
            {
            }
        }

        [Test]
        public void CommandLineHandler()
        {
            var a = new CommandLineHandlerTestApp();
            var p = new Parser(a);
            p.Run(new[]{"Hello", "DoSomething"});
            Assert.AreEqual(2, a.beforeParseCalls);
            Assert.AreEqual("Hello", a.unknownArgument);
        }
    }
}
