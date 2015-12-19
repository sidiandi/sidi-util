using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sidi.IO.Mock
{
    internal class FileSystemInfo : IFileSystemInfo
    {
        public readonly Dictionary<string, FileSystemInfo> Childs = new Dictionary<string, FileSystemInfo>();
        private LPath path;

        public FileSystemInfo(FileSystem fileSystem, string name, bool isDirectory)
        {
            this.fileSystem = fileSystem;
            this.Name = name;
            this.isDirectory = isDirectory;
        }

        public FileSystemInfo(FileSystem fileSystem, LPath path, bool isDirectory)
        {
            this.fileSystem = fileSystem;
            this.path = path;
            this.isDirectory = isDirectory;
        }
        public FileSystemInfo Parent { get; set; }

        public LPath _content;

        FileSystem fileSystem;

        FileSystem MockFs
        {
            get
            {
                return fileSystem;
            }
        }
        
        IFileSystem RealFs
        {
            get
            {
                return MockFs.RealFs;
            }
        }

        public LPath Content
        {
            get
            {
                if (_content == null)
                {
                    _content = MockFs.ContentDirectory.CatDir(LPath.GetRandomFileName());
                    _content.EnsureParentDirectoryExists();
                }
                return _content;
            }
        }

        public FileSystemInfo AddChild(FileSystemInfo c)
        {
            c.Parent = this;
            Childs.Add(c.Name, c);
            return c;
        }

        public System.IO.FileAttributes Attributes
        {
            get; set; 
        }

        public DateTime CreationTime
        {
            get { return CreationTimeUtc.ToLocalTime(); }
        }

        public DateTime CreationTimeUtc
        {
            get;
            set; 
        }

        public void Delete()
        {
            this.Parent.Childs.Remove(Name);
        }

        public override string ToString()
        {
            return String.Format("Mock({0})", FullName);
        }

        public bool Exists
        {
            get
            {
                var mfs = (FileSystem)FullName.FileSystem;
                return mfs.TryGetElement(FullName) != null;
            }
        }

        public string Extension
        {
            get { return FullName.Extension; }
        }

        public string FileNameWithoutExtension
        {
            get { return FullName.FileNameWithoutExtension; }
        }

        public LPath FullName
        {
            get
            {
                if (Parent == null)
                {
                    return path;
                }
                else
                {
                    return Parent.FullName.CatDir(Name);
                }
            }
        }

        public IList<IFileSystemInfo> GetChildren(string searchPattern = null)
        {
            return Childs.Values.Cast<IFileSystemInfo>().ToList();
        }

        public IList<IFileSystemInfo> GetDirectories(string searchPattern = null)
        {
            return GetChildren(searchPattern).Where(x => x.IsDirectory).ToList();
        }

        public IList<IFileSystemInfo> GetFiles(string searchPattern = null)
        {
            return GetChildren(searchPattern).Where(x => x.IsFile).ToList();
        }

        bool isDirectory;

        public bool IsDirectory
        {
            get { return isDirectory; }
        }

        public bool IsFile
        {
            get { return !IsDirectory; }
        }

        bool IsAttribute(System.IO.FileAttributes a)
        {
            return (Attributes & a) == a;
        }

        void SetAttributes(System.IO.FileAttributes a, bool value)
        {
            Attributes = (Attributes & ~a) | (value ? a : 0);
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

        public DateTime LastAccessTime
        {
            get { return LastAccessTimeUtc.ToLocalTime(); }
        }

        public DateTime LastAccessTimeUtc
        {
            get; set;
        }

        public DateTime LastWriteTime
        {
            get { return LastWriteTimeUtc.ToLocalTime(); }
        }

        public DateTime LastWriteTimeUtc
        {
            get; set; 
        }

        public long Length
        {
            get { return Content.Info.Length; }
        }

        public string Name
        {
            get;
            set; 
        }

        public void Refresh()
        {
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
