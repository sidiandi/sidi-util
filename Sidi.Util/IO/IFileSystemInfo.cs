using System;
using System.Collections.Generic;
namespace Sidi.IO
{
    /// <summary>
    /// Set and get attributes of a file system element (file or directory)
    /// </summary>
    /// Most properties are identical to the properties of System.IO.FileInfo and System.IO.DirectoryInfo
    public interface IFileSystemInfo
    {
        System.IO.FileAttributes Attributes { get; set; }
        DateTime CreationTime { get; }
        DateTime CreationTimeUtc { get; }
        void Delete();
        bool Exists { get; }
        string Extension { get; }
        string FileNameWithoutExtension { get; }
        LPath FullName { get; }

        /// <summary>
        /// Get all files and directories.
        /// </summary>
        /// <param name="searchPattern">A file name with wild cards * and ?. If omitted, all elements are returned.</param>
        /// <returns>Will return an empty list if called on files.</returns>
        IList<IFileSystemInfo> GetChildren(string searchPattern = null);

        /// <summary>
        /// Like GetChildren, but will only return directories.
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        IList<IFileSystemInfo> GetDirectories(string searchPattern = null);

        /// <summary>
        /// Like GetChildren, but will only return files.
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        IList<IFileSystemInfo> GetFiles(string searchPattern = null);
        
        /// <summary>
        /// Returns true if this element exists and is a directory
        /// </summary>
        bool IsDirectory { get; }
        
        /// <summary>
        /// Returns true if this element exists and is a file
        /// </summary>
        bool IsFile { get; }

        /// <summary>
        /// Returns true if this element is hidden.
        /// </summary>
        bool IsHidden { get; set; }

        /// <summary>
        /// Returns true if this element is read-only
        /// </summary>
        bool IsReadOnly { get; set; }

        DateTime LastAccessTime { get; }
        DateTime LastAccessTimeUtc { get; }
        DateTime LastWriteTime { get; }
        DateTime LastWriteTimeUtc { get; }

        /// <summary>
        /// Length of the element in bytes. Will return 0 for directories.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Name of the element, i.e. the file name without path information
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Re-read the element's attributes from disk.
        /// </summary>
        void Refresh();
    }
}
