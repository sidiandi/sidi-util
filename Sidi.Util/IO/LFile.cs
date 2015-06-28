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
using System.Threading;
using Sidi.Extensions;

namespace Sidi.IO
{
    [Obsolete("Use IFileSystem and LPath methods")]
    public class LFile
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Delete(LPath path)
        {
            path.DeleteFile();
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static System.IO.FileStream Open(LPath path, System.IO.FileMode fileMode)
        {
            return path.FileSystem.Open(path, fileMode);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static System.IO.FileStream Open(
            LPath fileName, 
            System.IO.FileMode fileMode, 
            System.IO.FileAccess fileAccess,
            System.IO.FileShare shareMode)
        {
            return fileName.FileSystem.Open(fileName, fileMode, fileAccess, shareMode);
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
            var progress = new CopyProgress(sourceFileName, destFileName);

            var p = new Progress<CopyFileProgress>();
            p.ProgressChanged += (s,e) =>
                {
                    progress.Progress.Total = e.TotalFileSize;
                    if (progress.Progress.Update(e.TotalBytesTransferred))
                    {
                        progressCallback(progress);
                    }
                };

            CancellationToken ct;
            sourceFileName.CopyFile(destFileName, null, ct, new CopyFileOptions { FailIfExists = !overwrite });
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
            sourceFileName.Move(destFileName);
        }

        public static void CreateHardLink(LPath fileName, LPath existingFileName)
        {
            existingFileName.CreateHardLink(fileName);
        }

        public static bool EqualByTimeAndLength(params LPath[] files)
        {
            return EqualByTimeAndLength(TimeSpan.Zero, files);
        }

        public static bool EqualByTimeAndLength(TimeSpan maxTimeDifference, params LPath[] files)
        {
            var info = files.Select(_ => _.Info).ToArray();

            if (!info.All(_=>_.IsFile))
            {
                return false;
            }

            var f = info.First();
            if (!info.Skip(1).All(_ => _.Length == f.Length))
            {
                return false;
            }

            var timeDifference = (info.Max(_=>_.LastWriteTimeUtc) - info.Min(_=>_.LastWriteTimeUtc));
            if (timeDifference <= maxTimeDifference)
            {
                return true;
            }
            else
            {
                log.Info(timeDifference);
                return false;
            }
        }

        static bool Equals(byte[] b1, byte[] b2, int count)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return Sidi.IO.Windows.NativeMethods.memcmp(b1, b2, count) == 0;
        }

        public static bool EqualByContent(LPath f1, LPath f2)
        {
            return FileCompare.EqualByContent(f1, f2);
        }

        public static bool Exists(LPath filename)
        {
            return filename.IsFile;
        }
    }
}
