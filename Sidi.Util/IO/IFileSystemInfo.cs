using System;
using System.Collections.Generic;
namespace Sidi.IO
{
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
        IList<IFileSystemInfo> GetChildren(string searchPattern = null);
        IList<IFileSystemInfo> GetDirectories(string searchPattern = null);
        IList<IFileSystemInfo> GetFiles(string searchPattern = null);
        bool IsDirectory { get; }
        bool IsFile { get; }
        bool IsHidden { get; set; }
        bool IsReadOnly { get; set; }
        DateTime LastAccessTime { get; }
        DateTime LastAccessTimeUtc { get; }
        DateTime LastWriteTime { get; }
        DateTime LastWriteTimeUtc { get; }
        long Length { get; }
        string Name { get; }
        void Refresh();
    }
}
