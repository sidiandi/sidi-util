using NUnit.Framework;
using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    [TestFixture]
    class ParserTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void ParseExpressions()
        {
            var text = new Sidi.Parse.Text(@"// hello, world

/* dfj;asldkfj asd;lfkja sdf

asdfjadskfj


asdlfjasldfa s;dlkj;lk

*/

adkfjahs dfkjha sdfka hsdfkj

");
            log.Info(Sidi.Parse.Parser.Whitespace(text));
        }
        
        [Test]
        public void Parse()
        {
            foreach (var i in new[]{
@"C:\temp\hello.txt",
@"\\server\share\temp\hello.txt",
@"\\?\UNC\server\share\temp\hello.txt",
@"\temp\hello.txt",
@"temp\hello.txt"})
            {
                var p = new LPath(i);
                log.InfoFormat("{0} = {1}", i, p);
                Assert.AreEqual(i, p.ToString());
            }
        }
    }
}
