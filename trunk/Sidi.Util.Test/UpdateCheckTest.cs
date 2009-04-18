using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;
using Sidi.IO;
using System.Threading;
using NUnit.Framework;
using Sidi.Util;
using System.Diagnostics;

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
            UpdateCheck c = new UpdateCheck(Assembly.GetExecutingAssembly(), uri);
            c.Check();
            Assert.IsTrue(c.IsUpdateRequired);
        }

        [Test]
        public void CheckAsync()
        {
            UriBuilder u = new UriBuilder();
            Uri uri = new Uri(FileUtil.BinFile(@"Test\UpdateInfo.xml"));
            UpdateCheck c = new UpdateCheck(Assembly.GetExecutingAssembly(), uri);
            c.CheckAsync(delegate()
            {
                log.Info(c.AvailableVersion);
                log.Info(c.InstalledVersion);
                log.Info(c.DownloadUrl);
            });

            c.WaitCompleted();
            Assert.IsTrue(c.IsUpdateRequired);
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

        [Test]
        public void Test404()
        {
            UpdateCheck c = new UpdateCheck(Assembly.GetExecutingAssembly(), new Uri("http://andreas-grimme.gmxhome.de/ThisFileDoesNotExist"));
            c.Check();
        }

        [Test]
        public void TestSimple()
        {
            UpdateCheck c = new UpdateCheck(new Uri(FileUtil.BinFile(@"Test\UpdateInfo.xml")));
            c.CheckAsync();
            c.WaitCompleted();
        }

        [Test]
        public void TestOpen()
        {
            Process p = new Process();
            p.StartInfo.FileName = "http://www.spiegel.de";
            p.StartInfo.UseShellExecute = true;
            p.Start();
        }
    }
}



