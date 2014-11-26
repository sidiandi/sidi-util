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
using System.Text;
using System.IO;
using System.Reflection;
using Sidi.Util;
using Sidi.Extensions;
using System.Runtime.InteropServices;
using System.Linq;
using L = Sidi.IO;

namespace Sidi.IO
{
    public static class FileUtil
    {
        /// <summary>
        /// Compares two files bytewise
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool FilesAreEqual(LPath a, LPath b)
        {
            if (a.IsFile)
            {
                if (b.IsFile)
                {
                    FileInfo ia = new FileInfo(a);
                    FileInfo ib = new FileInfo(b);
                    if (ia.Length != ib.Length)
                    {
                        return false;
                    }

                    Stream fa = null;
                    Stream fb = null;
                    try
                    {
                        fa = LFile.OpenRead(a);
                        fb = LFile.OpenRead(b);
                        int da;
                        int db;
                        do
                        {
                            da = fa.ReadByte();
                            db = fb.ReadByte();
                            if (da != db)
                            {
                                return false;
                            }
                        }
                        while (da != -1);

                        return true;
                    }
                    finally
                    {
                        if (fa != null) fa.Close();
                        if (fb != null) fb.Close();
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (b.IsFile)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public static bool FilesAreEqualByTime(LPath a, LPath b)
        {
            if (a.IsFile)
            {
                if (b.IsFile)
                {
                    var fa = new FileInfo(a);
                    var fb = new FileInfo(b);
                    return 
                        fa.Length == fb.Length &&
                        Math.Abs((fa.LastWriteTimeUtc - fb.LastWriteTimeUtc).TotalSeconds) < 2.0;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return !b.IsFile;
            }
        }
    }
}
