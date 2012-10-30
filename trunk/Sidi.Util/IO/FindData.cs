using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComTypes=System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;

namespace Sidi.IO
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct FindData
    {
        internal System.IO.FileAttributes Attributes;
        internal ComTypes.FILETIME ftCreationTime;
        internal ComTypes.FILETIME ftLastAccessTime;
        internal ComTypes.FILETIME ftLastWriteTime;
        internal int nFileSizeHigh;
        internal int nFileSizeLow;
        internal int dwReserved0;
        internal int dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Kernel32.MAX_PATH)]
        internal string Name;
        // not using this
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        internal string cAlternate;

        public bool IsDirectory
        {
            get
            {
                return Test(System.IO.FileAttributes.Directory);
            }
        }

        public bool Hidden
        {
            get
            {
                return Test(System.IO.FileAttributes.Hidden);
            }
        }

        bool Test(System.IO.FileAttributes a)
        {
            return (Attributes & a) == a;
        }

        public long Length
        {
            get
            {
                return (long)(nFileSizeHigh << 32 ) + (long)nFileSizeLow;
            }
        }
    }

}
