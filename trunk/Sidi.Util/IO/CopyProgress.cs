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
using Sidi.Util;

namespace Sidi.IO
{
    public class CopyProgress
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CopyProgress(
            LPath source,
            LPath destination)
        {
            this.Source = source;
            this.Destination = destination;
            this.Progress = new Progress();
        }

        public Progress Progress { get; private set; }

        public bool Update(long totalBytesTransferred, long totalFileSize)
        {
            Progress.Total = totalFileSize;
            return Progress.Update(totalBytesTransferred);
        }

        public LPath Source;
        public LPath Destination;

        public string Message
        {
            get
            {
                return String.Format("{0}: {1} -> {2}", Progress.ToString(), Source, Destination);
            }
        }

        public override string ToString()
        {
            return Message;
        }
    }

}
