using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Sidi.Util;
using System.Runtime.InteropServices;

namespace Sidi.IO
{
    public class LFile
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Delete(LPath path)
        {
            if (!NativeMethods.DeleteFile(path.Param))
            {
                new FileSystemInfo(path).IsReadOnly = false;
                NativeMethods.DeleteFile(path.Param).CheckApiCall(path);
            }
            log.InfoFormat("Delete {0}", path);
        }

        public static void WriteAllText(LPath path, string contents)
        {
            using (var w = StreamWriter(path))
            {
                w.Write(contents);
            }
        }

        public static string ReadAllText(LPath path)
        {
            using (var r = StreamReader(path))
            {
                return r.ReadToEnd();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static System.IO.StreamReader StreamReader(LPath path)
        {
            var s = OpenRead(path);
            return new System.IO.StreamReader(s);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static System.IO.StreamWriter StreamWriter(LPath path)
        {
            return new System.IO.StreamWriter(OpenWrite(path));
        }

        public static bool Exists(LPath path)
        {
            using (var f = LDirectory.FindFile(path).GetEnumerator())
            {
                if (f.MoveNext())
                {
                    return ((f.Current.Attributes & System.IO.FileAttributes.Directory) == 0);
                }
                else
                {
                    return false;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static System.IO.FileStream Open(LPath path, System.IO.FileMode fileMode)
        {
            NativeMethods.EFileAccess dwDesiredAccess = NativeMethods.EFileAccess.GenericAll;
            NativeMethods.EFileShare dwShareMode = NativeMethods.EFileShare.None;
            IntPtr lpSecurityAttributes = IntPtr.Zero;
            NativeMethods.ECreationDisposition dwCreationDisposition = NativeMethods.ECreationDisposition.OpenExisting;
            NativeMethods.EFileAttributes dwFlagsAndAttributes = 0;
            IntPtr hTemplateFile = IntPtr.Zero;
            System.IO.FileAccess access = System.IO.FileAccess.Read;

            switch (fileMode)
            {
                case System.IO.FileMode.Create:
                    dwDesiredAccess = NativeMethods.EFileAccess.GenericWrite;
                    dwCreationDisposition = NativeMethods.ECreationDisposition.CreateAlways;
                    access = System.IO.FileAccess.ReadWrite;
                    break;
                case System.IO.FileMode.Open:
                    dwDesiredAccess = NativeMethods.EFileAccess.GenericRead;
                    dwCreationDisposition = NativeMethods.ECreationDisposition.OpenExisting;
                    access = System.IO.FileAccess.Read;
                    break;
                default:
                    throw new NotImplementedException(fileMode.ToString());
            }

            var h = NativeMethods.CreateFile(path.Param, dwDesiredAccess,
                dwShareMode, lpSecurityAttributes, dwCreationDisposition,
                dwFlagsAndAttributes,
                hTemplateFile);

            return new System.IO.FileStream(h, access);
        }

        public static System.IO.FileStream OpenWrite(LPath path)
        {
            path.EnsureParentDirectoryExists();
            return Open(path, System.IO.FileMode.Create);
        }

        public static System.IO.FileStream OpenRead(LPath path)
        {
            return Open(path, System.IO.FileMode.Open);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static System.IO.StreamWriter TextWriter(LPath p)
        {
            return new System.IO.StreamWriter(Open(p, System.IO.FileMode.Create));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static System.IO.StreamReader TextReader(LPath p)
        {
            return new System.IO.StreamReader(Open(p, System.IO.FileMode.Open));
        }

        //
        // Summary:
        //     Copies an existing file to a new file. Overwriting a file of the same name
        //     is not allowed.
        //
        // Parameters:
        //   sourceFileName:
        //     The file to copy.
        //
        //   destFileName:
        //     The name of the destination file. This cannot be a directory or an existing
        //     file.
        //
        // Exceptions:
        //   System.UnauthorizedAccessException:
        //     The caller does not have the required permission.
        //
        //   System.ArgumentException:
        //     sourceFileName or destFileName is a zero-length string, contains only white
        //     space, or contains one or more invalid characters as defined by System.IO.Path.InvalidPathChars.-or-
        //     sourceFileName or destFileName specifies a directory.
        //
        //   System.ArgumentNullException:
        //     sourceFileName or destFileName is null.
        //
        //   System.IO.PathTooLongException:
        //     The specified path, file name, or both exceed the system-defined maximum
        //     length. For example, on Windows-based platforms, paths must be less than
        //     248 characters, and file names must be less than 260 characters.
        //
        //   System.IO.DirectoryNotFoundException:
        //     The path specified in sourceFileName or destFileName is invalid (for example,
        //     it is on an unmapped drive).
        //
        //   System.IO.FileNotFoundException:
        //     sourceFileName was not found.
        //
        //   System.IO.IOException:
        //     destFileName exists.-or- An I/O error has occurred.
        //
        //   System.NotSupportedException:
        //     sourceFileName or destFileName is in an invalid format.
        public static void Copy(LPath sourceFileName, LPath destFileName)
        {
            Copy(sourceFileName, destFileName, false);
        }

        //
        // Summary:
        //     Copies an existing file to a new file. Overwriting a file of the same name
        //     is allowed.
        //
        // Parameters:
        //   sourceFileName:
        //     The file to copy.
        //
        //   destFileName:
        //     The name of the destination file. This cannot be a directory.
        //
        //   overwrite:
        //     true if the destination file can be overwritten; otherwise, false.
        //
        // Exceptions:
        //   System.UnauthorizedAccessException:
        //     The caller does not have the required permission. -or-destFileName is read-only.
        //
        //   System.ArgumentException:
        //     sourceFileName or destFileName is a zero-length string, contains only white
        //     space, or contains one or more invalid characters as defined by System.IO.Path.InvalidPathChars.-or-
        //     sourceFileName or destFileName specifies a directory.
        //
        //   System.ArgumentNullException:
        //     sourceFileName or destFileName is null.
        //
        //   System.IO.PathTooLongException:
        //     The specified path, file name, or both exceed the system-defined maximum
        //     length. For example, on Windows-based platforms, paths must be less than
        //     248 characters, and file names must be less than 260 characters.
        //
        //   System.IO.DirectoryNotFoundException:
        //     The path specified in sourceFileName or destFileName is invalid (for example,
        //     it is on an unmapped drive).
        //
        //   System.IO.FileNotFoundException:
        //     sourceFileName was not found.
        //
        //   System.IO.IOException:
        //     destFileName exists and overwrite is false.-or- An I/O error has occurred.
        //
        //   System.NotSupportedException:
        //     sourceFileName or destFileName is in an invalid format.
        public static void Copy(LPath sourceFileName, LPath destFileName, bool overwrite)
        {
            Copy(sourceFileName, destFileName, overwrite, (p) =>
                {
                    log.Info(p.Message);
                });
        }

        static TimeSpan progressInterval = TimeSpan.FromSeconds(1);

        public static void Copy(
            LPath sourceFileName,
            LPath destFileName,
            bool overwrite,
            Action<CopyProgress> progressCallback)
        {
            Int32 pbCancel = 0;
            var progress = new CopyProgress(sourceFileName, destFileName);

            var nextProgress = DateTime.Now + progressInterval;

            NativeMethods.CopyFileEx(
                sourceFileName.Param,
                destFileName.Param,
                new NativeMethods.CopyProgressRoutine(
                    (long TotalFileSize,
            long TotalBytesTransferred,
            long StreamSize,
            long StreamBytesTransferred,
            uint dwStreamNumber,
            NativeMethods.CopyProgressCallbackReason dwCallbackReason,
            IntPtr hSourceFile,
            IntPtr hDestinationFile,
            IntPtr lpData) =>
                    {
                        var n = DateTime.Now;
                        if (n > nextProgress)
                        {
                            progress.Update(TotalBytesTransferred, TotalFileSize);
                            progressCallback(progress);
                            nextProgress = n + progressInterval;
                        }
                        return NativeMethods.CopyProgressResult.PROGRESS_CONTINUE;
                    }),
            IntPtr.Zero,
           ref pbCancel,
           overwrite ? 0 : NativeMethods.CopyFileFlags.COPY_FILE_FAIL_IF_EXISTS)
            .CheckApiCall(String.Format("{0} -> {1}", sourceFileName, destFileName));
        }

        //
        // Summary:
        //     Moves a specified file to a new location, providing the option to specify
        //     a new file name.
        //
        // Parameters:
        //   sourceFileName:
        //     The name of the file to move.
        //
        //   destFileName:
        //     The new path for the file.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     The destination file already exists.
        //
        //   System.ArgumentNullException:
        //     sourceFileName or destFileName is null.
        //
        //   System.ArgumentException:
        //     sourceFileName or destFileName is a zero-length string, contains only white
        //     space, or contains invalid characters as defined in System.IO.Path.InvalidPathChars.
        //
        //   System.UnauthorizedAccessException:
        //     The caller does not have the required permission.
        //
        //   System.IO.FileNotFoundException:
        //     sourceFileName was not found.
        //
        //   System.IO.PathTooLongException:
        //     The specified path, file name, or both exceed the system-defined maximum
        //     length. For example, on Windows-based platforms, paths must be less than
        //     248 characters, and file names must be less than 260 characters.
        //
        //   System.IO.DirectoryNotFoundException:
        //     The path specified in sourceFileName or destFileName is invalid, (for example,
        //     it is on an unmapped drive).
        //
        //   System.NotSupportedException:
        //     sourceFileName or destFileName is in an invalid format.
        public static void Move(LPath sourceFileName, LPath destFileName)
        {
            NativeMethods.MoveFileEx(sourceFileName.Param, destFileName.Param, 0)
                .CheckApiCall(String.Format("{0} -> {1}", sourceFileName, destFileName));
        }

        public static void CreateHardLink(LPath fileName, LPath existingFileName)
        {
            NativeMethods.CreateHardLink(fileName.Param, existingFileName.Param, IntPtr.Zero)
                .CheckApiCall(String.Format("{0} -> {1}", fileName, existingFileName));
        }

        public static void CopyOrHardLink(LPath source, LPath destination)
        {
            if (!destination.Exists)
            {
                destination.EnsureParentDirectoryExists();
                if (source.PathRoot.Equals(destination.PathRoot))
                {
                    CreateHardLink(destination, source);
                }
                else
                {
                    LFile.Copy(source, destination);
                }
            }
        }

        public static bool EqualByTime(LPath f1, LPath f2)
        {
            FindData d1;
            FindData d2;
            if (f1.GetFindData(out d1) && f2.GetFindData(out d2))
            {
                return d1.Length == d2.Length && d1.ftLastWriteTime.Equals(d2.ftLastWriteTime);
            }
            else
            {
                return false;
            }
        }

        static bool Equals(byte[] b1, byte[] b2, int count)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return NativeMethods.memcmp(b1, b2, count) == 0;
        }

        public static bool EqualByContent(LPath f1, LPath f2)
        {
            FindData d1;
            FindData d2;
            if (!(f1.GetFindData(out d1) && f2.GetFindData(out d2)))
            {
                return false;
            }

            if (d1.Length != d2.Length)
            {
                return false;
            }

            const int bufSize = 0x1000;
            byte[] b1 = new byte[bufSize];
            byte[] b2 = new byte[bufSize];

            using (var s1 = Open(f1, System.IO.FileMode.Open))
            {
                using (var s2 = Open(f2, System.IO.FileMode.Open))
                {
                    int readCount = s1.Read(b1, 0, bufSize);
                    if (readCount != s2.Read(b2, 0, bufSize))
                    {
                        return false;
                    }

                    if (!Equals(b1, b2, readCount))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
