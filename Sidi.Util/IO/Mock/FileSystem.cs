using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.IO.Mock;
using System.Threading;
using System.IO;

namespace Sidi.IO.Mock
{
    /// <summary>
    /// Mock file system
    /// </summary>
    /// This implementation of IFileSystem uses a temporary ContentDirectory to store all generated files. 
    /// The ContentDirectory will be deleted on disposal of this instance.
    /// 
    /// Usage example:
    /// \snippet Sidi.Util.Test\IO\Mock\FileSystemTest.cs Usage
    public class FileSystem : IFileSystem, IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FileSystem()
        {
            ContentDirectory = Paths.Temp.CatDir(typeof(FileSystem).FullName, LPath.GetValidFilename(DateTime.UtcNow.ToString("o")));
            using (Sidi.IO.FileSystem.SetCurrent(RealFs))
            {
                ContentDirectory.EnsureDirectoryExists();
            }
            log.InfoFormat("Mock file system is using content directory {0}", ContentDirectory);
        }

        public LPath ContentDirectory { get; private set; }

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Deletes the ContentDirectory
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    using (Sidi.IO.FileSystem.SetCurrent(RealFs))
                    {
                        ContentDirectory.EnsureNotExists();
                        log.InfoFormat("Delete {0}", ContentDirectory);
                    }
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Creates a new virtual drive root
        /// </summary>
        /// Can create local drives (X:\\) and network shares (\\\\server\\share)
        /// <param name="root">Path of a file system  object in the drive root to be created.</param>
        public void CreateRoot(LPath root)
        {
            roots[root.Prefix] = new FileSystemInfo(this, new LPath(this, root.Root), true);
        }

        public void Move(LPath existingPath, LPath newPath)
        {
            var s = GetElement(existingPath);
            var d = GetElement(newPath.Parent);
            s.Parent.Childs.Remove(s.Name);
            s.Name = newPath.FileName;
            d.AddChild(s);
        }

        internal FileSystemInfo GetElement(LPath path)
        {
            var e = TryGetElement(path);
            if (e == null)
            {
                throw new System.IO.FileNotFoundException("file not found", path.ToString());
            }
            return e;
        }

        internal FileSystemInfo TryGetElement(LPath path)
        {
            FileSystemInfo e = null;
            if (!roots.TryGetValue(path.Prefix, out e))
            {
                return null;
            }

            foreach (var i in path.Parts)
            {
                if (!e.Childs.TryGetValue(i, out e))
                {
                    return null;
                }
            }

            return e;
        }
        
        public IFileSystemInfo GetInfo(LPath path)
        {
            var i = TryGetElement(path);
            if (i == null)
            {
                i = new FileSystemInfo(this, new LPath(this, path), false);
            }
            return i;
        }

        public IFileSystem RealFs = new Sidi.IO.Windows.FileSystem();

        public System.IO.Stream Open(LPath path, System.IO.FileMode fileMode, System.IO.FileAccess fileAccess, System.IO.FileShare shareMode)
        {
            var e = CreateFile(path);
            return RealFs.Open(e.Content, fileMode, fileAccess, shareMode);
        }

        public void DeleteFile(LPath fileName)
        {
            var e = GetElement(fileName);
            e.Delete();
        }

        FileSystemInfo CreateFile(LPath path)
        {
            var e = TryGetElement(path);
            if (e == null)
            {
                e = GetElement(path.Parent).AddChild(new FileSystemInfo(this, path.FileName, false));
            }
            else if (!e.IsFile)
            {
                throw new System.IO.IOException();
            }
            return e;
        }

        public void CopyFile(LPath existingFileName, LPath newFileName, IProgress<CopyFileProgress> progress = null, System.Threading.CancellationToken cancellationToken = new CancellationToken(), CopyFileOptions options = null)
        {
            var e = GetElement(existingFileName);
            var d = TryGetElement(newFileName);
            if (d == null)
            {
                d = CreateFile(newFileName);
                RealFs.CopyFile(e.Content, d.Content);
            }
        }

        public IHardLinkInfo GetHardLinkInfo(LPath path)
        {
            throw new NotImplementedException();
        }

        public void CreateHardLink(LPath fileName, LPath existingFileName)
        {
            throw new NotImplementedException();
        }

        public void EnsureDirectoryExists(LPath directoryName)
        {
            var e = TryGetElement(directoryName);
            if (e != null)
            {
                return;
            }

            var parent = directoryName.Parent;
            if (parent == null)
            {
                throw new System.IO.IOException(String.Format("Cannot create directory {0}", directoryName));
            }
            EnsureDirectoryExists(parent);
            var parentElement = GetElement(parent);
            parentElement.AddChild(new FileSystemInfo(this, directoryName.FileName, true));
        }

        public void RemoveDirectory(LPath directoryName)
        {
            var e = GetElement(directoryName);
            if (!e.IsDirectory || e.Childs.Any())
            {
                throw new System.IO.IOException();
            }
            e.Delete();
        }

        public bool TryRemoveDirectory(LPath directoryName)
        {
            var e = GetElement(directoryName);
            if (!e.IsDirectory || e.Childs.Any())
            {
                return false;
            }
            e.Delete();
            return true;
        }

        public LPath CurrentDirectory { get; set; }

        public IEnumerable<IFileSystemInfo> FindFile(LPath searchPath)
        {
            var d = GetElement(searchPath.Parent);
            var wc = searchPath.FileName;
            return d.Childs.Values.Where(x => IsWildcardMatch(x.Name, wc));
        }

        static bool IsWildcardMatch(string name, string pattern)
        {
            return true;
        }

        public IEnumerable<LPath> GetDrives()
        {
            return roots.Values
                .Select(_ => _.FullName)
                .Where(_ => !_.IsUnc);
        }

        public IEnumerable<LPath> GetAvailableDrives()
        {
            for (char letter = 'A'; letter <= 'Z'; ++letter)
            {
                var d = LPath.GetDriveRoot(letter);
                if (!d.Exists)
                {
                    yield return d;
                }
            }
        }

        Dictionary<string, FileSystemInfo> roots = new Dictionary<string, FileSystemInfo>();

    }
}
