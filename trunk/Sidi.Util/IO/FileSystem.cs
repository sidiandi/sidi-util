using Sidi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    public class FileSystem
    {
        public static IFileSystem Current
        {
            get
            {
                return current;
            }
        }

        static IFileSystem current = new Sidi.IO.Windows.FileSystem();

        public static IDisposable SetCurrent(IFileSystem fileSystem)
        {
            var originalValue = Current;
            current = fileSystem;
            return new OnDispose(() => { current = originalValue; });
        }
    }
}
