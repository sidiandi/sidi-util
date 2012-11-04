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
using Sidi.Util;
using System.IO;
using Sidi.CommandLine;

namespace Sidi.IO
{
    public class Find
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Find()
        {
            Visit = x => { };
            Follow = x => true;
            Output = x => true;
        }

        public static IEnumerable<LFileSystemInfo> AllFiles(LPath root)
        {
            var e = new Find()
            {
                Root = root,
                Output = Find.OnlyFiles,
            };
            return e.Depth();
        }

        public static IEnumerable<LFileSystemInfo> AllFiles(IEnumerable<LPath> roots)
        {
            var e = new Find()
            {
                Roots = roots.ToList(),
                Output = Find.OnlyFiles,
            };
            return e.Depth();
        }

        public static Find Parse(string text)
        {
            var list = PathList.Parse(text);
            return new Find()
            {
                Output = OnlyFiles,
                Roots = list
            };
        }
        
        /// <summary>
        /// Set this function to decide which files should be returned.
        /// </summary>
        public Func<LFileSystemInfo, bool> Output { set; get; }

        /// <summary>
        /// Set this function to decide which directories should be followed.
        /// </summary>
        public Func<LFileSystemInfo, bool> Follow { set; get; }

        /// <summary>
        /// Set this function to see every file, not matter if output or not.
        /// </summary>
        public Action<LFileSystemInfo> Visit { set; get; }

        /// <summary>
        /// Counts the visited files
        /// </summary>
        public int Count { private set; get; }
        
        /// <summary>
        /// List of start paths for Depth() and Breath(). Multiple start roots are supported.
        /// </summary>
        public IList<LPath> Roots = new List<LPath>();

        /// <summary>
        /// Sets a single root path. Roots will then have exactly one element.
        /// </summary>
        public LPath Root
        {
            set
            {
                Roots = new List<LPath>() { value };
            }
        }

        /// <summary>
        /// Recurses all start roots depth-first
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LFileSystemInfo> Depth()
        {
            Count = 0;
            var stack = new List<LFileSystemInfo>(Roots.Where(x => x.Exists).Select(x => x.Info));

            for (; stack.Count > 0; )
            {
                var i = stack.First();
                stack.RemoveAt(0);

                Visit(i);

                if (Output(i))
                {
                    yield return i;
                }

                if (MustFollow(i))
                {
                    stack.InsertRange(0, i.GetFileSystemInfos());
                }
            }
        }

        /// <summary>
        /// Recurses all start roots breadth-first
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LFileSystemInfo> Breadth()
        {
            Count = 0;
            var stack = new List<LFileSystemInfo>(Roots.Where(x => x.Exists).Select(x => x.Info));

            for (; stack.Count > 0; )
            {
                var i = stack.First();
                stack.RemoveAt(0);

                Visit(i);

                if (Output(i))
                {
                    yield return i;
                    ++Count;
                }

                if (i.IsDirectory && Follow(i))
                {
                    stack.InsertRange(stack.Count, i.GetFileSystemInfos());
                }
            }
        }

        DateTime nextReport = DateTime.MinValue;

        bool MustFollow(LFileSystemInfo i)
        {
            var f = i.IsDirectory && Follow(i);
            if (f)
            {
                log.DebugFormat("Follow {0}", i);
            }
            return f;
        }

        /// <summary>
        /// Follows files only
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static bool OnlyFiles(LFileSystemInfo i)
        {
            return !i.IsDirectory;
        }

        /// <summary>
        /// Only follows elements which do not start with . and are not hidden
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static bool NoDotNoHidden(LFileSystemInfo i)
        {
            return !i.Hidden && !i.Name.StartsWith(".");
        }
    }
}
