using NUnit.Framework;
using Sidi.Parse2;
using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    [TestFixture]
    class PathParser2Test : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void ParseExpressions()
        {
            var text = new Sidi.Parse2.Text(@"// hello, world

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
            var text = new Sidi.Parse2.Text(" ");
            var ast = PathParser2.NtfsAllowedCharacter()(text);
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
                var p = PathParser2.Path()(new Sidi.Parse2.Text(i));
                Assert.NotNull(p);
                log.Info(p.Details);
                var path = new LPath(i);
                log.InfoFormat("{0} = {1}", i, path);
                Assert.AreEqual(i, path.ToString());
            }
        }
    }
}
