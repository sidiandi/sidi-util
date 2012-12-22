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
using System.Text;
using System.IO;
using System.Reflection;
using Sidi.Util;

namespace Sidi.IO
{
    public static class FileUtil
    {
        /// <summary>
        /// Constructs the path for a sibling of a file, i.e. of a file in the same directory.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="siblingName"></param>
        /// <returns></returns>
        public static string Sibling(this string path, string siblingName)
        {
            DirectoryInfo parent = System.IO.Directory.GetParent(path);
            if (!parent.Exists)
            {
                parent.Create();
            }
            return System.IO.Path.Combine(
                parent.FullName,
                siblingName);
        }

        /// <summary>
        /// Searches fileName in directory first, then in all parent directories
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string SearchUp(string directory, string fileName)
        {
            if (directory == null)
            {
                return null;
            }

            string p = FileUtil.CatDir(directory, fileName);
            if (File.Exists(p))
            {
                return p;
            }

            return FileUtil.SearchUp(Path.GetDirectoryName(directory), fileName);
        }


        public static string Sibling(this Assembly assembly, string siblingName)
        {
            return Sibling(assembly.Location, siblingName);
        }

        /// <summary>
        /// Returns a file in the same directory as the assembly.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string BinFile(string path)
        {
            return Assembly.GetExecutingAssembly().Sibling(path);
        }

        public static string UserSetting(this Type type, string name)
        {
            string root = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            string assemblyName = type.Assembly.GetName().Name;
            string path = CatDir(root, assemblyName, name);
            return path;
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

        public static string CatDir(params string[] dirs)
        {
            string r = String.Empty;
            foreach (string i in dirs)
            {
                r = System.IO.Path.Combine(r, i);
            }
            return r;
        }

        public static string CatDir(IEnumerable<string> dirs)
        {
            string r = String.Empty;
            foreach (string i in dirs)
            {
                r = System.IO.Path.Combine(r, i);
            }
            return r;
        }

        /// <summary>
        /// Replaces the file extension of a path. 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="newExtension">New extension (without dot)</param>
        /// <returns></returns>
        public static string ReplaceExtension(this string path, string newExtension)
        {
            string d = Path.GetDirectoryName(path);
            string n = Path.GetFileNameWithoutExtension(path);
            return FileUtil.CatDir(d, n + "." + newExtension);
        }
    }
}
