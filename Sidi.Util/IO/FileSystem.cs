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
    }
}
