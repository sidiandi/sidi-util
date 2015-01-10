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
                if (current == null)
                {
                    current = DefaultThreadCurrent;
                }
                return current;
            }

            set
            {
                current = value;
            }
        }

        public static IFileSystem DefaultThreadCurrent
        {
            get
            {
                lock (syncRoot)
                {
                    if (defaultThreadCurrent == null)
                    {
                        defaultThreadCurrent = new Windows.FileSystem();
                    }
                    return defaultThreadCurrent;
                }
            }

            set
            {
                defaultThreadCurrent = value;
            }
        }

        private static object syncRoot = new Object(); 
        static IFileSystem defaultThreadCurrent;

        [ThreadStatic]
        static IFileSystem current = null;

        public static IDisposable SetCurrent(IFileSystem fileSystem)
        {
            var originalValue = Current;
            current = fileSystem;
            return new OnDispose(() => { current = originalValue; });
        }
    }
}
