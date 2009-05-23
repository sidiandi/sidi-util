using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Sidi.IO;

namespace Sidi.Build.Test
{
    [TestFixture]
    public class SourceIndexTest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        SourceIndex instance = null;

        public SourceIndexTest()
        {
            log4net.Config.BasicConfigurator.Configure();
        }

        [SetUp]
        public void SetUp()
        {
            instance = new SourceIndex();
        }

        [Test]
        public void Test()
        {
            instance.Directory = @"D:\work\Sidi.Util";
            instance.Url = "http://sidi-util.googlecode.com/svn/trunk";

            TaskItem t = new TaskItem(@"D:\temp\Sidi.Util.dll");
            instance.Modules = new ITaskItem[] { t };

            instance.Execute();

            Srctool s = new Srctool();
            s.Extract(t.ItemSpec.ReplaceExtension("pdb"));
        }
    }
}
