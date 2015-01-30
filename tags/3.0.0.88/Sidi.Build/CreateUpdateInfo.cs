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
            var a = Assembly.ReflectionOnlyLoadFrom(AssemblyFile);
            var v = new VersionInfo
            {
                AssemblyName = a.GetName(),
                DownloadUrl = DownloadUrl,
                Message = Message
            };
            var i = new UpdateInfo();
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
