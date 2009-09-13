using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sidi.Util;

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
            dest.EnsureParentDirectoryExists();
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

            FileInfo si = new FileInfo(source);
            FileInfo di = new FileInfo(dest);
            return !(
                !di.Exists ||
                si.LastWriteTime != di.LastWriteTime ||
                si.Length != di.Length);
        }

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
                File.Copy(source, dest, true);
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
