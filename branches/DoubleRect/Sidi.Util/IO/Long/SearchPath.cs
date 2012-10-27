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
using Sidi.Extensions;

namespace Sidi.IO.Long
{
    public class SearchPath
    {
        public List<Path> Paths;

        public SearchPath()
        {
            Paths = new List<Path>();
        }

        public static SearchPath PATH
        {
            get
            {
                return SearchPath.Parse(Environment.GetEnvironmentVariable("PATH"));
            }
        }

        public void AppendIfNotExist(Path dir)
        {
            if (Paths.Contains(dir))
            {
                return;
            }
            Paths.Add(dir);
        }

        public void PrependIfNotExist(Path dir)
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
            p.Paths = new List<Path>(semiColonSeparatedPath
                .SplitPathList()
                .Select(x => new Path(x))
                );
            return p;
        }

        public override string ToString()
        {
            return Paths.Join(";");
        }

        public Path Find(string file)
        {
            Path path;
            if (TryFind(file, out path))
            {
                return path;
            }

            throw new System.IO.FileNotFoundException(Paths.Select(x => x.CatDir(file)).Join());
        }

        public bool TryFind(string file, out Path path)
        {
            foreach (var i in Paths)
            {
                string p = i.CatDir(file);
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
