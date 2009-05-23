using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.IO;
using System.IO;
using Sidi.Util;

namespace Sidi.Build.Test
{
    [TestFixture]
    public class PdbstrTest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Pdbstr instance = null;

        public PdbstrTest()
        {
            log4net.Config.BasicConfigurator.Configure();
        }

        [SetUp]
        public void SetUp()
        {
            instance = new Pdbstr();
        }

        string pdbFile = FileUtil.BinFile(@"Test\Sidi.Util.pdb");

        [Test]
        public void Write()
        {
            string pdbFileCopy = pdbFile + ".modified.pdb";
            File.Copy(pdbFile, pdbFileCopy, true);

            string content = "Hello, Test";
            string streamName = "srcsrv";

            instance.Write(pdbFileCopy, streamName, content);

            Assert.IsTrue(instance.Read(pdbFileCopy, streamName).StartsWith(content));
        }
    }
}
