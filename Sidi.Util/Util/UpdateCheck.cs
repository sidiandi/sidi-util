// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.Reflection;
using System.Xml.Serialization;
using System.Net;
using System.IO;
using L = Sidi.IO;
using System.Threading;
using System.Net.Cache;
using System.Windows.Forms;
using Sidi.Extensions;
using Sidi.IO;

namespace Sidi.Util
{
    public class VersionInfo
    {
        [XmlIgnore]
        public AssemblyName AssemblyName
        {
            set
            {
                Name = value.Name;
                Version = value.Version.ToString();
            }
        }

        public string Name;
        public string Version;
        public string DownloadUrl;
        public string Message;
    }

    public class UpdateInfo
    {
        public List<VersionInfo> VersionInfo = new List<VersionInfo>();

        public void Write(LPath outFile)
        {
            XmlSerializer s = new XmlSerializer(typeof(UpdateInfo));
            using (Stream f = outFile.OpenWrite())
            {
                s.Serialize(f, this);
            }
        }
    }

    public class UpdateCheck
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Uri updateInfoUri;
        UpdateInfo updateInfo;
        Assembly assembly;

        public Assembly Assembly
        {
            get { return assembly; }
            set { assembly = value; }
        }

        public bool IsUpdateRequired
        {
            get
            {
                return InstalledVersion < AvailableVersion;
            }
        }

        public Version InstalledVersion
        {
            get
            {
                return assembly.GetName().Version;
            }
        }

        public Version AvailableVersion
        {
            get
            {
                return new Version(VersionInfo.Version);
            }
        }

        VersionInfo VersionInfo
        {
            get
            {
                try
                {
                    return updateInfo.VersionInfo.First(x => x.Name.Equals(assembly.GetName().Name));
                }
                catch (Exception ex)
                {
                    throw new Exception("No VersionInfo for {0}".F(assembly), ex);
                }
            }
        }

        public string DownloadUrl
        {
            get { return VersionInfo.DownloadUrl; }
        }

        public string Message
        {
            get { return VersionInfo.Message; }
        }

        public UpdateInfo UpdateInfo
        {
            get { return updateInfo; }
            set
            {
                updateInfo = value;
                log.Info("Update info changed.");
                OnUpdateInfoChanged();
            }
        }

        public UpdateCheck(Uri updateInfoUri)
            : this(Assembly.GetCallingAssembly(), updateInfoUri)
        {
        }


        public UpdateCheck(Assembly assembly, Uri updateInfoUri)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            if (updateInfoUri == null)
            {
                throw new ArgumentNullException("updateInfoUri");
            }
            this.updateInfoUri = updateInfoUri;
            this.assembly = assembly;
        }

        public void Check()
        {
            try
            {
                ReadUpdateInfo(false);
            }
            catch (Exception e)
            {
                log.Error("Error during update check", e);
            }
        }

        Action updateRequiredHandler;

        public void CheckAsync()
        {
            CheckAsync(delegate()
            {
                FormCollection of = Application.OpenForms;
                if (of.Count > 0)
                {
                    of[0].Invoke(new Action(delegate()
                    {
                        using (var u = new UpdateForm(this))
                        {
                            u.ShowDialog();
                        }
                    }));
                }
                else
                {
                    using (var u = new UpdateForm(this))
                    {
                        u.ShowDialog();
                    }
                }
            });
        }

        public void CheckAsync(Action updateRequiredHandler)
        {
            this.updateRequiredHandler = updateRequiredHandler;
            completed = false;
            ReadUpdateInfo(true);
        }

        bool completed = true;

        public void WaitCompleted()
        {
            while (!completed)
            {
                Thread.Sleep(100);
            }
        }

        Uri uri;

        public void ReadUpdateInfo(bool async)
        {
            using (WebClient web = new WebClient())
            {
                web.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                UriBuilder uriBuilder = new UriBuilder(updateInfoUri);
                if (!updateInfoUri.IsFile)
                {
                    AssemblyName an = assembly.GetName();
                    uriBuilder.Query = "name=" + an.Name + "&" + "version=" + an.Version.ToString();
                }
                uri = uriBuilder.Uri;
                log.InfoFormat("Downloading update info from {0}", uri);
                if (async)
                {
                    web.DownloadDataCompleted += new DownloadDataCompletedEventHandler(web_DownloadDataCompleted);
                    web.DownloadDataAsync(uri);
                }
                else
                {
                    Parse(web.DownloadData(uri));
                }
            }
        }

        void web_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                log.Error(String.Format("Error while downloading {0}", uri), e.Error);
            }
            else
            {
                try
                {
                    Parse(e.Result);
                }
                catch (Exception ex)
                {
                    log.Error("Error while processing update info", ex);
                }
            }
            completed = true;
        }

        void Parse(byte[] data)
        {
            using (var m = new MemoryStream(data))
            {
                XmlSerializer s = new XmlSerializer(typeof(UpdateInfo));
                UpdateInfo = (UpdateInfo)s.Deserialize(m);
            }
        }

        protected virtual void OnUpdateInfoChanged()
        {
            XmlSerializer s = new XmlSerializer(typeof(UpdateInfo));
            s.Serialize(Console.Out, UpdateInfo);

            if (IsUpdateRequired)
            {
                if (updateRequiredHandler != null)
                {
                    updateRequiredHandler();
                }
            }
        }
    }

}
