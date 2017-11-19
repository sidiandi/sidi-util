using NUnit.Framework;
using Sidi.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sidi.CommandLine.Test
{
    [Usage("Calculate")]
    public class Calculator
    {
        [Usage("Add two numbers and print result")]
        public void Add(double a, double b)
        {
            AddResult = a + b;
            Console.WriteLine(AddResult);
        }

        public double AddResult;
    }

    [Usage("Module to test GetOpt. Greets names")]
    public class HelloWorld : Sidi.CommandLine.IArgumentHandler
    {
        [Usage("option")]
        public bool f { get; set; }

        [Usage("option")]
        public bool e { get; set; }

        [Usage("option")]
        public bool d { get; set; }

        [Usage("Add two numbers and print result")]
        public void Add(double a, double b)
        {
            AddResult = a + b;
            Console.WriteLine(AddResult);
        }

        public double AddResult;

        [Usage(@"Say hello to someone.

Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.
")]
        public void SayHello(string name)
        {
            this.name = name;
        }

        [SubCommand]
        Calculator Calculator;

        public string name;

        [Usage("be cordial")]
        public bool Cordially { get; set; }

        [Usage("When to say hello")]
        public DateTime When { get; set; }
        
        [Usage("output path")]
        public string Output { get; set; }

        public string[] args;

        [Usage("[name]...")]
        public void ProcessArguments(string[] args)
        {
            this.args = args;

            foreach (var i in args)
            {
                if (Cordially)
                {
                    Console.WriteLine("Hello, dear {0}", i);
                }
                else
                {
                    Console.WriteLine("Hello, {0}", i);
                }
            }
        }
    }


    [TestFixture]
    class GetOptTest : Sidi.Test.TestBase
    {
        [Test]
        public void NoArgs()
        {
            GetOpt.Run(new HelloWorld(), new string[] { });
        }

        [Test, Explicit("will terminate test process")]
        public void Help()
        {
            GetOpt.Run(new HelloWorld(), new string[] { "-h" });
            GetOpt.Run(new HelloWorld(), new string[] { "--help" });
        }

        [Test, Explicit("will terminate test process")]
        public void Run()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var program = new HelloWorld();
            Assert.AreEqual(0, GetOpt.Run(program, new string[] { "-vvvvvvvvv", "-c", "Anna", "Lena", "--add", "1.0", "99.0" }));
            Assert.AreEqual(100.0, program.AddResult);
        }

        [Test]
        public void HelpText()
        {
            var go = new GetOpt();
            go.modules.Add(new HelloWorld());
            new Sidi.CommandLine.GetOptInternal.ShowHelp(go).PrintHelp(Console.Out);
        }

        [Test]
        public void VersionText()
        {
            var go = new GetOpt();
            go.modules.Add(new HelloWorld());
            new Sidi.CommandLine.GetOptInternal.ShowHelp(go).PrintVersion(Console.Out);
        }

        [Test, Explicit("will terminate test process")]
        public void Version()
        {
            GetOpt.Run(new HelloWorld(), new string[] { "-v" });
        }

        [Test]
        public void HandleLongOption()
        {
            var m = new HelloWorld();
            var options = GetOpt.GetOptions(new[] { m });

            var args = new GetOpt.Args(new[] { "--cordially", "--say-hello=Andreas" });
            Assert.IsTrue(GetOpt.HandleLongOption(args, options, longOptionPrefix));
            Assert.AreEqual(0, args.i);

            Assert.IsTrue(GetOpt.HandleLongOption(args, options, longOptionPrefix));
            Assert.AreEqual(1, args.i);
            Assert.AreEqual("Andreas", m.name);
        }

        const string shortOptionPrefix = "-";
        const string longOptionPrefix = "--";

        [Test]
        public void HandleOption()
        {
            var m = new HelloWorld();
            var options = GetOpt.GetOptions(new object[] { m });

            var args = new GetOpt.Args(new[] { "-c", "-sAndreas" , "-w1.1.2018"});
            Assert.IsTrue(GetOpt.HandleOption(args, options, shortOptionPrefix));
            Assert.AreEqual(0, args.i);

            Assert.IsTrue(GetOpt.HandleOption(args, options, shortOptionPrefix));
            Assert.AreEqual(1, args.i);

            Assert.IsTrue(GetOpt.HandleOption(args, options, shortOptionPrefix));
            Assert.AreEqual(2, args.i);
        }

        [Test]
        public void HandleOption2()
        {
            var m = new HelloWorld();
            GetOpt.Run(m, new[] { "-v", "Bert" });
            Assert.IsTrue(m.args.SequenceEqual(new[] { "Bert" }));
        }

        [Test]
        public void HandleOption3()
        {
            var m = new HelloWorld();
            GetOpt.Run(m, new[] { "-def", "Bert" });
            Assert.IsTrue(m.d & m.e & m.f);
            Assert.IsTrue(m.args.SequenceEqual(new[] { "Bert" }));
        }

        /// <summary>
        /// The argument ‘--’ terminates all options; any following arguments are treated as non-option arguments, even if they begin with a hyphen. 
        /// </summary>
        [Test]
        public void TwoDashes()
        {
            var m = new HelloWorld();
            GetOpt.Run(m, new[] { "--cordially", "-v", "Bert", "--", "--cordially" });
            Assert.IsTrue(m.args.SequenceEqual(new[] { "Bert", "--cordially" }), m.args.Join());
        }

        /// An option and its argument may or may not appear as separate tokens. (In other words, the whitespace separating them is optional.) Thus, ‘-o foo’ and ‘-ofoo’ are equivalent. 
        [Test]
        public void OptionAndArgumentAsSeparateTokens()
        {
            var m1 = new HelloWorld();
            GetOpt.Run(m1, new[] { "-o", "foo" });
            
            var m2 = new HelloWorld();
            GetOpt.Run(m2, new[] { "-ofoo" });

            Assert.AreEqual("foo", m1.Output);
            Assert.AreEqual("foo", m2.Output);
        }


        /// <summary>
        /// A token consisting of a single hyphen character is interpreted as an ordinary non-option argument. By convention, it is used to specify input from or output to the standard input and output streams. 
        /// </summary>
        [Test]
        public void OneDash()
        {
            var m = new HelloWorld();
            GetOpt.Run(m, new[] { "--cordially", "-" });
            Assert.IsTrue(m.args.SequenceEqual(new[] { "-" }), m.args.Join());
        }

        /// <summary>
        ///  Users can abbreviate the option names as long as the abbreviations are unique. 
        /// </summary>
        [Test]
        public void LongOptions()
        {
            var name = "Donald";
            foreach (var abbreviation in new[] { "say-hello", "s", "sh", "say", "so" })
            {
                var m = new HelloWorld();
                GetOpt.Run(m, new[] { "--" + abbreviation, name });
                Assert.AreEqual(name, m.name, abbreviation);
            }
        }

        [Test]
        public void IsMatch()
        {
            Assert.IsTrue (GetOpt.IsAbbreviationFor("shello", "say-hello"));
            Assert.IsTrue(GetOpt.IsAbbreviationFor("so", "say-hello"));
            Assert.IsTrue(GetOpt.IsAbbreviationFor("sh", "say-hello"));
            Assert.IsTrue(GetOpt.IsAbbreviationFor("s", "say-hello"));
            Assert.IsFalse(GetOpt.IsAbbreviationFor("o", "say-hello"));
        }
    }
}
