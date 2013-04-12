using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Extensions;

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
            Assert.IsTrue(ValueParser.IsSuitable(mi));
        }

        [Test]
        public void ParseExamples()
        {
            foreach (var vp in new Parser().ValueParsers)
            {
                log.Info(vp.UsageText);
                foreach (var example in vp.Examples)
                {
                    var r = vp.Handle(new List<string>() { example.Value }, true);
                    log.InfoFormat("Parsing {0} returns {1}", example.Value.Quote(), r);
                }
            }
        }
    }
}
