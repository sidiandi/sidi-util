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
using System.IO;

namespace Sidi.IO
{
    public class SearchPath
    {
        public List<string> Paths;

        public SearchPath()
        {
            Paths = new List<string>();
        }

        public void AppendIfNotExist(string dir)
        {
            if (Paths.Contains(dir))
            {
                return;
            }
            Paths.Add(dir);
        }

        public void PrependIfNotExist(string dir)
        {
            if (Paths.Contains(dir))
            {
                return;
            }
            Paths.Insert(0, dir);
        }

        public static SearchPath Parse(string semiColonSeparatedPath)
        {
            SearchPath p = new SearchPath();
            p.Paths = new List<string>(semiColonSeparatedPath.SplitPathList());
            return p;
        }

        public override string ToString()
        {
            return Paths.Aggregate((x, y) => x + ";" + y);
        }

        public string NewLineSeparatedString
        {
            get
            {
                return Paths.Aggregate((x, y) => x + "\r\n" + y);
            }
        }

        public string Find(string file)
        {
            string path;
            if (TryFind(file, out path))
            {
                return path;
            }

            StringBuilder b = new StringBuilder();
            foreach (string i in Paths)
            {
                b.AppendLine(FileUtil.CatDir(i, file));
            }

            throw new System.IO.FileNotFoundException(b.ToString(), file);
        }

        public bool TryFind(string file, out string path)
        {
            foreach (string i in Paths)
            {
                string p = FileUtil.CatDir(i, file);
                if (File.Exists(p))
                {
                    path = p;
                    return true;
                }
            }
            path = null;
            return false;
        }
    }
}
