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
using System.Drawing;
using System.Threading;
using Sidi.IO;

namespace Sidi.Visualization
{
    public class FileSystemTree
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Tree Get(LPath dir)
        {
            return GetRecursive(null, dir.Info);
        }

        public static Tree GetBackground(LPath dir)
        {
            var t = new Tree(null) { Object = dir.Info };

            var filler = new Thread(() =>
                {
                    GetChildrenRecursive(t);
                });
            filler.Start();
            return t;
        }

        static void GetChildrenRecursive(Tree t)
        {
            log.Info(t.Object);    
            var i = (Sidi.IO.LFileSystemInfo) t.Object;
            if (i.IsDirectory)
            {
                foreach (var x in i.GetChildren())
                {
                    new Tree(t){ Object = x };
                }

                foreach (var c in t.Children)
                {
                    GetChildrenRecursive(c);
                }
            }
        }

        static Tree GetRecursive(Tree parent, Sidi.IO.LFileSystemInfo i)
        {
            var t = new Tree(parent) { Object = i };
            if (i.IsDirectory)
            {
                log.Info(i);
                
                foreach (var x in i.GetChildren())
                {
                    GetRecursive(t, x);
                }
                t.Size = t.ChildSize;
            }
            else
            {
                t.Size = i.Length;
            }
            return t;
        }
    }
}
