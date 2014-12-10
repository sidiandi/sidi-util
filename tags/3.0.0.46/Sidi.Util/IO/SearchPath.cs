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
using System.IO;
using Sidi.Extensions;

namespace Sidi.IO
{
    public class SearchPath
    {
        public PathList Paths;

        public SearchPath()
        {
            Paths = new PathList();
        }

        public static SearchPath PATH
        {
            get
            {
                return SearchPath.Parse(Environment.GetEnvironmentVariable("PATH"));
            }
        }

        public void AppendIfNotExist(LPath dir)
        {
            if (Paths.Contains(dir))
            {
                return;
            }
            Paths.Add(dir);
        }

        public void PrependIfNotExist(LPath dir)
        {
            if (Paths.Contains(dir))
            {
                return;
            }
            Paths.Insert(0, dir);
        }

        public static SearchPath Parse(string semiColonSeparatedPath)
        {
            return new SearchPath()
            {
                Paths = PathList.Parse(semiColonSeparatedPath)
            };
        }

        public override string ToString()
        {
            return Paths.Join(";");
        }

        public LPath Find(LPath file)
        {
            LPath path;
            if (TryFind(file, out path))
            {
                return path;
            }

            throw new System.IO.FileNotFoundException(Paths.Select(x => x.CatDir(file)).Join());
        }

        public bool TryFind(LPath file, out LPath path)
        {
            path = Paths.Select(_ => _.CatDir(file))
                .FirstOrDefault(x => x.IsFile);
            return path != null;
        }
    }
}
