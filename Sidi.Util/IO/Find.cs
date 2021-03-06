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
            GetChildren = x => x.GetChildren();
        }

        /// <summary>
        /// Searches fileName in directory first, then in all parent directories
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        /// <returns>Path of found file, null if no file was found.</returns>
        public static LPath SearchUpwards(LPath directory, string fileName)
        {
            if (directory == null)
            {
                return null;
            }

            return directory.Lineage
                .Select(d => d.CatDir(fileName))
                .FirstOrDefault(_ => _.Exists);
        }

        public static IEnumerable<IFileSystemInfo> AllFiles(LPath root)
        {
            var e = new Find()
            {
                Root = root,
                Output = Find.OnlyFiles,
            };
            return e.Depth();
        }

        public static IEnumerable<IFileSystemInfo> AllFiles(IEnumerable<LPath> roots)
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
        public Func<IFileSystemInfo, bool> Output { set; get; }

        /// <summary>
        /// Set this function to decide which directories should be followed.
        /// </summary>
        public Func<IFileSystemInfo, bool> Follow { set; get; }

        /// <summary>
        /// Set this function to see every file, not matter if output or not.
        /// </summary>
        public Action<IFileSystemInfo> Visit { set; get; }

        /// <summary>
        /// Set this function to determine how the child elements of a file system element are determined
        /// and in which order they are presented to the searcher. Default calls IFileSystemInfo.GetChildren
        /// </summary>
        public Func<IFileSystemInfo, IEnumerable<IFileSystemInfo> > GetChildren { set; get; }

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
        public IEnumerable<IFileSystemInfo> Depth()
        {
            Count = 0;
            var stack = Roots.Where(x => x.Exists).Select(x => x.Info).ToList();

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
                    stack.InsertRange(0, GetChildren(i));
                }
            }
        }

        /// <summary>
        /// Recurses all start roots breadth-first
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IFileSystemInfo> Breadth()
        {
            Count = 0;
            var stack = new List<IFileSystemInfo>(Roots.Where(x => x.Exists).Select(x => x.Info));

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
                    stack.InsertRange(stack.Count, i.GetChildren());
                }
            }
        }

        DateTime nextReport = DateTime.MinValue;

        bool MustFollow(IFileSystemInfo i)
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
        public static bool OnlyFiles(IFileSystemInfo i)
        {
            return !i.IsDirectory;
        }

        /// <summary>
        /// Only follows elements which do not start with . and are not hidden
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static bool NoDotNoHidden(IFileSystemInfo i)
        {
            return !i.IsHidden && !i.Name.StartsWith(".");
        }
    }
}
