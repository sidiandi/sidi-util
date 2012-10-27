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
using L = Sidi.IO.Long;

namespace Sidi.IO
{
    public class Copy
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public Copy()
        {
            CopyCondition = x => true;
        }

        public void CopyToDir(IEnumerable<string> files, string destinationDir)
        {
            foreach (string s in files)
            {
                string d = Path.Combine(destinationDir, Path.GetFileName(s));
                FastCopy(s, d);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="destinationDir"></param>
        /// <returns>The destination path of the file</returns>
        public string CopyToDir(string s, string destinationDir)
        {
            string d = Path.Combine(destinationDir, Path.GetFileName(s));
            FastCopy(s, d);
            return d;
        }

        public bool FastCopy(string source, string dest)
        {
            new L.Path(dest).EnsureParentDirectoryExists();
            return FastCopyNoCreateDir(source, dest);
        }

        public Func<string, bool> CopyCondition { get; set; }

        /// <summary>
        /// Used by FastCopy to decide if a file must be copied or can be skipped
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public virtual bool CanSkipCopy(string source, string dest)
        {
            if (!CopyCondition(source))
            {
                return true;
            }

            return FileUtil.FilesAreEqualByTime(source, dest);
        }

        public string RenameLockedDestinationFilesExtension = "renamed-to-overwrite";

        public bool FastCopyNoCreateDir(string source, string dest)
        {
            FileInfo si = new FileInfo(source);
            FileInfo di = new FileInfo(dest);
            bool copied = false;

            if (!CanSkipCopy(source, dest))
            {
                if (di.Exists && OverwriteReadOnly)
                {
                    FileInfo destFileInfo = new FileInfo(dest);
                    if ((destFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        destFileInfo.Attributes = (FileAttributes)(destFileInfo.Attributes - FileAttributes.ReadOnly);
                    }
                }

                try
                {
                    File.Copy(source, dest, true);
                }
                catch (Exception ex)
                {
                    if (RenameLockedDestinationFiles)
                    {
                        string oldFile = String.Empty;
                        for (int unique = 0; unique < 100; ++unique)
                        {
                            oldFile = String.Format("{0}.{1}.{2}", dest, unique, RenameLockedDestinationFilesExtension);
                            if (!File.Exists(oldFile))
                            {
                                break;
                            }
                        }
                        log.InfoFormat("Move {0} -> {1}", dest, oldFile);
                        File.Move(dest, oldFile);
                        File.Copy(source, dest, true);
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

                log.InfoFormat("Copy {0} -> {1}", source.Printable(), dest.Printable());
                copied = true;
            }
            else
            {
                log.InfoFormat("Skip: {0} -> {1}", source.Printable(), dest.Printable());
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

        public void CopyRecursive(string source, string dest, int numberOfCopiedDirectoryLevels)
        {
            string[] parts = source.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            if (parts.Length < numberOfCopiedDirectoryLevels)
            {
                throw new ArgumentOutOfRangeException("numberOfCopiedDirectoryLevels", numberOfCopiedDirectoryLevels, "must be equal or less the number of directories in source");
            }

            for (int i = 0; i < parts.Length; ++i)
            {
                if (i >= parts.Length - numberOfCopiedDirectoryLevels)
                {
                    dest = Path.Combine(dest, parts[i]);
                }
            }

            if (Directory.Exists(source))
            {
                DirectoryInfo d = new DirectoryInfo(source);

                if ((d.Attributes & FileAttributes.Hidden) != 0)
                {
                    return;
                }

                log.Info(d.FullName);
                foreach (FileSystemInfo i in d.GetFileSystemInfos())
                {
                    CopyRecursive(i.FullName, Path.Combine(dest, i.Name));
                }
            }
            else
            {
                FastCopy(source, dest);
            }
        }

        public void IncrementalCopy(L.Path source, L.Path dest, L.Path existingCopy)
        {
            foreach (var sInfo in L.FileEnumerator.AllFiles(source))
            {
                var s = sInfo.FullName;
                var relativePath = s.GetRelative(source);
                var d = dest.CatDir(relativePath);
                var e = existingCopy.CatDir(relativePath);

                if (sInfo.IsDirectory)
                {
                    Directory.CreateDirectory(d);
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
                            File.Copy(s, d);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Delete all files in directory dir. Does not recurse into sub directories.
        /// </summary>
        /// <param name="dir"></param>
        public void DeleteAllFilesIn(string dir)
        {
            if (Directory.Exists(dir))
            {
                log.InfoFormat("Deleting all files in {0}", dir);
                foreach (string i in Directory.GetFiles(dir))
                {
                    try
                    {
                        File.Delete(i);
                    }
                    catch (System.UnauthorizedAccessException)
                    {
                        // try harder
                        log.WarnFormat("Removing read-only protection from {0}", i);
                        FileInfo fi = new FileInfo(i);
                        fi.Attributes = fi.Attributes & (~FileAttributes.ReadOnly);
                        File.Delete(i);
                    }
                }
            }
        }
    }
}
