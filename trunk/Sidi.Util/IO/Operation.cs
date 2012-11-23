using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Extensions;

namespace Sidi.IO
{
    /// <summary>
    /// File System Operations
    /// </summary>
    public class Operation
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Operation()
        {
            Simulate = false;
            Overwrite = false;
        }
        
        /// <summary>
        /// Moves files or directory trees. Falls back to copying if moving is not possible
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void Move(LPath from, LPath to)
        {
            if (!LPath.IsSameFileSystem(from, to))
            {
                log.InfoFormat("{0} and {1} are on different file systems. Copying instead of moving", from, to);
                Copy(from, to);
                Delete(from);
                return;
            }

            if (from.IsDirectory)
            {
                log.InfoFormat("{0}move directory {1} to {2}", this, from, to);
                if (!Simulate)
                {
                    to.EnsureParentDirectoryExists();
                    if (Overwrite)
                    {
                        Delete(to);
                    }
                    LDirectory.Move(from, to);
                }
            }
            else
            {
                log.InfoFormat("{0}move file {1} to {2}", this, from, to);
                if (!Simulate)
                {
                    to.EnsureParentDirectoryExists();
                    if (Overwrite)
                    {
                        Delete(to);
                    }
                    LFile.Move(from, to);
                }
            }
        }

        /// <summary>
        /// Creates a hard link of a file or a directory tree containing hard links.
        /// Falls back to copying if hard linking is not possible.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void Link(LPath from, LPath to)
        {
            if (!from.Exists)
            {
                throw new Exception(String.Format("{0} does not exist.", from));
            }

            if (!Simulate)
            {
                to.EnsureParentDirectoryExists();
            }

            LinkInternal(from, to);
        }

        public void LinkInternal(LPath from, LPath to)
        {
            if (from.IsDirectory)
            {
                if (!Simulate)
                {
                    to.EnsureDirectoryExists();
                }

                foreach (var c in from.Children)
                {
                    Copy(c, to.CatDir(c.FileName));
                }
            }
            else
            {
                log.InfoFormat("{0}link file {1} to {2}", this, from, to);
                if (!Simulate)
                {
                    LFile.CreateHardLink(to, from);
                }
            }
        }

        public void Copy(LPath from, LPath to)
        {
            if (!from.Exists)
            {
                throw new Exception(String.Format("{0} does not exist.", from));
            }

            if (!Simulate)
            {
                to.EnsureParentDirectoryExists();
            }

            CopyInternal(from, to);
        }

        public void CopyInternal(LPath from, LPath to)
        {
            if (from.IsDirectory)
            {
                if (!Simulate)
                {
                    to.EnsureDirectoryExists();
                }

                foreach (var c in from.Children)
                {
                    Copy(c, to.CatDir(c.FileName));
                }
            }
            else
            {
                log.InfoFormat("{0}copy file {1} to {2}", this, from, to);
                if (!Simulate)
                {
                    LFile.Copy(from, to, Overwrite);
                }
            }
        }

        public void DeleteEmptyDirectories(LPath tree)
        {
            if (tree.IsDirectory)
            {
                foreach (var d in tree.GetDirectories())
                {
                    DeleteEmptyDirectories(d);
                }
                LDirectory.Delete(tree);
            }
        }

        public void Delete(LPath tree)
        {
            if (tree.IsDirectory)
            {
                foreach (var c in tree.Children)
                {
                    Delete(c);
                }
            }
            DeleteElement(tree);
        }

        void DeleteElement(LPath path)
        {
            if (path.IsDirectory)
            {
                log.InfoFormat("{0}delete directory {1}", this, path);
                if (!Simulate)
                {
                    LDirectory.Delete(path);
                }
            }
            else if (path.IsFile)
            {
                log.InfoFormat("{0}delete file {1}", this, path);
                if (!Simulate)
                {
                    LFile.Delete(path);
                }
            }
        }

        public override string ToString()
        {
            return OptionsText;
        }

        public string OptionsText
        {
            get
            {
                var flags =
                    (Simulate ? new string[] { "Simulate" } : new string[] { })
                    .Concat((Overwrite ? new string[] { "Overwrite" } : new string[] { }))
                    .Join(",");

                if (String.IsNullOrEmpty(flags))
                {
                    return String.Empty;
                }
                else
                {
                    return "[" + flags + "] ";
                }
            }
        }

        public void EnsureParentDirectoryExists(LPath path)
        {
        }

        public void EnsureDirectoryExists(LPath path)
        {
        }

        public bool Simulate { set; get; }

        public bool Overwrite { get; set; }

        /// <summary>
        /// true, if p1 and p2 are on the same file system and can be moved
        /// or linked efficiently 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool CanMove(LPath p1, LPath p2)
        {
            return LPath.IsSameFileSystem(p1, p2);
        }
    }
}
