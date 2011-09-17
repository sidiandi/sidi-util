using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Sidi.Util;

namespace Sidi.IO.Long
{
    public class File
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Delete(LongName path)
        {
            if (!Kernel32.DeleteFile(path.Param))
            {
                new FileSystemInfo(path).ReadOnly = false;
                Kernel32.DeleteFile(path.Param).CheckApiCall(path);
            }
        }

        public static bool Exists(LongName path)
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

        public static System.IO.FileStream Open(LongName path, System.IO.FileMode fileMode)
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

        public static System.IO.StreamWriter TextWriter(LongName p)
        {
            return new System.IO.StreamWriter(Open(p, System.IO.FileMode.Create));
        }

        public static System.IO.StreamReader TextReader(LongName p)
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
        public static void Copy(LongName sourceFileName, LongName destFileName)
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
        public static void Copy(LongName sourceFileName, LongName destFileName, bool overwrite)
        {
            Kernel32.CopyFile(sourceFileName.Param, destFileName.Param, overwrite)
                .CheckApiCall(String.Format("{0} -> {1}", sourceFileName, destFileName));
        }

        public static void CreateHardLink(LongName fileName, LongName existingFileName)
        {
            Kernel32.CreateHardLink(fileName.Param, existingFileName.Param, IntPtr.Zero)
                .CheckApiCall(String.Format("{0} -> {1}", fileName, existingFileName));
        }

        public static bool EqualByTime(LongName f1, LongName f2)
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
    }
}
