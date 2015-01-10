using System;
using System.Collections.Generic;
using System.Threading;

namespace Sidi.IO
{
    public interface IFileSystem
    {
        #region File system element operations (for files and directories)

        /// <summary>
        /// Move a file system element (directory or file).
        /// </summary>
        /// <param name="existingPath">Existing file system element to be moved.</param>
        /// <param name="newPath">New path of the file system element.</param>
        void Move(LPath existingPath, LPath newPath);

        /// <summary>
        /// Retrieve information about a file system element
        /// </summary>
        /// <param name="path">Path to the file system element</param>
        /// <returns>Information about the file system element.</returns>
        IFileSystemInfo GetInfo(LPath path);

        #endregion

        #region File Operations

        /// <summary>
        /// Open a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileMode"></param>
        /// <returns></returns>
        System.IO.FileStream Open(LPath fileName, System.IO.FileMode fileMode);

        System.IO.FileStream Open(LPath path, System.IO.FileMode fileMode, System.IO.FileAccess fileAccess, System.IO.FileShare shareMode);

        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="fileName">Name of the file to be deleted.</param>
        void DeleteFile(LPath fileName);

        /// <summary>
        /// Copy a file.
        /// </summary>
        /// <param name="existingFileName">Name of the copy source file.</param>
        /// <param name="newFileName">Name of the copy destination file.</param>
        /// <param name="progress">To report progress information back to the caller. Can be null.</param>
        /// <param name="cancellationToken">To cancel the copy operation. When cancelled, the destination file will not exist.</param>
        /// <param name="options">Options for the copy operation.</param>
        void CopyFile(
            LPath existingFileName, 
            LPath newFileName, 
            IProgress<CopyFileProgress> progress = null,
            System.Threading.CancellationToken cancellationToken = new CancellationToken(), 
            CopyFileOptions options = null);

        /// <summary>
        /// Returns information about a hard linked file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        IHardLinkInfo GetHardLinkInfo(LPath path);

        /// <summary>
        /// Create a new hard link.
        /// </summary>
        /// Precondition: (existingFileName.IsFile &amp;&amp; !fileName.IsFile &amp;&amp; fileName.Parent.IsDirectory)
        /// Postcondition: (fileName.IsFile)
        /// Will throw an exception ? when fileName does not have the same root as existingFileName
        /// <param name="fileName">Path where the new hard link shall be created.</param>
        /// <param name="existingFileName">Existing file for which a hard link will be created.</param>
        void CreateHardLink(LPath fileName, LPath existingFileName);

        #endregion

        #region Directory operations

        /// <summary>
        /// Ensure that the specified directory exists.
        /// </summary>
        /// Will create parent directories if required.
        /// 
        /// Postcondition: (directoryName.IsDirectory)
        /// <param name="directoryName">Path to the directory</param>
        void EnsureDirectoryExists(LPath directoryName);

        /// <summary>
        /// Ensure that the specified directory is removed.
        /// </summary>
        /// Precondition: (directoryName.IsDirectory &amp;&amp; !directoryName.Children.Any())
        /// Postcondition: (!directoryName.Exists)
        /// <param name="directoryName">Path to the directory.</param>
        void RemoveDirectory(LPath directoryName);

        /// <summary>
        /// Returns the current directory of the process. Use wisely.
        /// </summary>
        /// <returns>Current directory.</returns>
        LPath CurrentDirectory
        {
            get;
            set;
        }

        System.Collections.Generic.IEnumerable<IFileSystemInfo> FindFile(LPath searchPath);

        #endregion

        #region File system operations

        /// <summary>
        /// Root paths of the existing drives (e.g. C:\, D:\)
        /// </summary>
        /// <returns>Root paths of existing drives.</returns>
        IEnumerable<LPath> GetDrives();

        /// <summary>
        /// Root paths of the drive letters which are not yet used. Can for example be used to determine a free drive letter for mounting network shares.
        /// </summary>
        /// <returns>List of available drive root paths/</returns>
        IEnumerable<LPath> GetAvailableDrives();

        #endregion
    }
}
