using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Util;
using NUnit.Framework;
using System.IO;
namespace Sidi.Util.Tests
{
    [TestFixture()]
    public class HexDumpTests
    {
        byte[] data = ASCIIEncoding.ASCII.GetBytes(@"using NUnit.Framework;
using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
");

        [Test()]
        public void WriteTest()
        {
            var sw = new StringWriter();
            using (var hexDump = new HexDumpStream(sw))
            {
                new MemoryStream(data).CopyTo(hexDump);
            }
            StringAssert.EndsWith(@"000000A0 3B 0D 0A                                        ;..             
", sw.ToString());

            sw = new StringWriter();
            using (var hexDump = new HexDumpStream(sw))
            {
                hexDump.Columns = 8;
                new MemoryStream(data).CopyTo(hexDump);
            }
            StringAssert.EndsWith(@"000000A0 3B 0D 0A                ;..     
", sw.ToString());
        }

        [Test()]
        public void WriteTest1()
        {
            var sw = new StringWriter();
            HexDump.Write(data, sw);
            StringAssert.EndsWith(@"000000A0 3B 0D 0A                                        ;..             
", sw.ToString());
        }

        [Test()]
        public void AsHexDumpTest()
        {
            StringAssert.EndsWith(@"000000A0 3B 0D 0A                                        ;..             
", data.AsHexDump());
        }
    }
}
