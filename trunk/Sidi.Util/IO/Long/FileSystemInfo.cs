using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO.Long.Extensions;

namespace Sidi.IO.Long
{
    public class FileSystemInfo : IEquatable<FileSystemInfo>
    {
        public Path FullName
        {
            get
            {
                if (parentDirectory == null)
                {
                    return new Path(Name);
                }
                else
                {
                    return parentDirectory.CatDir(Name);
                }
            }
        }

        public FileSystemInfo(Path path)
        {
            path = path.Canonic;
            parentDirectory = path.ParentDirectory;
            _findData.Name = path.Name;
            _findDataValid = false;
        }

        internal FileSystemInfo(Path directory, FindData findData)
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
                Kernel32.SetFileAttributes(FullName.Param, value).CheckApiCall(FullName);
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

        public IList<FileSystemInfo> GetFileSystemInfos()
        {
            return Directory.GetChilds(FullName);
        }

        public IList<FileSystemInfo> GetDirectories()
        {
            return GetDirectories("*");
        }

        public IList<FileSystemInfo> GetDirectories(string searchPattern)
        {
            return Directory.FindFile(FullName.CatDir(searchPattern))
                .Where(x => x.IsDirectory)
                .ToList();
        }

        public IList<FileSystemInfo> GetFiles()
        {
            return GetFiles("*");
        }

        public IList<FileSystemInfo> GetFiles(string searchPattern)
        {
            return Directory.FindFile(FullName.CatDir(searchPattern))
                .Where(x => !x.IsDirectory)
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
                return FullName.IsRoot ? 
                    System.IO.Directory.Exists(FullName.NoPrefix + @"\")
                    :
                    Directory.FindFile(FullName).Any();
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
                return _findData.Name;
            }
        }

        public string FileNameWithoutExtension
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(_findData.Name);
            }
        }

        public void Delete()
        {
            FullName.EnsureNotExists();
        }

        public void Refresh()
        {
            _findData = FullName.FindData;
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

        Path parentDirectory;

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
            return FullName.Equals(other.FullName);
        }
    }
}
