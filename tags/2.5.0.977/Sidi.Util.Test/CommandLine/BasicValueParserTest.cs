using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Extensions;
using Sidi.Test;
using System.Net;
using Sidi.Util;
using Sidi.IO;

namespace Sidi.CommandLine
{
    [TestFixture]
    public class BasicValueParserTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void ValueParserSuitable()
        {
            var mi = typeof(BasicValueParsers).GetMethod("ParseBool");
            Assert.IsTrue(StaticMethodValueParser.IsSuitable(mi));
        }

        [Test, RequiresSTA]
        public void ParseExamples()
        {
            foreach (var vp in new Parser().AvailableValueParsers)
            {
                log.Info(vp.UsageText);

                foreach (var example in vp.Examples.Where(x => !x.NoTest))
                {
                    log.InfoFormat("Parsing {0}", example.Value.Quote());
                    var args = Tokenizer.ToList(example.Value);
                    var r = vp.Handle(args, true);
                    Assert.IsTrue(args.Count == 0);
                    log.InfoFormat("Parsing {0} returns {1}", example.Value.Quote(), r);
                }
            }
        }

        void TestTimeInterval(string text)
        {
            var p = new Parser();
            var args = Tokenizer.ToList(text);
            var ti = p.ParseValue<TimeInterval>(args);
            log.InfoFormat("{0} parses to {1}", text, ti);
        }

        [Test]
        public void TestDateParsing()
        {
            var p = new Parser();
            p.ParseValue<DateTime>("tomorrow");
        }

        [Test]
        public void TimeIntervalParsing()
        {
            TestTimeInterval("[18.04.2013 03:35:04, 18.04.2013 15:35:04[");
            TestTimeInterval("year 2013-01-01");
            TestTimeInterval("year today");
            TestTimeInterval("FinancialYear today");
            TestTimeInterval("last 90 days");
            TestTimeInterval("next 12 hours");
            TestTimeInterval("(begin 2013-03-01 end 2013-04-10)");
            TestTimeInterval("[yesterday, Tomorrow[");
            TestTimeInterval("(begin yesterday end tomorrow)");
            TestTimeInterval("(last 30 days end tomorrow)");
        }

        [Test]
        public void PathList()
        {
            var p = new Parser();
            log.Info(p.ParseValue<PathList>(@"C:\temp\hello.txt"));
            try
            {
                log.Info(p.ParseValue<PathList>(@":current"));
                log.Info(p.ParseValue<PathList>(":selected"));
                log.Info(p.ParseValue<PathList>(@":paste"));
            }
            catch (CommandLineException)
            {
            }
        }

        [Test]
        public void Lists()
        {
            var p = new Parser();
            var args = new[] { "[", "A", "B", "C", "]" }.ToList();
            var list = (string[])  p.ParseValue(args, typeof(string[]));
            Assert.AreEqual(0, args.Count);
            Assert.AreEqual(3, list.Length);
        }

        public class SampleApp
        {
            [Usage("To demonstrate that IPAddress.Parse is used.")]
            public IPAddress Address;

            [Usage("Time interval value")]
            public TimeInterval Time;

            [Usage("Path list")]
            public PathList Paths;
        }
            
        [Test]
        public void StaticParse()
        {
            var a = new SampleApp();
            var ipString = "1.2.3.4";
            Parser.Run(a, new[]{"Address", ipString});
            Assert.AreEqual(new IPAddress(new byte[]{1,2,3,4}), a.Address);
        }

        [Test]
        public void ParseTimeInterval()
        {
            var a = new SampleApp();
            var ipString = "1.2.3.4";
            Parser.Run(a, new[] { "Time", "(", "begin", "2013-05-01", "end", "2013-05-02", ")", "Address", ipString });
            Assert.AreEqual(new TimeInterval(new DateTime(2013, 5, 1), new DateTime(2013, 5, 2)), a.Time);
        }

        [Test]
        public void Paths()
        {
            var a = new SampleApp();
            Parser.Run(a, new []{"Paths", ":current"});
        }
    }
}
