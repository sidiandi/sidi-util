﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using System.IO;
using Sidi.CommandLine;

namespace Sidi.IO.Long
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

        public bool Is(string fileName)
        {
            return e.Contains(Path.GetExtension(fileName).ToLower());
        }
    }

    public class Enum
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Enum()
        {
            Follow = x => true;
            Output = x => true;
        }
        
        /// <summary>
        /// Set this function to decide which files should be returned.
        /// </summary>
        public Func<FileSystemInfo, bool> Output { set; get; }

        /// <summary>
        /// Set this function to decide which directories should be followed.
        /// </summary>
        public Func<FileSystemInfo, bool> Follow { set; get; } 

        List<FileSystemInfo> root = new List<FileSystemInfo>();

        /// <summary>
        /// Adds a start root for Depth() and Breath(). Multiple start roots are supported.
        /// </summary>
        /// <param name="path"></param>
        public void AddRoot(LongName path)
        {
            var startItem = new FileSystemInfo(path);
            log.Info(startItem);
            root.Add(startItem);
        }

        /// <summary>
        /// Recurses all start roots depth-first
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileSystemInfo> Depth()
        {
            var stack = new List<FileSystemInfo>(root);

            for (; stack.Count > 0; )
            {
                var i = stack.First();
                stack.RemoveAt(0);

                if (Output(i))
                {
                    yield return i;
                }

                if (MustFollow(i))
                {
                    stack.InsertRange(0, i.GetChilds());
                }
            }
        }

        /// <summary>
        /// Recurses all start roots breadth-first
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileSystemInfo> Breadth()
        {
            var stack = new List<FileSystemInfo>(root);

            for (; stack.Count > 0; )
            {
                var i = stack.First();
                stack.RemoveAt(0);

                if (Output(i))
                {
                    yield return i;
                }

                if (i.IsDirectory && Follow(i))
                {
                    log.InfoFormat("Directory {0}", i);
                    stack.InsertRange(stack.Count, i.GetChilds());
                }
            }
        }

        bool MustFollow(FileSystemInfo i)
        {
            var f = i.IsDirectory && Follow(i);
            if (f)
            {
                log.InfoFormat("Follow {0}", i);
            }
            return f;
        }

        /// <summary>
        /// Follows files only
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static bool OnlyFiles(FileSystemInfo i)
        {
            return !i.IsDirectory;
        }

        /// <summary>
        /// Only follows elements which do not start with . and are not hidden
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static bool NoDotNoHidden(FileSystemInfo i)
        {
            return !i.Hidden && !i.Name.StartsWith(".");
        }
    }
}
