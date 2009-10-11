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
using System.Runtime.InteropServices;

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
            return Sibling(new Uri(assembly.CodeBase).LocalPath, siblingName);
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

        /// <summary>
        /// Deletes also write-protected files
        /// </summary>
        /// <param name="path"></param>
        public static void ForceDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (System.UnauthorizedAccessException)
            {
                path.SetReadOnly(false);
                File.Delete(path);
            }
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
                if (!String.IsNullOrEmpty(i))
                {
                    r = System.IO.Path.Combine(r, i);
                }
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

        public static void CreateHardLink(
                string fileName,
                string existingFileName)
        {
            bool result = CreateHardLink(fileName, existingFileName, IntPtr.Zero);
            if (!result)
            {
                throw new System.IO.IOException(String.Format("Cannot create hard link: {0} -> {1}", fileName, existingFileName));
            }
        }

        [DllImport("Kernel32.dll")]
        private static extern
            bool CreateHardLink(
                string FileName,
                string ExistingFileName,
                IntPtr lpSecurityAttributes
            );
    }
}
