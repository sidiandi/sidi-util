using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    public sealed class OnDispose : IDisposable
    {
        public OnDispose(Action action)
        {
            this.action = action;
        }

        Action action;
        bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                action();
                disposed = true;
            }
        }
    }

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
