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
    }
}
