using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Sidi.Util;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;

namespace Sidi.IO
{
    public static class PathEx
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string RemoveLeadingPathSep(this string p)
        {
            if (p.StartsWith(new String(Path.DirectorySeparatorChar, 1)))
            {
                return p.Substring(1);
            }
            else
            {
                return p;
            }
        }

        public static string CatDir(params string[] dirs)
        {
            string p = String.Empty;
            foreach (string i in dirs)
            {
                if (!String.IsNullOrEmpty(i))
                {
                    p = Path.Combine(p, i);
                }
            }
            return p;
        }

        public const int MAX_PATH = 260;
        public const int MAX_ALTERNATE = 14;

        [StructLayout(LayoutKind.Sequential)]
        public struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct WIN32_FIND_DATA
        {
            public FileAttributes dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public int nFileSizeHigh;
            public int nFileSizeLow;
            public int dwReserved0;
            public int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ALTERNATE)]
            public string cAlternate;
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FindClose(IntPtr hFindFile);

        public static IEnumerable<string> Find(string d)
        {
            return Find(d, "*");
        }

        public static IEnumerable<string> Find(string d, string filter)
        {
            IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
            WIN32_FIND_DATA findData;

            d = Path.GetFullPath(d);

            IntPtr findHandle = FindFirstFile(Path.Combine(d, filter), out findData);
            if (findHandle != INVALID_HANDLE_VALUE)
            {
                do
                {
                    yield return Path.Combine(d, findData.cFileName);
                }
                while (FindNextFile(findHandle, out findData));
                FindClose(findHandle);
            }
        }

        public static void EnsureParentDirectoryExists(this string path)
        {
            string destinationDir = Directory.GetParent(path).FullName;
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }
        }

        public static void WriteAllText(string path, string text)
        {
            path.EnsureParentDirectoryExists();
            File.WriteAllText(path, text);
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

        public static string GetRelativePath(string path, string basePath)
        {
            path = Path.GetFullPath(path);
            basePath = Path.GetFullPath(basePath);
            List<string> result = new List<string>();

            string[] p = path.Split('\\');
            string[] b = basePath.Split('\\');

            int different = 0;
            for (different = 0; different < p.Length && different < b.Length; ++different)
            {
                if (!p[different].Equals(b[different]))
                {
                    break;
                }
            }

            for (int i = different; i < b.Length; ++i)
            {
                result.Add("..");
            }

            for (int i = different; i < p.Length; ++i)
            {
                result.Add(p[i]);
            }

            return String.Join(new String(Path.DirectorySeparatorChar, 1), result.ToArray());
        }

        public static string Bin(string relPath)
        {
            var uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            return FileUtil.CatDir(uri.LocalPath, "..", relPath); ;
        }

        public static IEnumerable<string> SplitDir(this string path)
        {
            return path.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
        }

        public static string CatDir(this IEnumerable<string> parts)
        {
            return FileUtil.CatDir(parts.ToArray());
        }

        public static string[] SplitCommaSeparatedList(this string list)
        {
            return list.Split(new char[] { ',' }).Select(x => x.Trim()).ToArray();
        }

        public static bool IsReadOnly(this string path)
        {
            return (new FileInfo(path).Attributes & FileAttributes.ReadOnly) != 0;
        }

        public static void SetReadOnly(this string path, bool readOnly)
        {
            FileInfo f = new FileInfo(path);
            if (readOnly)
            {
                f.Attributes = f.Attributes | FileAttributes.ReadOnly;
            }
            else
            {
                f.Attributes = f.Attributes & ~FileAttributes.ReadOnly;
            }
        }

        public static void WriteXml(this string path, object x)
        {
            DataContractSerializer s = new DataContractSerializer(x.GetType());
            using (var os = new FileStream(path, FileMode.Create))
            {
                s.WriteObject(os, x);
            }
        }

        public static T ReadXml<T>(this string path)
        {
            DataContractSerializer s = new DataContractSerializer(typeof(T));
            using (var os = File.OpenRead(path))
            {
                return (T)s.ReadObject(os);
            }
        }

        const string pathlistSeparator = ";";

        public static string[] SplitPathList(this string pathList)
        {
            if (pathList == null)
            {
                return new string[] { };
            }

            return pathList.Split(new string[] { pathlistSeparator }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string JoinPathList(this IEnumerable<string> paths)
        {
            return String.Join(pathlistSeparator, paths.ToArray());
        }


    }
}
