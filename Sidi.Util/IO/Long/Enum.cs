using System;
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

        public bool Is(Path fileName)
        {
            return e.Contains(fileName.Extension.ToLower());
        }
    }

    public class FileEnum
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FileEnum()
        {
            Visit = x => { };
            Follow = x => true;
            Output = x => true;
        }

        public static IEnumerable<FileSystemInfo> AllFiles(Path root)
        {
            var e = new FileEnum()
            {
                Root = root,
                Output = FileEnum.OnlyFiles,
            };
            return e.Depth();
        }
        
        /// <summary>
        /// Set this function to decide which files should be returned.
        /// </summary>
        public Func<FileSystemInfo, bool> Output { set; get; }

        /// <summary>
        /// Set this function to decide which directories should be followed.
        /// </summary>
        public Func<FileSystemInfo, bool> Follow { set; get; }

        /// <summary>
        /// Set this function to see every file, not matter if output or not.
        /// </summary>
        public Action<FileSystemInfo> Visit { set; get; }

        /// <summary>
        /// Counts the visited files
        /// </summary>
        public int Count { private set; get; }
        
        /// <summary>
        /// List of start paths for Depth() and Breath(). Multiple start roots are supported.
        /// </summary>
        public IList<Path> Roots = new List<Path>();

        /// <summary>
        /// Sets a single root path. Roots will then have exactly one element.
        /// </summary>
        public Path Root
        {
            set
            {
                Roots = new List<Path>() { value };
            }
        }

        /// <summary>
        /// Recurses all start roots depth-first
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileSystemInfo> Depth()
        {
            Count = 0;
            var stack = new List<FileSystemInfo>(Roots.Where(x => x.Exists).Select(x => x.Info));

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
        public IEnumerable<FileSystemInfo> Breadth()
        {
            Count = 0;
            var stack = new List<FileSystemInfo>(Roots.Where(x => x.Exists).Select(x => x.Info));

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

        bool MustFollow(FileSystemInfo i)
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
