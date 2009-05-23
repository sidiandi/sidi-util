using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using System.Reflection;
using Sidi.Util;

namespace Sidi.Build
{
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
}
