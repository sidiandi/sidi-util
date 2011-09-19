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