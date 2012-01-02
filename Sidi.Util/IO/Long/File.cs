﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Sidi.Util;
using System.Runtime.InteropServices;

namespace Sidi.IO.Long
{
    public class File
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Delete(Path path)
        {
            if (!Kernel32.DeleteFile(path.Param))
            {
                new FileSystemInfo(path).IsReadOnly = false;
                Kernel32.DeleteFile(path.Param).CheckApiCall(path);
            }
            log.InfoFormat("Delete {0}", path);
        }

        public static void WriteAllText(Path path, string contents)
        {
            using (var w = StreamWriter(path))
            {
                w.Write(contents);
            }
        }

        public static string ReadAllText(Path path)
        {
            using (var r = StreamReader(path))
            {
                return r.ReadToEnd();
            }
        }

        public static System.IO.StreamReader StreamReader(Path path)
        {
            var s = Open(path, System.IO.FileMode.Open);
            return new System.IO.StreamReader(s);
        }

        public static System.IO.StreamWriter StreamWriter(Path path)
        {
            path.EnsureParentDirectoryExists();
            var s = Open(path, System.IO.FileMode.Create);
            return new System.IO.StreamWriter(s);
        }

        public static bool Exists(Path path)
        {
            using (var f = Directory.FindFile(path).GetEnumerator())
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

        public static System.IO.FileStream Open(Path path, System.IO.FileMode fileMode)
        {
            Kernel32.EFileAccess dwDesiredAccess = Kernel32.EFileAccess.GenericAll;
            Kernel32.EFileShare dwShareMode = Kernel32.EFileShare.None;
            IntPtr lpSecurityAttributes = IntPtr.Zero;
            Kernel32.ECreationDisposition dwCreationDisposition = Kernel32.ECreationDisposition.OpenExisting;
            Kernel32.EFileAttributes dwFlagsAndAttributes = 0;
            IntPtr hTemplateFile = IntPtr.Zero;
            System.IO.FileAccess access = System.IO.FileAccess.Read;

            switch (fileMode)
            {
                case System.IO.FileMode.Create:
                    dwDesiredAccess = Kernel32.EFileAccess.GenericWrite;
                    dwCreationDisposition = Kernel32.ECreationDisposition.CreateAlways;
                    access = System.IO.FileAccess.ReadWrite;
                    break;
                case System.IO.FileMode.Open:
                    dwDesiredAccess = Kernel32.EFileAccess.GenericRead;
                    dwCreationDisposition = Kernel32.ECreationDisposition.OpenExisting;
                    access = System.IO.FileAccess.Read;
                    break;
            }

            var h = Kernel32.CreateFile(path.Param, dwDesiredAccess,
                dwShareMode, lpSecurityAttributes, dwCreationDisposition,
                dwFlagsAndAttributes,
                hTemplateFile);

            return new System.IO.FileStream(h, access);
        }

        public static System.IO.StreamWriter TextWriter(Path p)
        {
            return new System.IO.StreamWriter(Open(p, System.IO.FileMode.Create));
        }

        public static System.IO.StreamReader TextReader(Path p)
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
        public static void Copy(Path sourceFileName, Path destFileName)
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
        public static void Copy(Path sourceFileName, Path destFileName, bool overwrite)
        {
            Kernel32.CopyFile(sourceFileName.Param, destFileName.Param, overwrite)
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
        public static void Move(Path sourceFileName, Path destFileName)
        {
            Kernel32.MoveFileEx(sourceFileName.Param, destFileName.Param, 0)
                .CheckApiCall(String.Format("{0} -> {1}", sourceFileName, destFileName));
        }

        public static void CreateHardLink(Path fileName, Path existingFileName)
        {
            Kernel32.CreateHardLink(fileName.Param, existingFileName.Param, IntPtr.Zero)
                .CheckApiCall(String.Format("{0} -> {1}", fileName, existingFileName));
        }

        public static void CopyOrHardLink(Path source, Path dest)
        {
            if (!dest.Exists)
            {
                dest.EnsureParentDirectoryExists();
                if (source.PathRoot.Equals(dest.PathRoot))
                {
                    CreateHardLink(dest, source);
                }
                else
                {
                    File.Copy(source, dest);
                }
            }
        }

        public static bool EqualByTime(Path f1, Path f2)
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

        [DllImport("msvcrt.dll")]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        static bool Equals(byte[] b1, byte[] b2, int count)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return memcmp(b1, b2, count) == 0;
        }

        public static bool EqualByContent(Path f1, Path f2)
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
