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
using System.Text;
using System.IO;
using Sidi.Util;
using Sidi.Extensions;
using L = Sidi.IO;

namespace Sidi.IO
{
    public class Copy
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public Copy()
        {
            CopyCondition = x => true;
        }

        public void CopyToDir(IEnumerable<LPath> files, LPath destinationDir)
        {
            foreach (var s in files)
            {
                string d = destinationDir.CatDir(s.FileName);
                FastCopy(s, d);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="destinationDir"></param>
        /// <returns>The destination path of the file</returns>
        public string CopyToDir(LPath s, LPath destinationDir)
        {
            string d = destinationDir.CatDir(s.FileName);
            FastCopy(s, d);
            return d;
        }

        public bool FastCopy(LPath source, LPath dest)
        {
            dest.EnsureParentDirectoryExists();
            return FastCopyNoCreateDir(source, dest);
        }

        public Func<LPath, bool> CopyCondition { get; set; }

        /// <summary>
        /// Used by FastCopy to decide if a file must be copied or can be skipped
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public virtual bool CanSkipCopy(LPath source, LPath dest)
        {
            if (!CopyCondition(source))
            {
                return true;
            }

            return FileUtil.FilesAreEqualByTime(source, dest);
        }

        public string RenameLockedDestinationFilesExtension = "renamed-to-overwrite";

        public bool FastCopyNoCreateDir(LPath source, LPath dest)
        {
            var si = source.Info;
            var di = dest.Info;
            bool copied = false;

            if (!CanSkipCopy(source, dest))
            {
                if (di.Exists && OverwriteReadOnly)
                {
                    var destFileInfo = dest.Info;
                    if (destFileInfo.IsReadOnly)
                    {
                        destFileInfo.IsReadOnly = false;
                    }
                }

                try
                {
                    LFile.Copy(source, dest, true);
                }
                catch (Exception ex)
                {
                    if (RenameLockedDestinationFiles)
                    {
                        string oldFile = String.Empty;
                        for (int unique = 0; unique < 100; ++unique)
                        {
                            oldFile = String.Format("{0}.{1}.{2}", dest, unique, RenameLockedDestinationFilesExtension);
                            if (!LFile.Exists(oldFile))
                            {
                                break;
                            }
                        }
                        log.InfoFormat("Move {0} -> {1}", dest, oldFile);
                        LFile.Move(dest, oldFile);
                        LFile.Copy(source, dest, true);
                    }
                    else
                    {
                        log.Error("Cannot copy {0} -> {1}".F(source, dest), ex);
                        throw;
                    }
                }

                if (MakeWritable)
                {
                    new FileInfo(dest).IsReadOnly = false;
                }

                log.InfoFormat("Copy {0} -> {1}", source, dest);
                copied = true;
            }
            else
            {
                log.InfoFormat("Skip: {0} -> {1}", source, dest);
            }
            return copied;
        }

        public bool OverwriteReadOnly
        {
            get;
            set;
        }

        public bool MakeWritable { get; set; }
        public bool RenameLockedDestinationFiles { get; set; }

        public void CopyRecursive(string source, string dest)
        {
            CopyRecursive(source, dest, 0);
        }

        public void CopyRecursive(LPath source, LPath dest, int numberOfCopiedDirectoryLevels)
        {
            string[] parts = source.Parts;
            if (parts.Length < numberOfCopiedDirectoryLevels)
            {
                throw new ArgumentOutOfRangeException("numberOfCopiedDirectoryLevels", numberOfCopiedDirectoryLevels, "must be equal or less the number of directories in source");
            }

            for (int i = 0; i < parts.Length; ++i)
            {
                if (i >= parts.Length - numberOfCopiedDirectoryLevels)
                {
                    dest = dest.CatDir(parts[i]);
                }
            }

            if (source.IsDirectory)
            {
                var d = source.Info;

                if (d.IsHidden)
                {
                    return;
                }

                log.Info(d.FullName);
                foreach (LFileSystemInfo i in d.GetFileSystemInfos())
                {
                    CopyRecursive(i.FullName, dest.CatDir(i.Name));
                }
            }
            else
            {
                FastCopy(source, dest);
            }
        }

        public void IncrementalCopy(L.LPath source, L.LPath dest, L.LPath existingCopy)
        {
            foreach (var sInfo in L.Find.AllFiles(source))
            {
                var s = sInfo.FullName;
                var relativePath = s.GetRelative(source);
                var d = dest.CatDir(relativePath);
                var e = existingCopy.CatDir(relativePath);

                if (sInfo.IsDirectory)
                {
                    Sidi.IO.LDirectory.Create(d);
                }
                else
                {
                    if (CopyCondition(s))
                    {
                        if (FileUtil.FilesAreEqualByTime(s, e))
                        {
                            log.InfoFormat("Link {0} -> {1}", d, e);
                            FileUtil.CreateHardLink(d, e);
                        }
                        else
                        {
                            log.InfoFormat("Copy {0} -> {1}", s, d);
                            LFile.Copy(s, d);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Delete all files in directory dir. Does not recurse into sub directories.
        /// </summary>
        /// <param name="dir"></param>
        public void DeleteAllFilesIn(LPath dir)
        {
            if (dir.IsDirectory)
            {
                log.InfoFormat("Deleting all files in {0}", dir);
                foreach (var i in dir.Children)
                {
                    try
                    {
                        Sidi.IO.LFile.Delete(i);
                    }
                    catch (System.UnauthorizedAccessException)
                    {
                        // try harder
                        log.WarnFormat("Removing read-only protection from {0}", i);
                        var info = i.Info;
                        if (info.IsReadOnly)
                        {
                            info.IsReadOnly = false;
                        }
                        Sidi.IO.LFile.Delete(i);
                    }
                }
            }
        }
    }
}
