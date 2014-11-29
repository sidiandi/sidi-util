using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    public static class FileSystemExtension
    {
        public static void CopyOrHardLink(this IFileSystem fs, LPath source, LPath destination)
        {
            destination.EnsureParentDirectoryExists();
            if (LPath.IsSameFileSystem(source, destination))
            {
                fs.CreateHardLink(destination, source);
            }
            else
            {
                fs.CopyFile(source, destination);
            }
        }
    }
}
