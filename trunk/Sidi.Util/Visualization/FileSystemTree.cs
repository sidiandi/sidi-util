using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;

namespace Sidi.Visualization
{
    public class FileSystemTree
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Tree<Sidi.IO.Long.FileSystemInfo> Get(Sidi.IO.Long.Path dir)
        {
            return GetRecursive(null, new Sidi.IO.Long.FileSystemInfo(dir));
        }

        public static Tree<Sidi.IO.Long.FileSystemInfo> GetBackground(Sidi.IO.Long.Path dir)
        {
            var t = new Tree<Sidi.IO.Long.FileSystemInfo>(null) { Data = new IO.Long.FileSystemInfo(dir) };
            t.Size = t.Data.Length;

            var filler = new Thread(() =>
                {
                    GetChildrenRecursive(t);
                });
            filler.Start();
            return t;
        }

        static void GetChildrenRecursive(Tree<Sidi.IO.Long.FileSystemInfo> t)
        {
            log.Info(t.Data);    
            var i = t.Data;
            if (i.IsDirectory)
            {
                t.Children = i.GetFileSystemInfos()
                    .Select(x => new Tree<Sidi.IO.Long.FileSystemInfo>(t){ Data = x, Size = x.Length })
                    .OrderByDescending(x => x.Size)
                    .ToList();

                foreach (var c in t.Children)
                {
                    GetChildrenRecursive(c);
                }
            }
        }

        static Tree<Sidi.IO.Long.FileSystemInfo> GetRecursive(
            Tree<Sidi.IO.Long.FileSystemInfo> parent,
            Sidi.IO.Long.FileSystemInfo i)
        {
            var t = new Tree<Sidi.IO.Long.FileSystemInfo>(parent);
            t.Data = i;
            if (i.IsDirectory)
            {
                log.Info(i);
                t.Children = i.GetFileSystemInfos()
                    .Select(x => GetRecursive(t, x))
                    .OrderByDescending(x => x.Size)
                    .ToList();
                t.Size = t.ChildSize;
            }
            else
            {
                t.Size = i.Length;
            }
            return t;
        }

        public static Color ExtensionToColor(Sidi.IO.Long.FileSystemInfo i)
        {
            return ColorScale.ToColor(i.Extension);
        }
    }
}
