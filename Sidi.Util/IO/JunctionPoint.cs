// Copyright (c) 2006, Jeff Brown
// Taken from http://www.codeproject.com/Articles/15633/Manipulating-NTFS-Junction-Points-in-NET
// Modifications are Copyright (c) 2014, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using Sidi.IO.Windows;

namespace Sidi.IO
{
    /// <summary>
    /// Provides access to NTFS junction points in .Net.
    /// </summary>
    public static class JunctionPoint
    {
        /// <summary>
        /// Creates a junction point from the specified directory to the specified target directory.
        /// </summary>
        /// <remarks>
        /// Only works on NTFS.
        /// </remarks>
        /// <param name="junctionPoint">The junction point path</param>
        /// <param name="targetDir">The target directory</param>
        /// <param name="overwrite">If true overwrites an existing reparse point or empty directory</param>
        /// <exception cref="IOException">Thrown when the junction point could not be created or when
        /// an existing directory was found and <paramref name="overwrite" /> if false</exception>
        public static void Create(LPath junctionPoint, LPath targetDir, bool overwrite = false)
        {
            targetDir = targetDir.GetFullPath();

            if (!targetDir.IsDirectory)
            {
                throw new IOException(String.Format("Target path {0} does not exist or is not a directory.", targetDir));
            }

            if (junctionPoint.IsDirectory)
            {
                if (!overwrite)
                {
                    throw new IOException(String.Format("Directory {0} already exists and overwrite parameter is false.", junctionPoint));
                }
            }
            else
            {
                junctionPoint.EnsureDirectoryExists();
            }

            using (SafeFileHandle handle = OpenReparsePoint(junctionPoint, NativeMethods.EFileAccess.GenericWrite))
            {
                byte[] targetDirBytes = Encoding.Unicode.GetBytes(NativeMethods.NonInterpretedPathPrefix + targetDir);

                var reparseDataBuffer = new NativeMethods.REPARSE_DATA_BUFFER();

                reparseDataBuffer.ReparseTag = NativeMethods.IO_REPARSE_TAG_MOUNT_POINT;
                reparseDataBuffer.ReparseDataLength = (ushort)(targetDirBytes.Length + 12);
                reparseDataBuffer.SubstituteNameOffset = 0;
                reparseDataBuffer.SubstituteNameLength = (ushort)targetDirBytes.Length;
                reparseDataBuffer.PrintNameOffset = (ushort)(targetDirBytes.Length + 2);
                reparseDataBuffer.PrintNameLength = 0;
                reparseDataBuffer.PathBuffer = new byte[0x3ff0];
                Array.Copy(targetDirBytes, reparseDataBuffer.PathBuffer, targetDirBytes.Length);

                int inBufferSize = Marshal.SizeOf(reparseDataBuffer);
                IntPtr inBuffer = Marshal.AllocHGlobal(inBufferSize);

                try
                {
                    Marshal.StructureToPtr(reparseDataBuffer, inBuffer, false);

                    int bytesReturned;
                    bool result = NativeMethods.DeviceIoControl(handle.DangerousGetHandle(), NativeMethods.FSCTL_SET_REPARSE_POINT,
                        inBuffer, targetDirBytes.Length + 20, IntPtr.Zero, 0, out bytesReturned, IntPtr.Zero);

                    if (!result)
                    {
                        ThrowLastWin32Error("Unable to create junction point.");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(inBuffer);
                }
            }
        }

        /// <summary>
        /// Deletes a junction point at the specified source directory along with the directory itself.
        /// Does nothing if the junction point does not exist.
        /// </summary>
        /// <remarks>
        /// Only works on NTFS.
        /// </remarks>
        /// <param name="junctionPoint">The junction point path</param>
        public static void Delete(LPath junctionPoint)
        {
            if (junctionPoint.Exists)
            {
                if (!junctionPoint.IsDirectory)
                {
                    throw new IOException("Path is not a junction point.");
                }

                using (SafeFileHandle handle = OpenReparsePoint(junctionPoint, NativeMethods.EFileAccess.GenericWrite))
                {
                    var reparseDataBuffer = new NativeMethods.REPARSE_DATA_BUFFER();

                    reparseDataBuffer.ReparseTag = NativeMethods.IO_REPARSE_TAG_MOUNT_POINT;
                    reparseDataBuffer.ReparseDataLength = 0;
                    reparseDataBuffer.PathBuffer = new byte[0x3ff0];

                    int inBufferSize = Marshal.SizeOf(reparseDataBuffer);
                    IntPtr inBuffer = Marshal.AllocHGlobal(inBufferSize);
                    try
                    {
                        Marshal.StructureToPtr(reparseDataBuffer, inBuffer, false);

                        int bytesReturned;
                        bool result = NativeMethods.DeviceIoControl(handle.DangerousGetHandle(), NativeMethods.FSCTL_DELETE_REPARSE_POINT,
                            inBuffer, 8, IntPtr.Zero, 0, out bytesReturned, IntPtr.Zero);

                        if (!result)
                            ThrowLastWin32Error("Unable to delete junction point.");
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(inBuffer);
                    }

                    try
                    {
                        junctionPoint.RemoveDirectory();
                    }
                    catch (IOException ex)
                    {
                        throw new IOException("Unable to delete junction point.", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the specified path exists and refers to a junction point.
        /// </summary>
        /// <param name="path">The junction point path</param>
        /// <returns>True if the specified path represents a junction point</returns>
        /// <exception cref="IOException">Thrown if the specified path is invalid
        /// or some other error occurs</exception>
        public static bool Exists(LPath path)
        {
            if (!path.IsDirectory)
            {
                return false;
            }

            using (SafeFileHandle handle = OpenReparsePoint(path, NativeMethods.EFileAccess.GenericRead))
            {
                return InternalGetTarget(handle) != null;
            }
        }

        /// <summary>
        /// Gets the target of the specified junction point.
        /// </summary>
        /// <remarks>
        /// Only works on NTFS.
        /// </remarks>
        /// <param name="junctionPoint">The junction point path</param>
        /// <returns>The target of the junction point</returns>
        /// <exception cref="IOException">Thrown when the specified path does not
        /// exist, is invalid, is not a junction point, or some other error occurs</exception>
        public static LPath GetTarget(LPath junctionPoint)
        {
            using (var handle = OpenReparsePoint(junctionPoint, NativeMethods.EFileAccess.GenericRead))
            {
                var target = InternalGetTarget(handle);
                if (target == null)
                {
                    throw new IOException("Path is not a junction point.");
                }

                return target;
            }
        }

        private static LPath InternalGetTarget(SafeFileHandle handle)
        {
            int outBufferSize = Marshal.SizeOf(typeof(NativeMethods.REPARSE_DATA_BUFFER));
            IntPtr outBuffer = Marshal.AllocHGlobal(outBufferSize);

            try
            {
                int bytesReturned;
                bool result = NativeMethods.DeviceIoControl(handle.DangerousGetHandle(), NativeMethods.FSCTL_GET_REPARSE_POINT,
                    IntPtr.Zero, 0, outBuffer, outBufferSize, out bytesReturned, IntPtr.Zero);

                if (!result)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == NativeMethods.ERROR_NOT_A_REPARSE_POINT)
                        return null;

                    ThrowLastWin32Error("Unable to get information about junction point.");
                }

                var reparseDataBuffer = (NativeMethods.REPARSE_DATA_BUFFER)
                    Marshal.PtrToStructure(outBuffer, typeof(NativeMethods.REPARSE_DATA_BUFFER));

                if (reparseDataBuffer.ReparseTag != NativeMethods.IO_REPARSE_TAG_MOUNT_POINT)
                    return null;

                string targetDir = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer,
                    reparseDataBuffer.SubstituteNameOffset, reparseDataBuffer.SubstituteNameLength);

                if (targetDir.StartsWith(NativeMethods.NonInterpretedPathPrefix))
                    targetDir = targetDir.Substring(NativeMethods.NonInterpretedPathPrefix.Length);

                return new LPath(targetDir);
            }
            finally
            {
                Marshal.FreeHGlobal(outBuffer);
            }
        }

        private static SafeFileHandle OpenReparsePoint(LPath reparsePoint, NativeMethods.EFileAccess accessMode)
        {
            var fs = (Sidi.IO.Windows.FileSystem)reparsePoint.FileSystem;

            var h  = NativeMethods.CreateFile(fs.GetLongPathApiParameter(reparsePoint), accessMode,
                NativeMethods.EFileShare.Read | NativeMethods.EFileShare.Write | NativeMethods.EFileShare.Delete,
                IntPtr.Zero, NativeMethods.ECreationDisposition.OpenExisting,
                NativeMethods.EFileAttributes.BackupSemantics | NativeMethods.EFileAttributes.OpenReparsePoint, IntPtr.Zero);

            if (Marshal.GetLastWin32Error() != 0)
            {
                ThrowLastWin32Error("Unable to open reparse point.");
            }

            var reparsePointHandle = new SafeFileHandle(h, true);

            return reparsePointHandle;
        }

        private static void ThrowLastWin32Error(string message)
        {
            throw new IOException(message, Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
        }
    }
}
