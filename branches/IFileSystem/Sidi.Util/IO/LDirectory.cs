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
    public class LDirectory
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const string PathSep = @"\";
        const string ThisDir = ".";
        const string UpDir = "..";

        public static void Delete(LPath directory)
        {
            FileSystem.Current.RemoveDirectory(directory);
        }

        public static bool Exists(LPath directory)
        {
            FindData fd;
            if (directory.GetFindData(out fd))
            {
                return fd.IsDirectory;
            }
            else
            {
                if (directory.IsUnc && directory.Parts.Length == 4)
                {
                    return System.IO.Directory.Exists(directory.NoPrefix);
                }

                return false;
            }
        }

        public static void Move(LPath from, LPath to)
        {
            FileSystem.Current.Move(from, to);
        }

        /// <summary>
        /// Enumerates found files. Make sure that the Enumerator is closed properly to free the Find handle.
        /// </summary>
        /// <param name="searchPath">File search path complete with wildcards, e.g. C:\temp\*.doc</param>
        /// <returns></returns>
        public static IEnumerable<LFileSystemInfo> FindFile(LPath searchPath)
        {
            return FileSystem.Current.FindFileRaw(searchPath)
                .Where(x => !(x.Name.Equals(ThisDir) || x.Name.Equals(UpDir)))
                .Select(x => new LFileSystemInfo(searchPath.Parent, x));
        }

        public static void Create(LPath path)
        {
            FileSystem.Current.EnsureDirectoryExists(path);
        }

        public static LPath Current
        {
            get
            {
                return new LPath(System.Environment.CurrentDirectory);
            }
        }
    }
}
