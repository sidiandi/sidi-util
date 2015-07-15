using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sidi.IO.Windows
{
    /// <summary>
    /// Operations that access the Windows file system
    /// </summary>
    internal class FileSystem : Sidi.IO.IFileSystem
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static FileSystem Current
        {
            get
            {
                if (current == null)
                {
                    current = new FileSystem();
                }
                return current;
            }
        }

        static FileSystem current = null;

        /// <summary>
        /// Create a hardlink at fileName that points to existingFileName
        /// </summary>
        /// <param name="fileName">Path of the newly created link</param>
        /// <param name="existingFileName">Path of the existing file to which the link will point</param>
        public void CreateHardLink(LPath fileName, LPath existingFileName)
        {
            NativeMethods.CreateHardLink(GetLongPathApiParameter(fileName), GetLongPathApiParameter(existingFileName), IntPtr.Zero)
                .CheckApiCall(String.Format("Cannot create hard link: {0} -> {1}", fileName, existingFileName));
        }

        public IHardLinkInfo GetHardLinkInfo(LPath path)
        {
            return HardLinkInfo.Get(path);
        }

        const int ERROR_ALREADY_EXISTS = 183;
        const int ERROR_PATH_NOT_FOUND = 3;

        /// <summary>
        /// Creates a directory at path. Will create sub directories if required. Will not throw an exception if the directory already exists
        /// </summary>
        /// <param name="directory"></param>
        public void EnsureDirectoryExists(LPath directory)
        {
            if (!NativeMethods.CreateDirectory(GetLongPathApiParameter(directory), IntPtr.Zero))
            {
                switch (Marshal.GetLastWin32Error())
                {
                    case ERROR_ALREADY_EXISTS:
                        {
                            if (!this.GetInfo(directory).IsDirectory)
                            {
                                throw new System.IO.IOException(String.Format("Cannot create a directory at {0} because a file with that name already exists.", directory));
                            }
                            break;
                        }
                    case ERROR_PATH_NOT_FOUND:
                        {
                            var p = directory.Parent;
                            EnsureDirectoryExists(p);
                            EnsureDirectoryExists(directory);
                        }
                        break;
                    default:
                        false.CheckApiCall(directory);
                        break;
                }
            }
        }

        public void RemoveDirectory(LPath directory)
        {
            NativeMethods.RemoveDirectory(GetLongPathApiParameter(directory)).CheckApiCall(directory);
        }

        const string longPrefix = @"\\?\";
        const string longUncPrefix = @"\\?\UNC\";
        public const string DirectorySeparator = @"\";

        /// <summary>
        /// Returns a string representation of path that can be used to reference long paths and file names in the native Windows API.
        /// Example: C:\ will return
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetLongPathApiParameter(LPath path)
        {
            var p = GetLongPathApiParameterImpl(path);
            //log.Info(p);
            return p;
        }

        const string devicePrefix = @"\\.\";
        
        string GetLongPathApiParameterImpl(LPath path)
        {
            if (path.IsUnc)
            {
                return longUncPrefix + path.Server + LPath.DirectorySeparator + path.Share + LPath.DirectorySeparator + String.Join(DirectorySeparator, path.Parts);
            }
            else if (object.Equals(path.Prefix, devicePrefix))
            {
                return path.StringRepresentation;
            }
            else
            {
                return longPrefix + path.StringRepresentation;
            }
        }

        /// <summary>
        /// Move a file system item from existingFileName to newFileName
        /// </summary>
        /// <param name="existingFileName"></param>
        /// <param name="newFileName"></param>
        public void Move(LPath existingFileName, LPath newFileName)
        {
            NativeMethods.MoveFileEx(GetLongPathApiParameter(existingFileName), GetLongPathApiParameter(newFileName), 0).CheckApiCall(String.Format("{0} -> {1}", existingFileName, newFileName));
        }

        const string ThisDir = ".";
        const string UpDir = "..";

        public IFileSystemInfo GetInfo(LPath lPath)
        {
            return new FileSystemInfo(this, lPath.GetFullPath());
        }

        /// <summary>
        /// Enumerates found files. Make sure that the Enumerator is closed properly to free the Find handle.
        /// </summary>
        /// <param name="searchPath">File search path complete with wildcards, e.g. C:\temp\*.doc</param>
        /// <returns></returns>
        public IEnumerable<IFileSystemInfo> FindFile(LPath searchPath)
        {
            var p = searchPath.Parent;
            return FindFileRaw(searchPath)
                .Where(x => !(x.cFileName.Equals(ThisDir) || x.cFileName.Equals(UpDir)))
                .Select(x => new FileSystemInfo(this, p, x));
        }

        class RawEnumerable : IEnumerable<WIN32_FIND_DATA>
        {
            readonly string fileName;

            public RawEnumerable(string fileName)
            {
                this.fileName = fileName;
            }

            public IEnumerator<WIN32_FIND_DATA> GetEnumerator()
            {
                return new RawEnumerator(fileName);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return new RawEnumerator(fileName);
            }
        }

        class RawEnumerator : IEnumerator<WIN32_FIND_DATA>
        {
            FindHandle findHandle;
            readonly string fileName;
            WIN32_FIND_DATA current;

            public RawEnumerator(string fileName)
            {
                this.fileName = fileName;
            }

            public WIN32_FIND_DATA Current
            {
                get { return current; }
            }

            public void Dispose()
            {
                findHandle.Dispose();
            }

            object System.Collections.IEnumerator.Current
            {
                get { throw new NotImplementedException(); }
            }

            public bool MoveNext()
            {
                if (findHandle == null)
                {
                    findHandle = NativeMethods.FindFirstFile(fileName, out current);
                    return !findHandle.IsInvalid;
                }
                else if (!findHandle.IsInvalid)
                {
                    return NativeMethods.FindNextFile(findHandle, out current);
                }

                throw new InvalidOperationException();
            }

            public void Reset()
            {
                if (findHandle != null)
                {
                    findHandle.Dispose();
                }
            }
        }

        /// <summary>
        /// Thin wrapper around FindFirstFile and FindNextFile. Also will return "." and ".."
        /// </summary>
        /// <param name="searchPath"></param>
        /// <returns></returns>
        private IEnumerable<WIN32_FIND_DATA> FindFileRaw(LPath searchPath)
        {
            return new RawEnumerable(GetLongPathApiParameter(searchPath));
        }

        internal bool GetFindData(LPath path, out WIN32_FIND_DATA fd)
        {
            if (path.IsRoot)
            {
                if (System.IO.Directory.Exists(path.StringRepresentation))
                {
                    fd = new WIN32_FIND_DATA()
                    {
                        dwFileAttributes = (uint) System.IO.FileAttributes.Directory,
                        nFileSizeHigh = 0,
                        nFileSizeLow = 0,
                        cFileName = path.ToString(),
                    };
                    return true;
                }
                else
                {
                    fd = default(WIN32_FIND_DATA);
                    return false;
                }
            }

            using (var f = FindFileRaw(path).GetEnumerator())
            {
                if (f.MoveNext())
                {
                    fd = f.Current;
                    return true;
                }
                else
                {
                    fd = default(WIN32_FIND_DATA);
                    return false;
                }
            }
        }

        /// <summary>
        /// Opens a file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileMode"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public System.IO.FileStream Open(LPath fileName, System.IO.FileMode fileMode)
        {
            var desiredAccess = System.IO.FileAccess.ReadWrite;
            var shareMode = System.IO.FileShare.None;
            var lpSecurityAttributes = IntPtr.Zero;
            var creationDisposition = System.IO.FileMode.Open;
            var flagsAndAttributes = System.IO.FileAttributes.Normal;
            var hTemplateFile = IntPtr.Zero;

            switch (fileMode)
            {
                case System.IO.FileMode.Create:
                    desiredAccess = System.IO.FileAccess.Write;
                    creationDisposition = System.IO.FileMode.Create;
                    break;
                case System.IO.FileMode.Open:
                    desiredAccess = System.IO.FileAccess.Read;
                    creationDisposition = System.IO.FileMode.Open;
                    break;
                default:
                    throw new NotImplementedException(fileMode.ToString());
            }

            if (fileName.Prefix.Equals(@"\\.\"))
            {
                shareMode = System.IO.FileShare.Write;
                desiredAccess = System.IO.FileAccess.Read;
            }

            SafeFileHandle h;
            try
            {
                h = NativeMethods.CreateFile(
                    GetLongPathApiParameter(fileName),
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
                throw new System.IO.IOException(String.Format("Cannot open file: {0}", fileName), ex);
            }

            return new System.IO.FileStream(h, desiredAccess);
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

            switch (fileMode)
            {
                case System.IO.FileMode.Create:
                    creationDisposition = System.IO.FileMode.Create;
                    break;
                case System.IO.FileMode.Open:
                    creationDisposition = System.IO.FileMode.Open;
                    break;
                case System.IO.FileMode.CreateNew:
                    creationDisposition = System.IO.FileMode.CreateNew;
                    break;
                default:
                    throw new NotImplementedException(fileMode.ToString());
            }

            SafeFileHandle h;
            try
            {
                h = NativeMethods.CreateFile(
                    GetLongPathApiParameter(path),
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

            return new System.IO.FileStream(h, fileAccess);
        }

        /// <summary>
        /// Delete a files
        /// Precondition: fileName is a file
        /// Postcondiction: fileName does not exist
        /// </summary>
        /// <param name="fileName">File to be deleted.</param>
        public void DeleteFile(LPath fileName)
        {
            NativeMethods.DeleteFile(GetLongPathApiParameter(fileName)).CheckApiCall(fileName);
        }

        public LPath GetVolumePath(LPath path)
        {
            var sb = new StringBuilder(LPath.MaxPathLength);
            NativeMethods.GetVolumePathName(GetLongPathApiParameter(path), sb, (uint)sb.Capacity).CheckApiCall(path);
            return new LPath(sb.ToString());
        }

        public void CopyFile(
            LPath existingFileName, 
            LPath newFileName, 
            IProgress<CopyFileProgress> progress = null, 
            CancellationToken ct = new CancellationToken(), 
            CopyFileOptions options = null)
        {
            Int32 pbCancel = 0;

            if (options == null)
            {
                options = new CopyFileOptions();
            }

            var begin = DateTime.UtcNow;

            var progressCallback = progress != null ? new NativeMethods.CopyProgressRoutine(
                (
                    long TotalFileSize,
                    long TotalBytesTransferred,
                    long StreamSize,
                    long StreamBytesTransferred,
                    uint dwStreamNumber,
                    NativeMethods.CopyProgressCallbackReason dwCallbackReason,
                    IntPtr hSourceFile,
                    IntPtr hDestinationFile,
                    IntPtr lpData
                ) =>
                {
                    progress.Report(new CopyFileProgress(
                        begin,
                        TotalFileSize,
                        TotalBytesTransferred,
                        StreamSize,
                        StreamBytesTransferred,
                        (int) dwStreamNumber));

                    if (ct.IsCancellationRequested)
                    {
                        return NativeMethods.CopyProgressResult.PROGRESS_CANCEL;
                    }
                    else
                    {
                        return NativeMethods.CopyProgressResult.PROGRESS_CONTINUE;
                    }
                }) : null;

            NativeMethods.CopyFileEx(
                GetLongPathApiParameter(existingFileName),
                GetLongPathApiParameter(newFileName),
                progressCallback,
                IntPtr.Zero,
                ref pbCancel,
                (options.AllowDecryptedDestination ? NativeMethods.CopyFileFlags.COPY_FILE_ALLOW_DECRYPTED_DESTINATION : 0) |
                (options.CopySymlink ? NativeMethods.CopyFileFlags.COPY_FILE_COPY_SYMLINK : 0) |
                (options.FailIfExists ? NativeMethods.CopyFileFlags.COPY_FILE_FAIL_IF_EXISTS : 0) |
                (options.NoBuffering ? NativeMethods.CopyFileFlags.COPY_FILE_NO_BUFFERING : 0) |
                (options.OpenSourceForWrite ? NativeMethods.CopyFileFlags.COPY_FILE_OPEN_SOURCE_FOR_WRITE : 0) |
                (options.Restartable ? NativeMethods.CopyFileFlags.COPY_FILE_RESTARTABLE : 0))
                .CheckApiCall(String.Format("copy from {0} to {1}", existingFileName, newFileName));
        }

        public LPath CurrentDirectory
        {
            get
            {
                return new LPath(System.Environment.CurrentDirectory);
            }

            set
            {
                System.Environment.CurrentDirectory = value.StringRepresentation;
            }
        }

        public void SetFileAttribute(LPath path, System.IO.FileAttributes value)
        {
            NativeMethods.SetFileAttributes(GetLongPathApiParameter(path), value).CheckApiCall(path);
        }

        public IEnumerable<LPath> GetDrives()
        {
            return System.IO.DriveInfo.GetDrives().Select(_ => new LPath(_.RootDirectory.FullName));
        }

        public IEnumerable<LPath> GetAvailableDrives()
        {
            for (char letter = 'A'; letter <= 'Z'; ++letter)
            {
                var d = LPath.GetDriveRoot(letter);
                if (!d.Exists)
                {
                    yield return d;
                }
            }
        }
    }
}
