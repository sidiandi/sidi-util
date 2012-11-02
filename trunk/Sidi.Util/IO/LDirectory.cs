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
                new FileSystemInfo(directory).IsReadOnly = false;
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

        public static IList<FileSystemInfo> GetChilds(LPath directory)
        {
            return FindFile(directory.CatDir("*")).ToList();
        }

        /// <summary>
        /// Enumerates found files. Make sure that the Enumerator is closed properly to free the Find handle.
        /// </summary>
        /// <param name="searchPath">File search path complete with wildcards, e.g. C:\temp\*.doc</param>
        /// <returns></returns>
        public static IEnumerable<FileSystemInfo> FindFile(LPath searchPath)
        {
            return FindFileRaw(searchPath)
                .Where(x => !(x.Name.Equals(ThisDir) || x.Name.Equals(UpDir)))
                .Select(x => new FileSystemInfo(searchPath.Parent, x));
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
