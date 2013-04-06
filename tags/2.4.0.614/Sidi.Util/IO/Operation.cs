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
            Fast = true;
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
                    EnsureParentDirectoryExists(to);
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
                    EnsureParentDirectoryExists(to);
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
                EnsureParentDirectoryExists(to);
            }

            LinkInternal(from, to);
        }

        public void LinkInternal(LPath from, LPath to)
        {
            if (from.IsDirectory)
            {
                if (!Simulate)
                {
                    EnsureDirectoryExists(to);
                }

                foreach (var c in from.Children)
                {
                    LinkInternal(c, to.CatDir(c.FileName));
                }
            }
            else
            {
                LinkFile(from, to);
            }
        }

        void LinkFile(LPath from, LPath to)
        {
            log.InfoFormat("{0}link file {1} to {2}", OptionsText, from, to);
            if (!Simulate)
            {
                Count++;
                if (Overwrite)
                {
                    if (to.IsFile)
                    {
                        LFile.Delete(to);
                    }
                }
                LFile.CreateHardLink(to, from);
            }
        }

        /// <summary>
        /// Copies a file or directory tree.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void Copy(LPath from, LPath to)
        {
            if (!from.Exists)
            {
                throw new Exception(String.Format("{0} does not exist.", from));
            }

            if (!Simulate)
            {
                EnsureParentDirectoryExists(to);
            }

            Count = 0;

            CopyInternal(from, to);
        }

        public int Count { get; private set; }

        void CopyInternal(LPath from, LPath to)
        {
            if (from.IsDirectory)
            {
                if (!Simulate)
                {
                    EnsureDirectoryExists(to);
                }

                foreach (var c in from.Children)
                {
                    CopyInternal(c, to.CatDir(c.FileName));
                }
            }
            else
            {
                CopyFile(from, to);
            }
        }

        void CopyFile(LPath from, LPath to)
        {
            if (NeedCopy(from, to))
            {
                log.InfoFormat("{0}copy file {1} to {2}", OptionsText, from, to);
                if (!Simulate)
                {
                    Count++;
                    LFile.Copy(from, to, Overwrite);
                }
            }
            else
            {
                log.InfoFormat("{0}skip copy file {1} to {2}", OptionsText, from, to);
            }
        }

        Func<LPath, LPath, bool> NeedCopy;

        /// <summary>
        /// Deletes directory tree and all subdirectories if they are empty
        /// </summary>
        /// <param name="tree"></param>
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

        /// <summary>
        /// Deletes file or directory tree and all files and subdirectories below.
        /// </summary>
        /// <param name="tree"></param>
        public void Delete(LPath tree)
        {
            Count = 0;
            DeleteInternal(tree);
        }

        /// <summary>
        /// Deletes file or directory tree and all files and subdirectories below.
        /// </summary>
        /// <param name="tree"></param>
        public void DeleteInternal(LPath tree)
        {
            if (tree.IsDirectory)
            {
                foreach (var c in tree.Children)
                {
                    DeleteInternal(c);
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
                    Count++;
                    LDirectory.Delete(path);
                }
            }
            else if (path.IsFile)
            {
                log.InfoFormat("{0}delete file {1}", this, path);
                if (!Simulate)
                {
                    Count++;
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
                    .Concat((Fast ? new string[] { "Fast" } : new string[] { }))
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

        /// <summary>
        /// Ensures that the parent directory of path exists.
        /// </summary>
        /// <param name="path"></param>
        public void EnsureParentDirectoryExists(LPath path)
        {
            EnsureDirectoryExists(path.Parent);
        }

        /// <summary>
        /// Ensures that a directory exists at path.
        /// </summary>
        /// <param name="path"></param>
        public void EnsureDirectoryExists(LPath path)
        {
            if (!path.IsDirectory)
            {
                log.InfoFormat("{0}create directory {1}", OptionsText, path);
                if (!Simulate)
                {
                    LDirectory.Create(path);
                }
            }
        }

        /// <summary>
        /// When true, file system is not modified, but would-be modifications are logged.
        /// </summary>
        public bool Simulate { set; get; }

        /// <summary>
        /// When true, overwrites existing files
        /// </summary>
        public bool Overwrite { get; set; }

        /// <summary>
        /// If true, copying is skipped when length and last modified time of 
        /// source and target are identical
        /// </summary>
        public bool Fast
        {
            get
            {
                return fast;
            }

            set
            {
                fast = value;
                if (fast)
                {
                    NeedCopy = (from, to) => !LFile.EqualByTimeAndLength(from, to);
                }
                else
                {
                    NeedCopy = (from, to) => true;
                }
            }
        }

        bool fast;

        /// <summary>
        /// true, if p1 and p2 are on the same file system and can be moved
        /// or hard-linked efficiently 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool CanMove(LPath p1, LPath p2)
        {
            return LPath.IsSameFileSystem(p1, p2);
        }

        /// <summary>
        /// Copies from from to to. Tries to avoid making copies by 
        /// hard-linking existing files from existing to to.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="existing"></param>
        public void IncrementalCopy(LPath from, LPath to, LPath existing)
        {
            if (from.IsDirectory)
            {
                EnsureDirectoryExists(to);
                foreach (var i in from.Children)
                {
                    var fn = i.FileName;
                    IncrementalCopy(i, to.CatDir(fn), existing.CatDir(fn));
                }
            }
            else
            {
                if (LFile.EqualByTimeAndLength(from, existing))
                {
                    LinkFile(from, to);
                }
                else
                {
                    CopyFile(from, to);
                }
            }
        }

        public bool IsTreeIdentical(LPath a, LPath b)
        {
            if (a.IsDirectory)
            {
                var ca = a.Children;
                var cb = b.Children;
                if (ca.Count != cb.Count)
                {
                    return false;
                }
                return ca.All(i => IsTreeIdentical(i, b.CatDir(i.FileName)));
            }
            else
            {
                return LFile.EqualByTimeAndLength(a, b);
            }
        }
    }
}