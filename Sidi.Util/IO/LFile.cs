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
using System.Linq;
using System.Text;
using System.ComponentModel;
using Sidi.Util;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Sidi.IO
{
    public class LFile
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Delete(LPath path)
        {
            if (!NativeMethods.DeleteFile(path.Param))
            {
                new LFileSystemInfo(path).IsReadOnly = false;
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
        public static System.IO.FileStream Open(
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

        /// <summary>
        /// Ensures that parent directory exists and opens file for writing
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static System.IO.FileStream OpenWrite(LPath path)
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

        public static System.IO.FileStream OpenRead(LPath path)
        {
            return Open(path,
                System.IO.FileMode.Open,
                System.IO.FileAccess.Read,
                System.IO.FileShare.ReadWrite);
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
            Copy(sourceFileName, destFileName, overwrite, (p) => { log.InfoFormat("{0}: {1} -> {2}", p, sourceFileName, destFileName);  });
        }

        public static void Copy(
            LPath sourceFileName,
            LPath destFileName,
            bool overwrite,
            Action<CopyProgress> progressCallback)
        {
            Int32 pbCancel = 0;
            var progress = new CopyProgress(sourceFileName, destFileName);

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
                        progress.Progress.Total = TotalFileSize;
                        if (progress.Progress.Update(TotalBytesTransferred))
                        {
                            progressCallback(progress);
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
                if (LPath.IsSameFileSystem(source, destination))
                {
                    CreateHardLink(destination, source);
                }
                else
                {
                    LFile.Copy(source, destination);
                }
            }
        }

        public static bool EqualByTimeAndLength(LPath f1, LPath f2)
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

        public static bool EqualByTimeAndLength(LPath f1, LPath f2, TimeSpan maxTimeDifference)
        {
            FindData d1;
            FindData d2;
            if (f1.GetFindData(out d1) && f2.GetFindData(out d2))
            {
                if (d1.Length == d2.Length)
                {
                    var d = d1.ftLastWriteTime.DateTimeUtc - d2.ftLastWriteTime.DateTimeUtc;
                    return Math.Abs(d.TotalSeconds) <= maxTimeDifference.TotalSeconds;
                }
                else
                {
                    return false;
                }
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
