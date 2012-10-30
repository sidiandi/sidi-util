﻿using System;
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

        public static Tree Get(Path dir)
        {
            return GetRecursive(null, new FileSystemInfo(dir));
        }

        public static Tree GetBackground(Path dir)
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
            var i = (Sidi.IO.FileSystemInfo) t.Object;
            if (i.IsDirectory)
            {
                foreach (var x in i.GetFileSystemInfos())
                {
                    new Tree(t){ Object = x };
                }

                foreach (var c in t.Children)
                {
                    GetChildrenRecursive(c);
                }
            }
        }

        static Tree GetRecursive(Tree parent, Sidi.IO.FileSystemInfo i)
        {
            var t = new Tree(parent) { Object = i };
            if (i.IsDirectory)
            {
                log.Info(i);
                
                foreach (var x in i.GetFileSystemInfos())
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