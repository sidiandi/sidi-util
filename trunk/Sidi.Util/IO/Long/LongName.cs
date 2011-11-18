using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using System.Text.RegularExpressions;

namespace Sidi.IO.Long
{
    public static class LongNameEx
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static LongName Long(this string x)
        {
            return new LongName(x);
        }

        static Regex invalidFilenameRegex = new Regex(
            System.IO.Path.GetInvalidFileNameChars()
            .Select(n => Regex.Escape(new String(n,1)))
            .Join("|"));

        public static string MakeFilename(this string x)
        {
            return Truncate(invalidFilenameRegex.Replace(x, "_"), LongName.MaxFilenameLength);
        }

        static string Truncate(string x, int maxLength)
        {
            if (x.Length > maxLength)
            {
                return x.Substring(0, maxLength);
            }
            else
            {
                return x;
            }
        }

        public static bool IsValidFilename(this string x)
        {
            return x.Length <= LongName.MaxFilenameLength && !invalidFilenameRegex.IsMatch(x);
        }

        public static void EnsureNotExists(this LongName ln)
        {
            FindData fd;
            if (ln.GetFindData(out fd))
            {
                if (fd.IsDirectory)
                {
                    foreach (var c in Directory.FindFile(ln.CatDir("*")).ToList())
                    {
                        var cn = ln.CatDir(c.Name);
                        if (c.IsDirectory)
                        {
                            cn.EnsureNotExists();
                        }
                        else
                        {
                            File.Delete(cn);
                        }
                    }
                    Directory.Delete(ln);
                }
                else
                {
                    File.Delete(ln);
                }
                log.InfoFormat("Delete {0}", ln);
            }
        }

        public static void EnsureParentDirectoryExists(this LongName ln)
        {
            var p = ln.ParentDirectory;
            if (!Directory.Exists(p))
            {
                Directory.Create(p);
            }
        }
    }

    public class LongName
    {
        const string longNamePrefix = @"\\?\";
        const string longUncPrefix = @"\\?\UNC\";
        const string shortUncPrefix = @"\\";

        public LongName(string path)
        {
            // remove trailing slash
            if (path.EndsWith(@"\"))
            {
                path = path.Substring(0, path.Length - 1);
            }
            
            if (path.StartsWith(longNamePrefix) || path.StartsWith(longUncPrefix))
            {
                this.path = path;
            }
            else if (path.StartsWith(shortUncPrefix))
            {
                this.path = longUncPrefix + path.Substring(shortUncPrefix.Length);
            }
            else
            {
                this.path = longNamePrefix + path;
            }
            Check();
        }

        public LongName(IEnumerable<string> parts)
        : this(parts.Join(DirectorySeparator))
        {
        }

        public LongName Canonic
        {
            get
            {
                return new LongName(Parts.Where(x => !x.Equals(".")));
            }
        }

        public LongName UniqueFileName()
        {
            if (!new FileSystemInfo(this).Exists)
            {
                return this;
            }

            for (int i = 1; i < 1000; ++i)
            {
                var u = new LongName(String.Format("{0}.{1}", this, i));
                if (!new FileSystemInfo(u).Exists)
                {
                    return u;
                }
            }
            throw new System.IO.IOException(String.Format("{0} cannot be made unique.", this));
        }

        public bool Exists
        {
            get
            {
                return new FileSystemInfo(this).Exists;
            }
        }

        internal bool GetFindData(out FindData fd)
        {
            if (IsRoot)
            {
                fd = new FindData();
                fd.Attributes = System.IO.FileAttributes.Directory;
                fd.nFileSizeHigh = 0;
                fd.nFileSizeLow = 0;
                return true;
            }

            using (var f = Directory.FindFileRaw(this).GetEnumerator())
            {
                if (f.MoveNext())
                {
                    fd = f.Current;
                    return true;
                }
                else
                {
                    fd = default(FindData);
                    return false;
                }
            }
        }

        internal FindData FindData
        {
            get
            {
                FindData fd;
                if (!GetFindData(out fd))
                {
                    throw new System.IO.IOException(this.ToString());
                }
                return fd;
            }
        }

        public static readonly char DirectorySeparatorChar = System.IO.Path.DirectorySeparatorChar;

        public string[] Parts
        {
            get
            {
                return this.path.Split(DirectorySeparatorChar).Skip(3).ToArray();
            }
        }

        
        public LongName CatDir(IEnumerable<string> parts)
        {
            return new LongName((new string[] { this.path }.Concat(parts)).Join(DirectorySeparator));
        }

        public LongName CatDir(params string[] parts)
        {
            return CatDir(parts.Cast<string>());
        }

        public LongName CatDir(params LongName[] parts)
        {
            return CatDir(parts.Select(x => x.NoPrefix).ToArray());
        }

        public const int MaxFilenameLength = 255;

        void Check()
        {
            if (this.path.Length > 32000)
            {
                throw new System.IO.PathTooLongException(this.path);
            }

            var parts = Parts;
            var tooLong = parts.FirstOrDefault(x => x.Length > MaxFilenameLength);
            if (tooLong != null)
            {
                throw new System.IO.PathTooLongException(tooLong);
            }
        }

        string path;

        public LongName ParentDirectory
        {
            get
            {
                if (IsRoot)
                {
                    return null;
                }
                else
                {
                    var i = path.LastIndexOf(DirectorySeparator);
                    return new LongName(path.Substring(0, i));
                }
            }
        }

        public string Name
        {
            get
            {
                var i = path.LastIndexOf(DirectorySeparator);
                var n = path.Substring(i + 1);
                if (String.IsNullOrEmpty(n))
                {
                    return ".";
                }
                else
                {
                    return n;
                }
            }
        }

        public const string DirectorySeparator = @"\";

        public string NoPrefix
        {
            get
            {
                if (path.StartsWith(longUncPrefix))
                {
                    return shortUncPrefix + path.Substring(longUncPrefix.Length);
                }
                else
                {
                    return path.Substring(longNamePrefix.Length);
                }
            }
        }

        public override string ToString()
        {
            return NoPrefix;
        }

        public string Param
        {
            get
            {
                return path;
            }
        }

        /// <summary>
        /// Returns true if ParentDirectory would return null
        /// </summary>
        public bool IsRoot
        {
            get
            {
                if (IsUnc)
                {
                    return Parts.Length <= 3;
                }
                else
                {
                    return IsDriveRoot;
                }
            }
        }

        public bool IsDriveRoot
        {
            get
            {
                return path.StartsWith(longNamePrefix) && path.EndsWith(":") && path.Length == longNamePrefix.Length + 2;
            }
        }

        public bool IsUnc
        {
            get
            {
                return path.StartsWith(longUncPrefix);
            }
        }

        public override int GetHashCode()
        {
            return Param.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is LongName)
            {
                return Param.Equals(((LongName)obj).Param);
            }
            else
            {
                return false;
            }
        }
    }

}
