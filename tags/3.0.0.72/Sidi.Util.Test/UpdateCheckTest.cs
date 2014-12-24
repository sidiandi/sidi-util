// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

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
using Sidi.Test;

namespace Sidi.Util
{
    [TestFixture]
    public class UpdateCheckTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            Uri uri = new Uri(TestFile("UpdateInfo.xml"));
            log.Info(uri);
            UpdateCheck c = new UpdateCheck(Assembly.GetExecutingAssembly(), uri);
            c.Check();
            Assert.IsTrue(c.IsUpdateRequired);
        }

        [Test]
        public void CheckAsync()
        {
            UriBuilder u = new UriBuilder();
            Uri uri = new Uri(TestFile("UpdateInfo.xml"));
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

        [Test, Explicit("interactive")]
        public void TestSimple()
        {
            UpdateCheck c = new UpdateCheck(new Uri(TestFile("UpdateInfo.xml")));
            c.CheckAsync();
            c.WaitCompleted();
        }

        [Test, Explicit("interactive")]
        public void TestOpen()
        {
            Process p = new Process();
            p.StartInfo.FileName = "http://www.spiegel.de";
            p.StartInfo.UseShellExecute = true;
            p.Start();
        }
    }
}



