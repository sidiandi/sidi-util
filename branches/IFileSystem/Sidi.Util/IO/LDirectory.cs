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
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Sidi.IO
{
    [Obsolete("Use LPath, IFileSystem. IFileSystemInfo methods")]
    public class LDirectory
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const string PathSep = @"\";

        public static void Delete(LPath directory)
        {
            directory.RemoveDirectory();
        }

        public static bool Exists(LPath directory)
        {
            return directory.Info.IsDirectory;
        }

        public static void Move(LPath from, LPath to)
        {
            from.Move(to);
        }

        public static void Create(LPath path)
        {
            path.EnsureDirectoryExists();
        }

        public static LPath Current
        {
            get
            {
                return FileSystem.Current.CurrentDirectory;
            }
        }
    }
}
