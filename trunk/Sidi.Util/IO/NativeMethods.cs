using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Sidi.IO
{
    internal static class NativeMethods
    {
        [DllImport("Kernel32.dll")]
        internal static extern
            bool CreateHardLink(
                string FileName,
                string ExistingFileName,
                IntPtr lpSecurityAttributes
            );
    }
}
