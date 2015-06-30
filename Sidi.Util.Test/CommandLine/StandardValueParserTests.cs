using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.CommandLine;
using NUnit.Framework;
namespace Sidi.CommandLine.Tests
{
    [TestFixture()]
    public class StandardValueParserTests
    {
        public class Argument
        {
        }

        [Test()]
        public void StandardValueParserTest_gives_correct_exception_message_on_unsupported_type()
        {
            try
            {
                new Parser().ParseValue(new List<string> { "someArg" }, typeof(Argument));
                Assert.Fail("must throw");
            }
            catch (CommandLineException ex)
            {
                
                StringAssert.Contains("public static Argument Parse(string)", ex.InnerException.Message);
            }
        }
    }
}
