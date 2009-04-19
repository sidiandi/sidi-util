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
using System.Windows.Forms;

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
                s.Serialize(f, this);
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
            v.DownloadUrl = DownloadUrl;
            v.Message = Message;
            UpdateInfo i = new UpdateInfo();
            i.VersionInfo.Add(v);
            i.Write(OutputFile);
            return true;
        }

        public string AssemblyFile { set; get; }
        public string DownloadUrl { set; get; }
        public string Message { set; get; }

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
                return updateInfo.VersionInfo.First(x => x.Name.Equals(assembly.GetName().Name));
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
                        UpdateForm u = new UpdateForm(this);
                        u.ShowDialog();
                    }));
                }
                else
                {
                    UpdateForm u = new UpdateForm(this);
                    u.ShowDialog();
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
            WebClient web = new WebClient();
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
            MemoryStream m = new MemoryStream(data);
            XmlSerializer s = new XmlSerializer(typeof(UpdateInfo));
            UpdateInfo = (UpdateInfo)s.Deserialize(m);
        }

        protected virtual void OnUpdateInfoChanged()
        {
            XmlSerializer s = new XmlSerializer(typeof(UpdateInfo));
            s.Serialize(Console.Out, UpdateInfo);

            if (IsUpdateRequired)
            {
                updateRequiredHandler();
            }
        }
    }

}
