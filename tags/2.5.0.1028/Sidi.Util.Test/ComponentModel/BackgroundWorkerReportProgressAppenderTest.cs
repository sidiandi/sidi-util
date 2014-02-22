using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using Sidi.Test;

namespace Sidi.ComponentModel
{
    [TestFixture]
    public class BackgroundWorkerReportProgressAppenderTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Log()
        {
            int p = 0;

            var bgw = new BackgroundWorker()
            {
                WorkerReportsProgress = true
            };
            bgw.ProgressChanged += (s, e) =>
            {
                p = e.ProgressPercentage;
            };
            using (new BackgroundWorkerReportProgressAppender(bgw))
            {
                log.Info("hello");
            }

            Assert.AreEqual(1, p);
        }
    }
}
