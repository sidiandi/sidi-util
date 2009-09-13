using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Sidi.Util;
using System.Reflection;

namespace Sidi.IO
{
    public class PathEx
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PathEx()
        {
            CopyCondition = x => true;
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

        public void CopyToDir(IEnumerable<string> files, string destinationDir)
        {
            foreach (string s in files)
            {
                string d = Path.Combine(destinationDir, Path.GetFileName(s));
                FastCopy(s, d);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="destinationDir"></param>
        /// <returns>The destination path of the file</returns>
        public string CopyToDir(string s, string destinationDir)
        {
            string d = Path.Combine(destinationDir, Path.GetFileName(s));
            FastCopy(s, d);
            return d;
        }

        public bool FastCopy(string source, string dest)
        {
            CreateParentDirectory(dest);
            return FastCopyNoCreateDir(source, dest);
        }

        public Func<string, bool> CopyCondition { get; set; }

        /// <summary>
        /// Used by FastCopy to decide if a file must be copied or can be skipped
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public virtual bool CanSkipCopy(string source, string dest)
        {
            if (!CopyCondition(source))
            {
                return true;
            }

            FileInfo si = new FileInfo(source);
            FileInfo di = new FileInfo(dest);
            return !(
                !di.Exists ||
                si.LastWriteTime != di.LastWriteTime ||
                si.Length != di.Length);
        }

        public bool FastCopyNoCreateDir(string source, string dest)
        {
            FileInfo si = new FileInfo(source);
            FileInfo di = new FileInfo(dest);
            bool copied = false;

            if (!CanSkipCopy(source, dest))
            {
                if (di.Exists && OverwriteReadOnly)
                {
                    FileInfo destFileInfo = new FileInfo(dest);
                    if ((destFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        destFileInfo.Attributes = (FileAttributes)(destFileInfo.Attributes - FileAttributes.ReadOnly);
                    }
                }
                File.Copy(source, dest, true);
                if (MakeWritable)
                {
                    new FileInfo(dest).IsReadOnly = false;
                }

                log.InfoFormat("Copy {0} -> {1}", source.Printable(), dest.Printable());
                copied = true;
            }
            else
            {
                log.InfoFormat("Skip: {0} -> {1}", source.Printable(), dest.Printable());
            }
            return copied;
        }

        public bool OverwriteReadOnly
        {
            get;
            set;
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

        public bool MakeWritable { get; set; }

        public static void CreateParentDirectory(string path)
        {
            string destinationDir = Directory.GetParent(path).FullName;
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }
        }

        public void WriteAllText(string path, string text)
        {
            CreateParentDirectory(path);
            File.WriteAllText(path, text);
        }

        public void CopyRecursive(string source, string dest)
        {
            CopyRecursive(source, dest, 0);
        }

        public void CopyRecursive(string source, string dest, int numberOfCopiedDirectoryLevels)
        {
            string[] parts = source.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            if (parts.Length < numberOfCopiedDirectoryLevels)
            {
                throw new ArgumentOutOfRangeException("numberOfCopiedDirectoryLevels", numberOfCopiedDirectoryLevels, "must be equal or less the number of directories in source");
            }

            for (int i = 0; i < parts.Length; ++i)
            {
                if (i >= parts.Length - numberOfCopiedDirectoryLevels)
                {
                    dest = Path.Combine(dest, parts[i]);
                }
            }

            if (Directory.Exists(source))
            {
                DirectoryInfo d = new DirectoryInfo(source);

                if ((d.Attributes & FileAttributes.Hidden) != 0)
                {
                    return;
                }

                log.Info(d.FullName);
                foreach (FileSystemInfo i in d.GetFileSystemInfos())
                {
                    CopyRecursive(i.FullName, Path.Combine(dest, i.Name));
                }
            }
            else
            {
                FastCopy(source, dest);
            }
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

        /// <summary>
        /// Delete all files in directory dir. Does not recurse into sub directories.
        /// </summary>
        /// <param name="dir"></param>
        public void DeleteAllFilesIn(string dir)
        {
            if (Directory.Exists(dir))
            {
                log.InfoFormat("Deleting all files in {0}", dir);
                foreach (string i in Directory.GetFiles(dir))
                {
                    try
                    {
                        File.Delete(i);
                    }
                    catch (System.UnauthorizedAccessException)
                    {
                        // try harder
                        log.WarnFormat("Removing read-only protection from {0}", i);
                        FileInfo fi = new FileInfo(i);
                        fi.Attributes = fi.Attributes & (~FileAttributes.ReadOnly);
                        File.Delete(i);
                    }
                }
            }
        }

        public static string Bin(string relPath)
        {
            var uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            return FileUtil.CatDir(uri.LocalPath, "..", relPath); ;
        }
    }
}
