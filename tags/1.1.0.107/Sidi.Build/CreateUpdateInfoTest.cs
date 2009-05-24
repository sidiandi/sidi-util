using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.IO;
using System.Reflection;
using Sidi.Util;

namespace Sidi.Build.Test
{
    [TestFixture]
    public class CreateUpdateInfoTest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        CreateUpdateInfo instance = null;

        [SetUp]
        public void SetUp()
        {
            instance = new CreateUpdateInfo();
        }

        [Test]
        public void Writer()
        {
            string outFile = FileUtil.BinFile(@"Test\generated.xml");
            {
                CreateUpdateInfo cui = new CreateUpdateInfo();
                cui.OutputFile = outFile;
                cui.AssemblyFile = Assembly.GetExecutingAssembly().Location;
                cui.Execute();
            }

            UpdateCheck c = new UpdateCheck(Assembly.GetExecutingAssembly(), new Uri(outFile));
            c.Check();
            Assert.IsTrue(!c.IsUpdateRequired);
        }

    }
}
