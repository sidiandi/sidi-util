using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.IO.Long
{
    public class FileSystemInfo : IEquatable<FileSystemInfo>
    {
        public LongName FullPath
        {
            get
            {
                return parentDirectory.CatDir(Name);
            }
        }

        public FileSystemInfo(LongName path)
        {
            path = path.Canonic;
            parentDirectory = path.ParentDirectory;
            _findData.Name = path.Name;
            _findDataValid = false;
        }

        internal FileSystemInfo(LongName directory, FindData findData)
        {
            _findData = findData;
            parentDirectory = directory;
        }

        public System.IO.FileAttributes Attributes
        { 
            get
            {
                return FindData.Attributes;
            }

            set
            {
                Kernel32.SetFileAttributes(FullPath.Param, value).CheckApiCall(FullPath);
                _findData.Attributes = value;
            }
        }

        public bool ReadOnly
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

        bool IsAttribute(System.IO.FileAttributes a)
        {
            return (Attributes & a) == a;
        }

        void SetAttributes(System.IO.FileAttributes a, bool value)
        {
            Attributes = (Attributes & a) | (value ? a : 0);
        }

        public bool IsDirectory
        {
            get
            {
                return IsAttribute(System.IO.FileAttributes.Directory);
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

        public IEnumerable<FileSystemInfo> GetChilds()
        {
            return Directory.GetChilds(FullPath);
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
                return Directory.FindFile(FullPath).Any();
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
            return DateTime.FromFileTime(((long)fileTime.dwHighDateTime << 32) | fileTime.dwLowDateTime);
        }

        static DateTime ToDateTimeUtc(System.Runtime.InteropServices.ComTypes.FILETIME fileTime)
        {
            return DateTime.FromFileTimeUtc(((long)fileTime.dwHighDateTime << 32) | fileTime.dwLowDateTime);
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
                return _findData.Name;
            }
        }

        public void Delete()
        {
            FullPath.EnsureNotExists();
        }

        public void Refresh()
        {
            _findData = FullPath.FindData;
            _findDataValid = true;
        }

        FindData FindData
        {
            get
            {
                if (!_findDataValid)
                {
                    Refresh();
                }
                return _findData;
            }
        }
        FindData _findData;
        bool _findDataValid = false;

        LongName parentDirectory;

        public override string ToString()
        {
            return FullPath.ToString();
        }

        public override int GetHashCode()
        {
            return FullPath.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is FileSystemInfo)
            {
                return Equals((FileSystemInfo)obj);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(FileSystemInfo other)
        {
            return FullPath.Equals(other.FullPath);
        }
    }
}
