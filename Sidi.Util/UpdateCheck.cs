using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;
using System.Net;
using System.IO;
using Sidi.IO;
using System.Threading;
using System.Net.Cache;
using Microsoft.Build.Framework;

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

        public void Write(string outFile)
        {
            XmlSerializer s = new XmlSerializer(typeof(UpdateInfo));
            using (Stream f = File.OpenWrite(outFile))
            {
                s.Deserialize(f);
            }
        }
    }

    public class CreateUpdateInfo : ITask
    {
        #region ITask Members

        public IBuildEngine BuildEngine { set; get; }

        public bool Execute()
        {
            Assembly a = Assembly.ReflectionOnlyLoadFrom(AssemblyFile);
            VersionInfo v = new VersionInfo();
            v.AssemblyName = a.GetName();
            UpdateInfo i = new UpdateInfo();
            i.VersionInfo.Add(v);
            i.Write(OutputFile);
            return true;
        }

        public string AssemblyFile { set; get; }

        public string OutputFile { set; get; }

        public ITaskHost HostObject { set; get; }

        #endregion
    }


    public class UpdateCheck
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Uri updateInfoUri;
        UpdateInfo updateInfo;
        Assembly assembly;

        public class RequiresUpdateEventArgs
        {
            public string Message { get; set; }
            public string InstalledVersion { get; set; }
            public string AvailableVersion { get; set; }
            public string DownloadUrl { set; get; }
        }
        
        public delegate void RequiresUpdateEvent(object sender, RequiresUpdateEventArgs e);
        
        public event RequiresUpdateEvent RequiresUpdate;

        public Assembly Assembly
        {
            get { return assembly; }
            set { assembly = value; }
        }

        public bool IsUpdateRequired
        {
            get
            {
                return assembly.GetName().Version < AvailableVersion;
            }
        }

        Version AvailableVersion
        {
            get
            {
                return new Version(VersionInfo.Version);
            }
        }

        public VersionInfo VersionInfo
        {
            get
            {
                return updateInfo.VersionInfo.First(x => x.Name.Equals(assembly.GetName().Name));
            }
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
        
        public UpdateCheck(Assembly assembly, Uri updateInfoUri)
        {
            this.updateInfoUri = updateInfoUri;
            this.assembly = assembly;
        }

        public void CheckUpdateAvailable()
        {
            ReadUpdateInfo(updateInfoUri);
        }

        public void ReadUpdateInfo(Uri updateInfoUri)
        {
            WebClient web = new WebClient();
            web.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            web.DownloadDataCompleted += new DownloadDataCompletedEventHandler(web_DownloadDataCompleted);
            UriBuilder uriBuilder = new UriBuilder(updateInfoUri);
            if (!updateInfoUri.IsFile)
            {
                AssemblyName an = assembly.GetName();
                uriBuilder.Query = "name=" + an.Name + "&" + "version=" + an.Version.ToString();
            }
            log.InfoFormat("Downloading update info from {0}", uriBuilder.Uri);
            web.DownloadDataAsync(uriBuilder.Uri);
        }

        void web_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            MemoryStream m = new MemoryStream(e.Result);
            XmlSerializer s = new XmlSerializer(typeof(UpdateInfo));
            UpdateInfo = (UpdateInfo)s.Deserialize(m);
        }

        protected virtual void OnUpdateInfoChanged()
        {
            XmlSerializer s = new XmlSerializer(typeof(UpdateInfo));
            s.Serialize(Console.Out, UpdateInfo);

            if (IsUpdateRequired)
            {
                if (RequiresUpdate != null)
                {
                    RequiresUpdateEventArgs e = new RequiresUpdateEventArgs();
                    e.AvailableVersion = AvailableVersion.ToString();
                    e.InstalledVersion = Assembly.GetName().Version.ToString();
                    e.Message = VersionInfo.Message;
                    e.DownloadUrl = VersionInfo.DownloadUrl;
                    RequiresUpdate(this, e);
                }
            }
        }
    }

}
