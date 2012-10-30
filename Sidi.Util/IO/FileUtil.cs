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
using System.Text;
using System.IO;
using System.Reflection;
using Sidi.Util;
using Sidi.Extensions;
using System.Runtime.InteropServices;
using System.Linq;
using L = Sidi.IO;

namespace Sidi.IO
{
    public static class FileUtil
    {
        /// <summary>
        /// Searches fileName in directory first, then in all parent directories
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string SearchUp(L.Path directory, string fileName)
        {
            if (directory == null)
            {
                return null;
            }

            for (var p = directory; p != null; p = p.Parent)
            {
                var file = p.CatDir(fileName);
                if (File.Exists(file))
                {
                    return file;
                }
            }
            return null;
        }

        public static System.IO.FileSystemInfo GetFileSystemInfo(this string path)
        {
            FileInfo f = new FileInfo(path);
            if ((f.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return new DirectoryInfo(path);
            }
            else
            {
                return f;
            }
        }

        public static string GetRelativePath(this string path, string basePath)
        {
            basePath = System.IO.Path.GetFullPath(basePath);
            path = System.IO.Path.GetFullPath(path);

            string[] fromDirs = basePath.Split(System.IO.Path.DirectorySeparatorChar);
            string[] toDirs = path.Split(System.IO.Path.DirectorySeparatorChar);

            int i = 0;
            for (; i < fromDirs.Length && i < toDirs.Length; ++i)
            {
                if (fromDirs[i] != toDirs[i])
                {
                    break;
                }
            }
            int identicalIndex = i;

            List<string> rel = new List<string>();

            for (i = Math.Max(identicalIndex, fromDirs.Length); i > identicalIndex; --i)
            {
                rel.Add("..");
            }
            for (; i < toDirs.Length; ++i)
            {

                rel.Add(toDirs[i]);
            }

            return rel.Join(new String(System.IO.Path.DirectorySeparatorChar, 1));
        }

        /// <summary>
        /// Compares two files bytewise
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool FilesAreEqual(string a, string b)
        {
            if (File.Exists(a))
            {
                if (File.Exists(b))
                {
                    FileInfo ia = new FileInfo(a);
                    FileInfo ib = new FileInfo(b);
                    if (ia.Length != ib.Length)
                    {
                        return false;
                    }

                    Stream fa = null;
                    Stream fb = null;
                    try
                    {
                        fa = File.OpenRead(a);
                        fb = File.OpenRead(b);
                        int da;
                        int db;
                        do
                        {
                            da = fa.ReadByte();
                            db = fb.ReadByte();
                            if (da != db)
                            {
                                return false;
                            }
                        }
                        while (da != -1);

                        return true;
                    }
                    finally
                    {
                        if (fa != null) fa.Close();
                        if (fb != null) fb.Close();
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (File.Exists(b))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public static bool FilesAreEqualByTime(string a, string b)
        {
            if (File.Exists(a))
            {
                if (File.Exists(b))
                {
                    var fa = new FileInfo(a);
                    var fb = new FileInfo(b);
                    return 
                        fa.Length == fb.Length &&
                        Math.Abs((fa.LastWriteTimeUtc - fb.LastWriteTimeUtc).TotalSeconds) < 2.0;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return !File.Exists(b);
            }
        }

        public static void CreateHardLink(
                string fileName,
                string existingFileName)
        {
            bool result = NativeMethods.CreateHardLink(fileName, existingFileName, IntPtr.Zero);
            if (!result)
            {
                throw new System.IO.IOException(String.Format("Cannot create hard link: {0} -> {1}", fileName, existingFileName));
            }
        }

    }
}
