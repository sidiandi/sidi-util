﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Extensions;
using Sidi.Test;
using System.Net;

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
            foreach (var vp in new Parser().AvailableValueParsers)
            {
                log.Info(vp.UsageText);
                log.Info(vp.MethodInfo);
                foreach (var example in vp.Examples)
                {
                    log.InfoFormat("Parsing {0}", example.Value.Quote());
                    var r = vp.Handle(new List<string>() { example.Value }, true);
                    log.InfoFormat("Parsing {0} returns {1}", example.Value.Quote(), r);
                }
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
        }
            
        [Test]
        public void StaticParse()
        {
            var a = new SampleApp();
            var ipString = "1.2.3.4";
            Parser.Run(a, new[]{"Address", ipString});
            Assert.AreEqual(new IPAddress(new byte[]{1,2,3,4}), a.Address);
        }
    }
}