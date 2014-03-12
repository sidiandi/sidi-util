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

namespace Sidi.IO
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    [Serializable]
    internal struct FindData
    {
        // Summary:
        //     Represents the number of 100-nanosecond intervals since January 1, 1601.
        //     This structure is a 64-bit value.
        [Serializable]
        internal struct FILETIME
        {
            //
            // Summary:
            //     Specifies the low 32 bits of the FILETIME.
            public uint dwLowDateTime;

            // Summary:
            //     Specifies the high 32 bits of the FILETIME.
            public uint dwHighDateTime;

            public DateTime DateTime
            {
                get
                {
                    ulong h = dwHighDateTime;
                    h <<= 32;
                    ulong l = dwLowDateTime;
                    return DateTime.FromFileTime((long)(h | l));
                }
            }

            public DateTime DateTimeUtc
            {
                get
                {
                    ulong h = dwHighDateTime;
                    h <<= 32;
                    ulong l = dwLowDateTime;
                    return DateTime.FromFileTimeUtc((long)(h | l));
                }
            }
        }

        internal System.IO.FileAttributes Attributes;
        internal FILETIME ftCreationTime;
        internal FILETIME ftLastAccessTime;
        internal FILETIME ftLastWriteTime;
        internal int nFileSizeHigh;
        internal int nFileSizeLow;
        internal int dwReserved0;
        internal int dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH)]
        internal string Name;
        // not using this
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        internal string cAlternate;

        public bool Equals(FindData other)
        {
            return
                Attributes == other.Attributes &&
                Equals(ftCreationTime, other.ftCreationTime) && 
                Equals(ftLastAccessTime, other.ftLastAccessTime) && 
                Equals(ftLastWriteTime, other.ftLastWriteTime) &&
                nFileSizeHigh == other.nFileSizeHigh &&
                nFileSizeLow == other.nFileSizeLow &&
                dwReserved0 == other.dwReserved0 &&
                dwReserved1 == other.dwReserved1 &&
                Name == other.Name &&
                cAlternate == other.cAlternate;
        }

        static bool Equals(FILETIME a, FILETIME b)
        {
            return a.dwHighDateTime == b.dwHighDateTime &&
                a.dwLowDateTime == b.dwLowDateTime;
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
