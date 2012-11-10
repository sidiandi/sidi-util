// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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

namespace Sidi.IO
{
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
                return ToDateTime(FindData.ftCreationTime);
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                return ToDateTimeUtc(FindData.ftCreationTime);
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
            return ToDateTime(FindData.ftLastAccessTime);
        }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                return ToDateTime(FindData.ftLastAccessTime);
            }
        }

        static DateTime ToDateTime(System.Runtime.InteropServices.ComTypes.FILETIME fileTime)
        {
            ulong h = (uint)fileTime.dwHighDateTime;
            h <<= 32;
            ulong l = (uint)fileTime.dwLowDateTime;
            return DateTime.FromFileTime((long)(h | l));
        }

        static DateTime ToDateTimeUtc(System.Runtime.InteropServices.ComTypes.FILETIME fileTime)
        {
            ulong h = (uint) fileTime.dwHighDateTime;
            h <<= 32;
            ulong l = (uint)fileTime.dwLowDateTime;
            return DateTime.FromFileTimeUtc((long)(h | l));
        }

        public DateTime LastWriteTime
        { 
            get
            {
                return ToDateTime(FindData.ftLastWriteTime);
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                return ToDateTimeUtc(FindData.ftLastWriteTime);
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
    }
}
