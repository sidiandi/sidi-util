using NUnit.Framework;
using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Text = Sidi.IO.Text;

namespace Sidi.IO
{
    [TestFixture]
    class ParserTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void ParseExpressions()
        {
            var p = new Parser();
            var text = new Text(@"// hello, world

/* dfj;asldkfj asd;lfkja sdf

asdfjadskfj


asdlfjasldfa s;dlkj;lk

*/

adkfjahs dfkjha sdfka hsdfkj

");
            log.Info(p.Whitespace(ref text));
        }
        
        [Test]
        public void Parse()
        {
            log.Info(new LPath(@"C:\temp\hello.txt"));
            log.Info(new LPath(@"\\server\share\temp\hello.txt"));
            log.Info(new LPath(@"\\?\UNC\server\share\temp\hello.txt"));
            log.Info(new LPath(@"\temp\hello.txt"));
            log.Info(new LPath(@"temp\hello.txt"));
        }
    }
}
