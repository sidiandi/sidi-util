using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Sidi.IO
{
    public class FileSystem
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static LPath GetTempFileName()
        {
            return new LPath(System.IO.Path.GetTempFileName());
        }

        internal static LPath GetTempPath()
        {
            return new LPath(System.IO.Path.GetTempPath());
        }

        public LPath GetRandomFileName()
        {
            return new LPath(System.IO.Path.GetRandomFileName());
        }

        internal static LPath GetNonExistingFileName(LPath lPath)
        {
            if (!lPath.Exists)
            {
                return lPath;
            }

            for (int i = 1; i < 1000; ++i)
            {
                var u = lPath.Parent.CatDir(LPath.JoinFileName(new string[] { lPath.FileNameWithoutExtension, i.ToString(), lPath.ExtensionWithoutDot }));
                if (!u.Exists)
                {
                    return u;
                }
            }
            throw new System.IO.IOException(String.Format("No non existing file name can be found for {0}.", lPath));
        }

        internal static LFileSystemInfo GetInfo(LPath path)
        {
            return new LFileSystemInfo(path.GetFullPath());
        }

        public LPath CurrentDirectory
        {
            get
            {
                return LDirectory.Current;
            }
        }

        internal IEnumerable<LFileSystemInfo> FindFile(LPath searchPath)
        {
            return LDirectory.FindFile(searchPath);
        }

        internal void DeleteFile(LPath path)
        {
            if (!NativeMethods.DeleteFile(path.Param))
            {
                new LFileSystemInfo(path).IsReadOnly = false;
                NativeMethods.DeleteFile(path.Param).CheckApiCall(path);
            }
            log.InfoFormat("Delete {0}", path);
        }

        internal static void CreateDirectory(LPath lPath)
        {
            LDirectory.Create(lPath);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public System.IO.FileStream Open(LPath path, System.IO.FileMode fileMode)
        {
            var desiredAccess = System.IO.FileAccess.ReadWrite;
            var shareMode = System.IO.FileShare.None;
            var lpSecurityAttributes = IntPtr.Zero;
            var creationDisposition = System.IO.FileMode.Open;
            var flagsAndAttributes = System.IO.FileAttributes.Normal;
            var hTemplateFile = IntPtr.Zero;
            var access = System.IO.FileAccess.Read;

            switch (fileMode)
            {
                case System.IO.FileMode.Create:
                    desiredAccess = System.IO.FileAccess.Write;
                    creationDisposition = System.IO.FileMode.Create;
                    access = System.IO.FileAccess.ReadWrite;
                    break;
                case System.IO.FileMode.Open:
                    desiredAccess = System.IO.FileAccess.Read;
                    creationDisposition = System.IO.FileMode.Open;
                    access = System.IO.FileAccess.Read;
                    break;
                default:
                    throw new NotImplementedException(fileMode.ToString());
            }

            SafeFileHandle h;
            try
            {
                h = NativeMethods.CreateFile(
                    path.Param,
                    desiredAccess,
                    shareMode,
                    lpSecurityAttributes,
                    creationDisposition,
                    flagsAndAttributes,
                    hTemplateFile);

                if (h.IsInvalid)
                {
                    throw new Win32Exception();
                }
            }
            catch (Win32Exception ex)
            {
                throw new System.IO.IOException(String.Format("Cannot open file: {0}", path), ex);
            }

            return new System.IO.FileStream(h, access);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public System.IO.FileStream Open(
            LPath path,
            System.IO.FileMode fileMode,
            System.IO.FileAccess fileAccess,
            System.IO.FileShare shareMode)
        {
            var lpSecurityAttributes = IntPtr.Zero;
            var creationDisposition = System.IO.FileMode.Open;
            var flagsAndAttributes = System.IO.FileAttributes.Normal;
            var hTemplateFile = IntPtr.Zero;
            var access = System.IO.FileAccess.Read;

            switch (fileMode)
            {
                case System.IO.FileMode.Create:
                    fileAccess = System.IO.FileAccess.Write;
                    creationDisposition = System.IO.FileMode.Create;
                    access = System.IO.FileAccess.ReadWrite;
                    break;
                case System.IO.FileMode.Open:
                    fileAccess = System.IO.FileAccess.Read;
                    creationDisposition = System.IO.FileMode.Open;
                    access = System.IO.FileAccess.Read;
                    break;
                default:
                    throw new NotImplementedException(fileMode.ToString());
            }

            SafeFileHandle h;
            try
            {
                h = NativeMethods.CreateFile(
                    path.Param,
                    fileAccess,
                    shareMode,
                    lpSecurityAttributes,
                    creationDisposition,
                    flagsAndAttributes,
                    hTemplateFile);

                if (h.IsInvalid)
                {
                    throw new Win32Exception();
                }
            }
            catch (Win32Exception ex)
            {
                throw new System.IO.IOException(String.Format("Cannot open file: {0}", path), ex);
            }

            return new System.IO.FileStream(h, access);
        }

        public System.IO.FileStream OpenWrite(LPath path)
        {
            try
            {
                return Open(path,
                    System.IO.FileMode.Create,
                    System.IO.FileAccess.ReadWrite,
                    System.IO.FileShare.Read);
            }
            catch (System.IO.IOException)
            {
                if (!path.Parent.IsDirectory)
                {
                    path.EnsureParentDirectoryExists();
                    return Open(path,
                        System.IO.FileMode.Create,
                        System.IO.FileAccess.ReadWrite,
                        System.IO.FileShare.Read);
                }
                else
                {
                    throw;
                }
            }
        }

        public System.IO.FileStream OpenRead(LPath path)
        {
            return Open(path,
                System.IO.FileMode.Open,
                System.IO.FileAccess.Read,
                System.IO.FileShare.ReadWrite);
        }

        public void DeleteDirectory(LPath lPath)
        {
            LDirectory.Delete(lPath);
        }
    }
}
