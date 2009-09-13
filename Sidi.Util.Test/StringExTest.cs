using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using Sidi.Util;

namespace Sidi.Test.Util
{
    [TestFixture]
    public class StringExTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        [Test]
        public void Printable()
        {
            foreach (var i in Directory.GetFiles(@"I:\1_MR\1_HQMR\Teams\ExamFramework\arfa\cache\N4_VD11A_LATEST_20090905\Debug\examdb\MriProduct\examdb\Root\Test_Region\Test_Exam\Test_I18n"))
            {
                log.Info(i);
            }
        }

        [Test]
        public void GetSection()
        {
            StringWriter w = new StringWriter();
            w.WriteLine("[SectionA]");
            w.WriteLine("hello");
            w.WriteLine("[SectionB]");
            w.WriteLine("world");

            string t = w.ToString();

            Assert.AreEqual("hello", t.GetSection("SectionA"));
            Assert.AreEqual("world", t.GetSection("SectionB"));
        }

        [Test, Explicit("interactive")]
        public void EditInteractive()
        {
            log.Info("Hello, world".EditInteractive());
        }
    }
}
