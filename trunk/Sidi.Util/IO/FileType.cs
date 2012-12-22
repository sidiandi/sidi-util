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

namespace Sidi.IO
{
    public class FileType
    {
        /// <summary>
        /// Specify all extensions you want to match
        /// </summary>
        /// <param name="extensions">list of extensions without "."</param>
        public FileType(params string[] extensions)
        {
            e = new HashSet<string>(extensions.Select(x => "." + x.ToLower()));
        }

        HashSet<string> e;

        public bool Is(LPath fileName)
        {
            return e.Contains(fileName.Extension.ToLower());
        }
    }

}
