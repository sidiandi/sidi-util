using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;
using Sidi.IO;
using System.Threading;
using NUnit.Framework;

namespace Sidi.Util.Test
{
    [TestFixture]
    public class UpdateCheckTest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UpdateCheckTest()
        {
            log4net.Config.BasicConfigurator.Configure();
        }

        [Test]
        public void Serialize()
        {
            XmlSerializer s = new XmlSerializer(typeof(UpdateInfo));

            UpdateInfo u = new UpdateInfo();

            VersionInfo v = new VersionInfo();
            v.AssemblyName = Assembly.GetExecutingAssembly().GetName();
            v.Message = "";
            v.DownloadUrl = "http://andreas-grimme.gmxhome.de/trackutil/";
            u.VersionInfo.Add(v);

            s.Serialize(Console.Out, u);
        }

        [Test]
        public void Check()
        {
            UriBuilder u = new UriBuilder();
            Uri uri = new Uri(FileUtil.BinFile(@"Test\UpdateInfo.xml"));
            // Uri uri = new Uri("http://andreas-grimme.gmxhome.de/trackutil/UpdateInfo.xml");
            UpdateCheck c = new UpdateCheck(Assembly.GetExecutingAssembly(), uri);
            c.RequiresUpdate += new UpdateCheck.RequiresUpdateEvent(c_RequiresUpdate);
            c.CheckUpdateAvailable();


            while (c.UpdateInfo == null)
            {
                Thread.Sleep(1000);
            }

            Assert.IsTrue(c.IsUpdateRequired);
        }

        void c_RequiresUpdate(object sender, UpdateCheck.RequiresUpdateEventArgs e)
        {
            log.Info(e.AvailableVersion);
        }
    }
}
