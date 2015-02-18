using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Threading;
using System.Drawing;
using Sidi.Test;

namespace Sidi.Forms
{
    [TestFixture]
    public class JobListTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test, Explicit]
        public void Show()
        {
            var j = new JobListView();
            for (int i = 0; i < 10; ++i)
            {
                j.JobList.Jobs.Add(new Job(i.ToString(), bgw =>
                {
                    for (int n = 0; n < 100; ++n)
                    {
                        Thread.Sleep(100);
                        bgw.ReportProgress(n, n);
                    }
                }));
            }
            j.Size = new Size(800, 600);
            j.Run();
        }

        [Test, Explicit]
        public void Actions()
        {
            var j = new JobListView();
            j.JobList.MaxBusyCount = 8;
            for (int i = 0; i < 30; ++i)
            {
                j.JobList.Jobs.Add(new Job("Counting " + i.ToString(), () =>
                {
                    var nEnd = 100;
                    for (int n = 0; n < nEnd; ++n)
                    {
                        Thread.Sleep(100);
                        log.InfoFormat("counting to {0}: {1}", nEnd, n);
                    }
                }));
            }
            j.Size = new Size(800, 600);
            j.Run();
        }
    }
}
