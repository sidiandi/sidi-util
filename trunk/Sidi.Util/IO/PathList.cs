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
using System.Windows.Forms;
using Sidi.Util;
using Sidi.Extensions;

namespace Sidi.IO
{
    public class PathList : List<LPath>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PathList(IEnumerable<LPath> paths)
        : base(paths)
        {
        }

        public PathList()
        {
        }

        public static PathList ReadClipboard()
        {
            var fileList = new PathList();
            fileList.AddRange(Clipboard.GetFileDropList()
                .Cast<string>()
                .Select(x => new Sidi.IO.LPath(x)));
            return fileList;
        }

        public static PathList Parse(string files)
        {
            if (files.Equals(":paste", StringComparison.InvariantCultureIgnoreCase))
            {
                return ReadClipboard();
            }

            return new PathList(files.Split(new[] { ";" }, StringSplitOptions.None).Select(x => new Sidi.IO.LPath(x)));
        }

        public override string ToString()
        {
            return this.Join(";");
        }
    }
}
