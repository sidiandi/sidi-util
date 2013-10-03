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
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;

namespace Sidi.IO
{
    [Serializable]
    public class LFileSystemInfo : IEquatable<LFileSystemInfo>
    {
        public LFileSystemInfo(LPath path)
        {
            this.path = path.GetFullPath();
            _findDataValid = this.path.GetFindData(out _findData);
        }

        public LPath FullName
        {
            get { return path; }
        }
        
        LPath path;
        FindData _findData;
        bool _findDataValid = false;

        internal LFileSystemInfo(LPath directory, FindData findData)
        {
            _findData = findData;
            _findDataValid = true;
            path = directory.CatDir(findData.Name).GetFullPath();
        }

        public System.IO.FileAttributes Attributes
        { 
            get
            {
                return FindData.Attributes;
            }

            set
            {
                NativeMethods.SetFileAttributes(FullName.Param, value).CheckApiCall(FullName);
                _findData.Attributes = value;
            }
        }

        public bool IsReadOnly
        {
            set
            {
                SetAttributes(System.IO.FileAttributes.ReadOnly, value);
            }

            get
            {
                return IsAttribute(System.IO.FileAttributes.ReadOnly);
            }
        }

        public bool IsHidden
        {
            set
            {
                SetAttributes(System.IO.FileAttributes.Hidden, value);
            }

            get
            {
                return IsAttribute(System.IO.FileAttributes.Hidden);
            }
        }

        bool IsAttribute(System.IO.FileAttributes a)
        {
            return (Attributes & a) == a;
        }

        void SetAttributes(System.IO.FileAttributes a, bool value)
        {
            Attributes = (Attributes & ~a) | (value ? a : 0);
        }

        public bool IsDirectory
        {
            get
            {
                return IsAttribute(System.IO.FileAttributes.Directory);
            }
        }

        public bool IsFile
        {
            get
            {
                return Exists && !IsDirectory;
            }
        }

        public bool Hidden
        {
            get
            {
                return IsAttribute(System.IO.FileAttributes.Hidden);
            }

            set
            {
                SetAttributes(System.IO.FileAttributes.Hidden, value);
            }
        }

        public long Length
        {
            get
            {
                return FindData.Length;
            }
        }

        public IList<LFileSystemInfo> GetChildren()
        {
            return GetChildren(LPath.AllFilesWildcard);
        }

        public IList<LFileSystemInfo> GetChildren(string searchPattern)
        {
            return LDirectory.FindFile(this.FullName.CatDir(searchPattern)).ToList();
        }

        public IList<LFileSystemInfo> GetDirectories()
        {
            return GetDirectories(LPath.AllFilesWildcard);
        }

        public IList<LFileSystemInfo> GetDirectories(string searchPattern)
        {
            return GetChildren(searchPattern)
                .Where(x => x.IsDirectory)
                .ToList();
        }

        public IList<LFileSystemInfo> GetFiles()
        {
            return GetFiles(LPath.AllFilesWildcard);
        }

        public IList<LFileSystemInfo> GetFiles(string searchPattern)
        {
            return GetChildren(searchPattern)
                .Where(x => x.IsFile)
                .ToList();
        }

        public DateTime CreationTime
        { 
            get
            {
                return FindData.ftCreationTime.DateTime;
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                return FindData.ftCreationTime.DateTimeUtc;
            }
        }
        
        public bool Exists
        {
            get
            {
                return _findDataValid;
            }
        }

        public string Extension
        {
            get
            {
                return System.IO.Path.GetExtension(Name);
            }
        }

        public DateTime LastAccessTime
        {
            get
            {
                return FindData.ftLastAccessTime.DateTime;
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                return FindData.ftLastAccessTime.DateTimeUtc;
            }
        }

        public DateTime LastWriteTime
        { 
            get
            {
                return FindData.ftLastWriteTime.DateTime;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                return FindData.ftLastWriteTime.DateTimeUtc;
            }
        }

        public string Name
        {
            get
            {
                return FindData.Name;
            }
        }

        public string FileNameWithoutExtension
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(FindData.Name);
            }
        }

        public void Delete()
        {
            FullName.EnsureNotExists();
        }

        FindData FindData
        {
            get
            {
                return _findData;
            }
        }

        public override string ToString()
        {
            return FullName.ToString();
        }

        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is LFileSystemInfo)
            {
                return Equals((LFileSystemInfo)obj);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(LFileSystemInfo other)
        {
            return FullName.Equals(other.FullName);
        }

        NativeMethods.BY_HANDLE_FILE_INFORMATION GetByHandleFileInformation()
        {
            using (var handle = NativeMethods.CreateFile(
                this.FullName.Param, 
                System.IO.FileAccess.Read, 
                System.IO.FileShare.Read, IntPtr.Zero, 
                System.IO.FileMode.Open, System.IO.FileAttributes.Normal, 
                IntPtr.Zero))
                {
            if (handle.IsInvalid)
            {
                throw new Win32Exception(this.FullName);
            }

                var fileInfo = new NativeMethods.BY_HANDLE_FILE_INFORMATION();
                NativeMethods.GetFileInformationByHandle(handle, out fileInfo)
                    .CheckApiCall(this.FullName);
                return fileInfo;
            }
        }

        public int FileLinkCount
        {
            get
            {
                return (int) GetByHandleFileInformation().NumberOfLinks;
            }
        }

        public long FileIndex
        {
            get
            {
                var fileInfo = GetByHandleFileInformation();
                return (long)(((ulong)fileInfo.FileIndexHigh << 32) + (ulong)fileInfo.FileIndexLow);
            }
        }

        static string[] GetFileSiblingHardLinks(string filepath)
        {
            List<string> result = new List<string>();
            uint stringLength = 256;
            StringBuilder sb = new StringBuilder(256);
            NativeMethods.GetVolumePathName(filepath, sb, stringLength);
            string volume = sb.ToString();
            sb.Length = 0; stringLength = 256;
            IntPtr findHandle = NativeMethods.FindFirstFileNameW(filepath, 0, ref stringLength, sb);
            if (findHandle.ToInt32() != -1)
            {
                do
                {
                    StringBuilder pathSb = new StringBuilder(volume, 256);
                    NativeMethods.PathAppend(pathSb, sb.ToString());
                    result.Add(pathSb.ToString());
                    sb.Length = 0; stringLength = 256;
                } while (NativeMethods.FindNextFileNameW(findHandle, ref stringLength, sb));
                NativeMethods.FindClose(findHandle);
                return result.ToArray();
            }
            return null;
        }

        public IList<LPath> HardLinks
        {
            get
            {
                return GetFileSiblingHardLinks(FullName.ToString())
                    .Select(x => new LPath(x))
                    .ToList();
            }
        }
    }
}
