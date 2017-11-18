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

    [Usage("Module to test GetOpt")]
    public class HelloWorld : Sidi.CommandLine.IArgumentHandler
    {
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

        public void ProcessArguments(string[] args)
        {
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
        public void Options()
        {
            var m = new HelloWorld();
            var options = GetOpt.GetOptions(new object[] { m });
            options.ListFormat().AllPublic().RenderText();
            Assert.AreEqual(4, options.Count());
        }

        [Test]
        public void HandleLongOption()
        {
            var m = new HelloWorld();
            var options = GetOpt.GetOptions(new[] { m });

            var args = new GetOpt.Args(new[] { "--cordially", "--say-hello=Andreas" });
            Assert.IsTrue(GetOpt.HandleLongOption(args, options));
            Assert.AreEqual(1, args.i);

            Assert.IsTrue(GetOpt.HandleLongOption(args, options));
            Assert.AreEqual(2, args.i);
            Assert.AreEqual("Andreas", m.name);
        }

        [Test]
        public void HandleOption()
        {
            var m = new HelloWorld();
            var options = GetOpt.GetOptions(new object[] { m });

            var args = new GetOpt.Args(new[] { "-c", "-sAndreas" , "-w1.1.2018"});
            Assert.IsTrue(GetOpt.HandleOption(args, options));
            Assert.AreEqual(1, args.i);

            Assert.IsTrue(GetOpt.HandleOption(args, options));
            Assert.AreEqual(2, args.i);

            Assert.IsTrue(GetOpt.HandleOption(args, options));
            Assert.AreEqual(3, args.i);
        }
    }
}
