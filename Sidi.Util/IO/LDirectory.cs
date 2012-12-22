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
            if (!NativeMethods.RemoveDirectory(directory.Param))
            {
                new LFileSystemInfo(directory).IsReadOnly = false;
                NativeMethods.RemoveDirectory(directory.Param).CheckApiCall(directory);
            }
            log.InfoFormat("Delete {0}", directory);
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
            NativeMethods.MoveFileEx(from.Param, to.Param, 0).CheckApiCall(String.Format("{0} -> {1}", from, to));
        }

        /// <summary>
        /// Thin wrapper around FindFirstFile and FindNextFile. Also will return "." and ".."
        /// </summary>
        /// <param name="searchPath"></param>
        /// <returns></returns>
        internal static IEnumerable<FindData> FindFileRaw(LPath searchPath)
        {
            FindData fd;

            using (var fh = NativeMethods.FindFirstFile(searchPath.Param, out fd))
            {
                if (!fh.IsInvalid)
                {
                    yield return fd;
                    while (NativeMethods.FindNextFile(fh, out fd))
                    {
                        yield return fd;
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates found files. Make sure that the Enumerator is closed properly to free the Find handle.
        /// </summary>
        /// <param name="searchPath">File search path complete with wildcards, e.g. C:\temp\*.doc</param>
        /// <returns></returns>
        public static IEnumerable<LFileSystemInfo> FindFile(LPath searchPath)
        {
            return FindFileRaw(searchPath)
                .Where(x => !(x.Name.Equals(ThisDir) || x.Name.Equals(UpDir)))
                .Select(x => new LFileSystemInfo(searchPath.Parent, x));
        }

        public static void Create(LPath path)
        {
            CreateDirectoryInternal(path);
        }

        public static LPath Current
        {
            get
            {
                return new LPath(System.Environment.CurrentDirectory);
            }
        }

        const int ERROR_ALREADY_EXISTS = 183;
        const int ERROR_PATH_NOT_FOUND = 3;

        static void CreateDirectoryInternal(LPath path)
        {
            if (!NativeMethods.CreateDirectory(path.Param, IntPtr.Zero))
            {
                switch (Marshal.GetLastWin32Error())
                {
                    case ERROR_ALREADY_EXISTS:
                        return;
                    case ERROR_PATH_NOT_FOUND:
                        {
                            var p = path.Parent;
                            CreateDirectoryInternal(p);
                            log.InfoFormat("Create directory {0}", path);
                            NativeMethods.CreateDirectory(path.Param, IntPtr.Zero).CheckApiCall(path);
                        }
                        break;
                    default:
                        false.CheckApiCall(path);
                        break;
                }
            }
        }
    }

}
