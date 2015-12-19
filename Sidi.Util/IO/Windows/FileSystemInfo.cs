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
using System.Runtime.InteropServices;
using System.Security;

namespace Sidi.IO.Windows
{
    /// <summary>
    /// Same methods and properties as System.IO.FileSystemInfo, but can handle long paths
    /// </summary>
    [Serializable]
    internal class FileSystemInfo : IFileSystemInfo
    {
        #region FileSystemInfo methods
        
        // Summary:
        //     Gets or sets the attributes for the current file or directory.
        //
        // Returns:
        //     System.IO.FileAttributes of the current System.IO.FileSystemInfo.
        //
        // Exceptions:
        //   System.IO.FileNotFoundException:
        //     The specified file does not exist.
        //
        //   System.IO.DirectoryNotFoundException:
        //     The specified path is invalid; for example, it is on an unmapped drive.
        //
        //   System.Security.SecurityException:
        //     The caller does not have the required permission.
        //
        //   System.ArgumentException:
        //     The caller attempts to set an invalid file attribute. -or-The user attempts
        //     to set an attribute value but does not have write permission.
        //
        //   System.IO.IOException:
        //     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
        public System.IO.FileAttributes Attributes
        {
            get
            {
                return (System.IO.FileAttributes) FindData.dwFileAttributes;
            }

            set
            {
                FS.SetFileAttribute(FullName, value);
                Refresh();
            }
        }

        //
        // Summary:
        //     Gets or sets the creation time of the current file or directory.
        //
        // Returns:
        //     The creation date and time of the current System.IO.FileSystemInfo object.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
        //
        //   System.IO.DirectoryNotFoundException:
        //     The specified path is invalid; for example, it is on an unmapped drive.
        //
        //   System.PlatformNotSupportedException:
        //     The current operating system is not Windows NT or later.
        //
        //   System.ArgumentOutOfRangeException:
        //     The caller attempts to set an invalid creation time.
        public DateTime CreationTime
        {
            get
            {
                return DateTime.FromFileTime(FindData.ftCreationTime);
            }
        }

        //
        // Summary:
        //     Gets or sets the creation time, in coordinated universal time (UTC), of the
        //     current file or directory.
        //
        // Returns:
        //     The creation date and time in UTC format of the current System.IO.FileSystemInfo
        //     object.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
        //
        //   System.IO.DirectoryNotFoundException:
        //     The specified path is invalid; for example, it is on an unmapped drive.
        //
        //   System.PlatformNotSupportedException:
        //     The current operating system is not Windows NT or later.
        //
        //   System.ArgumentOutOfRangeException:
        //     The caller attempts to set an invalid access time.
        [ComVisible(false)]
        public DateTime CreationTimeUtc
        {
            get
            {
                return DateTime.FromFileTimeUtc(FindData.ftCreationTime);
            }
        }

        //
        // Summary:
        //     Gets a value indicating whether the file or directory exists.
        //
        // Returns:
        //     true if the file or directory exists; otherwise, false.
        public bool Exists
        {
            get
            {
                return _findDataValid;
            }
        }

        //
        // Summary:
        //     Gets the string representing the extension part of the file.
        //
        // Returns:
        //     A string containing the System.IO.FileSystemInfo extension.
        public string Extension
        {
            get
            {
                return System.IO.Path.GetExtension(Name);
            }
        }
        
        //
        // Summary:
        //     Gets the full path of the directory or file.
        //
        // Returns:
        //     A string containing the full path.
        //
        // Exceptions:
        //   System.IO.PathTooLongException:
        //     The fully qualified path and file name is 260 or more characters.
        //
        //   System.Security.SecurityException:
        //     The caller does not have the required permission.
        public LPath FullName
        {
            get { return path; }
        }

        //
        // Summary:
        //     Gets or sets the time the current file or directory was last accessed.
        //
        // Returns:
        //     The time that the current file or directory was last accessed.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
        //
        //   System.PlatformNotSupportedException:
        //     The current operating system is not Windows NT or later.
        //
        //   System.ArgumentOutOfRangeException:
        //     The caller attempts to set an invalid access time
        public DateTime LastAccessTime
        {
            get
            {
                return DateTime.FromFileTime(FindData.ftLastAccessTime);
            }
        }

        //
        // Summary:
        //     Gets or sets the time, in coordinated universal time (UTC), that the current
        //     file or directory was last accessed.
        //
        // Returns:
        //     The UTC time that the current file or directory was last accessed.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
        //
        //   System.PlatformNotSupportedException:
        //     The current operating system is not Windows NT or later.
        //
        //   System.ArgumentOutOfRangeException:
        //     The caller attempts to set an invalid access time.
        [ComVisible(false)]
        public DateTime LastAccessTimeUtc
        {
            get
            {
                return DateTime.FromFileTimeUtc(FindData.ftLastAccessTime);
            }
        }

        //
        // Summary:
        //     Gets or sets the time when the current file or directory was last written
        //     to.
        //
        // Returns:
        //     The time the current file was last written.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
        //
        //   System.PlatformNotSupportedException:
        //     The current operating system is not Windows NT or later.
        //
        //   System.ArgumentOutOfRangeException:
        //     The caller attempts to set an invalid write time.
        public DateTime LastWriteTime
        {
            get
            {
                return DateTime.FromFileTime(FindData.ftLastWriteTime);
            }
        }

        //
        // Summary:
        //     Gets or sets the time, in coordinated universal time (UTC), when the current
        //     file or directory was last written to.
        //
        // Returns:
        //     The UTC time when the current file was last written to.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
        //
        //   System.PlatformNotSupportedException:
        //     The current operating system is not Windows NT or later.
        //
        //   System.ArgumentOutOfRangeException:
        //     The caller attempts to set an invalid write time.
        [ComVisible(false)]
        public DateTime LastWriteTimeUtc
        {
            get
            {
                return DateTime.FromFileTimeUtc(FindData.ftLastWriteTime);
            }
        }

        //
        // Summary:
        //     For files, gets the name of the file. For directories, gets the name of the
        //     last directory in the hierarchy if a hierarchy exists. Otherwise, the Name
        //     property gets the name of the directory.
        //
        // Returns:
        //     A string that is the name of the parent directory, the name of the last directory
        //     in the hierarchy, or the name of a file, including the file name extension.
        public string Name
        {
            get
            {
                return FindData.cFileName;
            }
        }

        // Summary:
        //     Deletes a file or directory.
        //
        // Exceptions:
        //   System.IO.DirectoryNotFoundException:
        //     The specified path is invalid; for example, it is on an unmapped drive.
        //
        //   System.IO.IOException:
        //     There is an open handle on the file or directory, and the operating system
        //     is Windows XP or earlier. This open handle can result from enumerating directories
        //     and files. For more information, see How to: Enumerate Directories and Files.
        public void Delete()
        {
            FullName.EnsureNotExists();
        }

        //
        // Summary:
        //     Refreshes the state of the object.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     A device such as a disk drive is not ready.
        [SecuritySafeCritical]
        public void Refresh()
        {
            _findDataValid = FS.GetFindData(this.path, out _findData);
        }

        #endregion

        [NonSerialized]
        readonly FileSystem _fileSystem;

        FileSystem FS
        {
            get
            {
                return _fileSystem;
            }
        }
        
        LPath path;
        WIN32_FIND_DATA _findData;
        bool _findDataValid = false;

        internal FileSystemInfo(FileSystem fileSystem, LPath path)
        {
            this._fileSystem = fileSystem;
            this.path = path.GetFullPath();
            Refresh();
        }

        internal FileSystemInfo(FileSystem fileSystem, LPath directory, WIN32_FIND_DATA findData)
        {
            this._fileSystem = fileSystem;
            _findData = findData;
            _findDataValid = true;
            path = directory.CatDir(findData.cFileName).GetFullPath();
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

        public long Length
        {
            get
            {
                return FindData.Length;
            }
        }

        public IList<IFileSystemInfo> GetChildren(string searchPattern = null)
        {
            return FS.FindFile(SearchPath(searchPattern)).ToList();
        }

        LPath SearchPath(string searchPattern)
        {
            if (searchPattern == null)
            {
                searchPattern = LPath.AllFilesWildcard;
            }
            return FullName.CatDir(searchPattern);
        }

        public IList<IFileSystemInfo> GetDirectories(string searchPattern)
        {
            return GetChildren(searchPattern)
                .Where(x => x.IsDirectory)
                .ToList();
        }

        public IList<IFileSystemInfo> GetFiles(string searchPattern = null)
        {
            return GetChildren(searchPattern)
                .Where(x => x.IsFile)
                .ToList();
        }

        public string FileNameWithoutExtension
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(FindData.cFileName);
            }
        }

        WIN32_FIND_DATA FindData
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
            return path.Equals(other.path) && 
                FindData.Equals(other.FindData);
        }

        public int CompareTo(object obj)
        {
            try
            {
                var o = (FileSystemInfo)obj;
                return path.CompareTo(o.path);
            }
            catch (InvalidCastException ex)
            {
                throw new ArgumentException("obj is not of type IFileSystemInfo", ex);
            }
        }

        public bool Equals(IFileSystemInfo other)
        {
            var r = other as FileSystemInfo;
            if (r == null)
            {
                return false;
            }
            return object.Equals(this.FullName, r.FullName);
        }

        public int CompareTo(IFileSystemInfo other)
        {
            return FullName.CompareTo(other.FullName);
        }
    }
}
