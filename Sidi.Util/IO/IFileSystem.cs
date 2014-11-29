using System;
using System.Collections.Generic;
using System.Threading;

namespace Sidi.IO
{
    public interface IFileSystem
    {
        void DeleteFile(LPath fileName);
        void CopyFile(LPath existingFileName, LPath newFileName, IProgress<CopyFileProgress> progress = null, System.Threading.CancellationToken ct = new CancellationToken(), CopyFileOptions options = null);
        void Move(LPath existingFileName, LPath newFileName);

        IHardLinkInfo GetHardLinkInfo(LPath path);
        void CreateHardLink(LPath fileName, LPath existingFileName);

        void EnsureDirectoryExists(LPath directory);
        void RemoveDirectory(LPath directory);
        LPath GetCurrentDirectory();

        System.Collections.Generic.IEnumerable<IFileSystemInfo> FindFile(LPath searchPath);
        IFileSystemInfo GetInfo(LPath lPath);

        System.IO.FileStream Open(LPath fileName, System.IO.FileMode fileMode);
        System.IO.FileStream Open(LPath path, System.IO.FileMode fileMode, System.IO.FileAccess fileAccess, System.IO.FileShare shareMode);

        IEnumerable<LPath> GetDrives();
        IEnumerable<LPath> GetAvailableDrives();
    }
}
