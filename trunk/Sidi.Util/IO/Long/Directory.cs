using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Sidi.IO.Long
{
    public class Directory
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const string PathSep = @"\";
        const string ThisDir = ".";
        const string UpDir = "..";

        public static void Delete(Path directory)
        {
            if (!Kernel32.RemoveDirectory(directory.Param))
            {
                new FileSystemInfo(directory).IsReadOnly = false;
                Kernel32.RemoveDirectory(directory.Param).CheckApiCall(directory);
            }
            log.InfoFormat("Delete {0}", directory);
        }

        public static bool Exists(Path directory)
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

        public static void Move(Path from, Path to)
        {
            Kernel32.MoveFileEx(from.Param, to.Param, 0).CheckApiCall(String.Format("{0} -> {1}", from, to));
        }

        /// <summary>
        /// Thin wrapper around FindFirstFile and FindNextFile. Also will return "." and ".."
        /// </summary>
        /// <param name="searchPath"></param>
        /// <returns></returns>
        internal static IEnumerable<FindData> FindFileRaw(Path searchPath)
        {
            FindData fd;

            using (var fh = Kernel32.FindFirstFile(searchPath.Param, out fd))
            {
                if (!fh.IsInvalid)
                {
                    yield return fd;
                    while (Kernel32.FindNextFile(fh, out fd))
                    {
                        yield return fd;
                    }
                }
            }
        }

        public static IList<FileSystemInfo> GetChilds(Path directory)
        {
            return FindFile(directory.CatDir("*")).ToList();
        }

        /// <summary>
        /// Enumerates found files. Make sure that the Enumerator is closed properly to free the Find handle.
        /// </summary>
        /// <param name="searchPath">File search path complete with wildcards, e.g. C:\temp\*.doc</param>
        /// <returns></returns>
        public static IEnumerable<FileSystemInfo> FindFile(Path searchPath)
        {
            return FindFileRaw(searchPath)
                .Where(x => !(x.Name.Equals(ThisDir) || x.Name.Equals(UpDir)))
                .Select(x => new FileSystemInfo(searchPath.ParentDirectory, x));
        }

        public static void Create(Path path)
        {
            CreateDirectoryInternal(path);
        }

        public static Path Current
        {
            get
            {
                return new Path(System.Environment.CurrentDirectory);
            }
        }

        const int ERROR_ALREADY_EXISTS = 183;
        const int ERROR_PATH_NOT_FOUND = 3;

        static void CreateDirectoryInternal(Path path)
        {
            if (!Kernel32.CreateDirectory(path.Param, IntPtr.Zero))
            {
                switch (Marshal.GetLastWin32Error())
                {
                    case ERROR_ALREADY_EXISTS:
                        return;
                    case ERROR_PATH_NOT_FOUND:
                        {
                            var p = path.ParentDirectory;
                            CreateDirectoryInternal(p);
                            Kernel32.CreateDirectory(path.Param, IntPtr.Zero).CheckApiCall(path);
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
