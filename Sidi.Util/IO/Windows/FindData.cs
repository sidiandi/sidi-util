// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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

namespace Sidi.IO.Windows
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack=4, Size=318)]
    [Serializable]
    internal struct WIN32_FIND_DATA
    {
        public uint dwFileAttributes;
        public long ftCreationTime;
        public long ftLastAccessTime;
        public long ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;

        public bool Equals(WIN32_FIND_DATA other)
        {
            return
                dwFileAttributes == other.dwFileAttributes &&
                Equals(ftCreationTime, other.ftCreationTime) &&
                Equals(ftLastAccessTime, other.ftLastAccessTime) &&
                Equals(ftLastWriteTime, other.ftLastWriteTime) &&
                nFileSizeHigh == other.nFileSizeHigh &&
                nFileSizeLow == other.nFileSizeLow &&
                dwReserved0 == other.dwReserved0 &&
                dwReserved1 == other.dwReserved1 &&
                cFileName == other.cFileName &&
                cAlternateFileName == other.cAlternateFileName;
        }

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
            return ((System.IO.FileAttributes)dwFileAttributes & a) == a;
        }

        public long Length
        {
            get
            {
                return (long)(nFileSizeHigh << 32) + (long)nFileSizeLow;
            }
        }
    }
}
