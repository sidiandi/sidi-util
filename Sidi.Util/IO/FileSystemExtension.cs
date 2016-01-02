using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    public static class FileSystemExtension
    {
        /// <summary>
        /// Will return the default file system if called on a null reference.
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        public static IFileSystem OrDefault(this IFileSystem fs)
        {
            if (fs == null)
            {
                fs = FileSystem.Current;
            }
            return fs;
        }

        public static System.IO.Stream Open(this IFileSystem fileSystem, LPath fileName, System.IO.FileMode fileMode)
        {
            var desiredAccess = System.IO.FileAccess.ReadWrite;
            var shareMode = System.IO.FileShare.None;

            if (fileName.Prefix.Equals(@"\\.\"))
            {
                shareMode = System.IO.FileShare.Write;
                desiredAccess = System.IO.FileAccess.Read;
            }

            return fileSystem.Open(fileName, fileMode, desiredAccess, shareMode);
        }
    }
}
