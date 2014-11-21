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
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;

namespace Sidi.IO
{
    internal static class NativeMethods
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [DllImport("msvcrt.dll")]
        internal static extern int memcmp(byte[] b1, byte[] b2, long count);

        internal static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        internal static int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

        public const int MAX_PATH = 260;

        public static void CheckApiCall(this bool result, LPath path)
        {
            CheckApiCall(result, path.ToString());
        }

        public const int ERROR_REQUEST_ABORTED = 1235;
        
        public static void CheckApiCall(this bool result, string message)
        {
            if (!result)
            {
                var win32Exception = new Win32Exception();
                if (win32Exception.NativeErrorCode == ERROR_REQUEST_ABORTED)
                {
                    throw new OperationCanceledException(message, win32Exception);
                }
                throw new System.IO.IOException(message, win32Exception);
            }
        }

        [Flags]
        public enum EFileAccess : uint
        {

            GenericRead = 0x80000000,

            GenericWrite = 0x40000000,

            GenericExecute = 0x20000000,

            GenericAll = 0x10000000,

        }

        [Flags]
        public enum FileAccess : uint
        {
            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000,
        }

        [Flags]
        public enum EFileShare : uint
        {
            None = 0x00000000,
            Read = 0x00000001,
            Write = 0x00000002,
            Delete = 0x00000004,
        }

        public enum ECreationDisposition : uint
        {
            New = 1,
            CreateAlways = 2,
            OpenExisting = 3,
            OpenAlways = 4,
            TruncateExisting = 5,
        }

        [Flags]
        public enum EFileAttributes : uint
        {
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            Write_Through = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        /// <summary>The GetForegroundWindow function returns a handle to the foreground window.</summary>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();
        
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteFile(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CreateDirectory(string lpPathName, IntPtr secAttributes);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool RemoveDirectory(string lpPathName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetFileAttributesExW(string lpFileName,
           GET_FILEEX_INFO_LEVELS fInfoLevelId, out WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

        [StructLayout(LayoutKind.Sequential)]
        struct WIN32_FILE_ATTRIBUTE_DATA
        {
            public System.IO.FileAttributes dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
        }

        public enum GET_FILEEX_INFO_LEVELS
        {
            GetFileExInfoStandard,
            GetFileExMaxInfoLevel
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool GetVolumePathName(string lpszFileName,
            [Out] StringBuilder lpszVolumePathName, uint cchBufferLength);

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool PathAppend([In, Out] StringBuilder pszPath, string pszMore);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindFirstFileNameW(
            string lpFileName,
            uint dwFlags,
            ref uint stringLength,
            StringBuilder fileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool FindNextFileNameW(
            IntPtr hFindStream,
            ref uint stringLength,
            StringBuilder fileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafeFileHandle CreateFile(
            string lpFileName,
            [MarshalAs(UnmanagedType.U4)] System.IO.FileAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] System.IO.FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] System.IO.FileMode dwCreationDisposition,
            [MarshalAs(UnmanagedType.U4)] System.IO.FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(SafeHandle hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern FindHandle FindFirstFile(string lpFileName, out FindData lpFindFileData);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        internal static extern bool FindNextFile(FindHandle hFindFile, out FindData lpFindFileData);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError=true)]
        internal static extern bool FindClose(IntPtr hFindFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError=true)]
        internal static extern bool CopyFile(string lpExistingFileName, string lpNewFileName, bool bFailIfExists);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern
            bool CreateHardLink(
                string FileName,
                string ExistingFileName,
                IntPtr lpSecurityAttributes
            );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetFileAttributes(string lpFileName, System.IO.FileAttributes dwFileAttributes);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, int dwFlags);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CopyFileEx(
           string lpExistingFileName, 
           string lpNewFileName,
           CopyProgressRoutine lpProgressRoutine, 
           IntPtr lpData, 
           ref Int32 pbCancel,
           CopyFileFlags dwCopyFlags);

        public delegate CopyProgressResult CopyProgressRoutine(
            long TotalFileSize,
            long TotalBytesTransferred,
            long StreamSize,
            long StreamBytesTransferred,
            uint dwStreamNumber,
            CopyProgressCallbackReason dwCallbackReason,
            IntPtr hSourceFile,
            IntPtr hDestinationFile,
            IntPtr lpData);

        public enum CopyProgressResult : uint
        {
            PROGRESS_CONTINUE = 0,
            PROGRESS_CANCEL = 1,
            PROGRESS_STOP = 2,
            PROGRESS_QUIET = 3
        }

        public enum CopyProgressCallbackReason : uint
        {
            CALLBACK_CHUNK_FINISHED = 0x00000000,
            CALLBACK_STREAM_SWITCH = 0x00000001
        }

        [Flags]
        public enum CopyFileFlags : uint
        {
            COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008, // An attempt to copy an encrypted file will succeed even if the destination copy cannot be encrypted.
            COPY_FILE_COPY_SYMLINK = 0x00000800, // If the source file is a symbolic link, the destination file is also a symbolic link pointing to the same file that the source symbolic link is pointing to.
            COPY_FILE_FAIL_IF_EXISTS = 0x00000001, // The copy operation fails immediately if the target file already exists.
            COPY_FILE_NO_BUFFERING = 0x00001000, // The copy operation is performed using unbuffered I/O, bypassing system I/O cache resources. Recommended for very large file transfers.
            COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004, // The file is copied and the original file is opened for write access.
            COPY_FILE_RESTARTABLE = 0x00000002, // Progress of the copy is tracked in the target file in case the copy fails. The failed copy can be restarted at a later time by specifying the same values for lpExistingFileName and lpNewFileName as those used in the call that failed. This can significantly slow down the copy operation as the new file may be flushed multiple times during the copy operation.
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        public enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BY_HANDLE_FILE_INFORMATION
        {
            public uint FileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GetFileInformationByHandle(SafeFileHandle handle, out BY_HANDLE_FILE_INFORMATION lpFileInformation);
    }
}
