// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

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
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH)]
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
