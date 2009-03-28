// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Sidi.IO
{
    public static class FileUtil
    {
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

            return String.Join(new String(System.IO.Path.DirectorySeparatorChar, 1), rel.ToArray());
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
    }
}
