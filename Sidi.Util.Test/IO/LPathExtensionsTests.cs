using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.IO;
using NUnit.Framework;
using Sidi.Test;
using System.IO;
using Sidi.Extensions;
using Sidi.Util;

namespace Sidi.IO.Tests
{
    [TestFixture()]
    public class LPathExtensionsTests : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test()]
        public void ReadTest()
        {
            var f = TestFile(System.IO.Path.GetRandomFileName());
            f.EnsureFileNotExists();
            f.Write(WriteNumbers);
            var numbers = f.Read(ReadNumbers);
            log.Info(() => numbers.Take(10));
            log.Info(() => numbers.Take(10));
        }

        static void WriteNumbers(TextWriter w)
        {
            for (int i=0;i<100;++i)
            {
                w.WriteLine(i);
            }
        }

        static IEnumerable<int> ReadNumbers(TextReader r)
        {
            for (; ; )
            {
                yield return Int32.Parse(r.ReadLine());
            }
        }

        static IEnumerable<byte> ReadBytes(Stream s)
        {
            for (;;)
            {
                var b = s.ReadByte();
                if (b < 0)
                {
                    break;
                }
                yield return (byte)b;
            }
        }

        [Test()]
        public void ReadTest1()
        {
            var f = TestFile(@"mail\message-1-1456.eml");
            log.Info(() => f.Read(ReadBytes).Take(100));
        }
    }
}
