using NUnit.Framework;
using Sidi.Parse;
using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Text = Sidi.Parse.Text;

namespace Sidi.IO
{
    [TestFixture]
    class PathParserTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void ParseExpressions()
        {
            var text = new Text(@"// hello, world

/* dfj;asldkfj asd;lfkja sdf

asdfjadskfj


asdlfjasldfa s;dlkj;lk

*/

adkfjahs dfkjha sdfka hsdfkj

");
            // log.Info(Parser.Whitespace()(text));
        }

        [Test]
        public void ParseNtfs()
        {
            var text = new Text(" ");
            var ast = PathParser.NtfsAllowedCharacter()(text);
            Assert.NotNull(ast);
            log.Info(ast.Details);
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
                var p = PathParser.Path()(new Text(i));
                Assert.NotNull(p);
                log.Info(p.Details);
                var path = new LPath(i);
                log.InfoFormat("{0} = {1}", i, path);
                Assert.AreEqual(i, path.ToString());
            }
        }
    }
}
