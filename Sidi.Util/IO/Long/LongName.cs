using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using System.Text.RegularExpressions;

namespace Sidi.IO.Long
{
    public class Path
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const string pathPrefix = @"\\?\";
        const string longUncPrefix = @"\\?\UNC\";
        const string shortUncPrefix = @"\\";

        static Regex invalidFilenameRegex = new Regex(
            System.IO.Path.GetInvalidFileNameChars()
            .Select(n => Regex.Escape(new String(n, 1)))
            .Join("|"));

        public static string GetValidFilename(string x)
        {
            return Truncate(invalidFilenameRegex.Replace(x, "_"), Path.MaxFilenameLength);
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

        public static bool IsValidFilename(string x)
        {
            return x.Length <= Path.MaxFilenameLength && !invalidFilenameRegex.IsMatch(x);
        }

        public Path(string path)
        {
            // remove trailing slash
            if (path.EndsWith(@"\"))
            {
                path = path.Substring(0, path.Length - 1);
            }
            
            if (path.StartsWith(pathPrefix) || path.StartsWith(longUncPrefix))
            {
                this.path = path;
            }
            else if (path.StartsWith(shortUncPrefix))
            {
                this.path = longUncPrefix + path.Substring(shortUncPrefix.Length);
            }
            else
            {
                this.path = pathPrefix + path;
            }
            Check();
        }

        public Path(IEnumerable<string> parts)
        : this(parts.Join(DirectorySeparator))
        {
        }

        public Path Canonic
        {
            get
            {
                return new Path(Parts.Where(x => !x.Equals(".")));
            }
        }

        public Path UniqueFileName()
        {
            if (!new FileSystemInfo(this).Exists)
            {
                return this;
            }

            for (int i = 1; i < 1000; ++i)
            {
                var u = new Path(String.Format("{0}.{1}", this, i));
                if (!new FileSystemInfo(u).Exists)
                {
                    return u;
                }
            }
            throw new System.IO.IOException(String.Format("{0} cannot be made unique.", this));
        }

        public static Path Parse(string x)
        {
            return new Path(x);
        }

        public bool Exists
        {
            get
            {
                return new FileSystemInfo(this).Exists;
            }
        }

        public Path PathRoot
        {
            get
            {
                return new Path(Parts.Take(1));
            }
        }

        internal bool GetFindData(out FindData fd)
        {
            if (IsRoot)
            {
                fd = new FindData()
                {
                    Attributes = System.IO.FileAttributes.Directory,
                    nFileSizeHigh = 0,
                    nFileSizeLow = 0,
                    Name = this.NoPrefix,
                };
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

        public FileSystemInfo Info
        {
            get
            {
                return new FileSystemInfo(this);
            }
        }
        
        public Path CatDir(IEnumerable<string> parts)
        {
            return new Path((new string[] { this.path }.Concat(parts)).Join(DirectorySeparator));
        }

        public Path CatDir(params string[] parts)
        {
            return CatDir(parts.Cast<string>());
        }

        public Path CatDir(params Path[] parts)
        {
            return CatDir(parts.Select(x => x.NoPrefix).ToArray());
        }

        public Path CatName(string namePostfix)
        {
            return new Path(this.path + namePostfix);
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

        public Path ParentDirectory
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
                    return new Path(path.Substring(0, i));
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
                    return path.Substring(pathPrefix.Length);
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
                return path.StartsWith(pathPrefix) && path.EndsWith(":") && path.Length == pathPrefix.Length + 2;
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

        const StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase;

        public override bool Equals(object obj)
        {
            if (obj is Path)
            {
                return Param.Equals(((Path)obj).Param, stringComparison);
            }
            else
            {
                return false;
            }
        }

        public Path RelativeTo(Path root)
        {
            if (!path.StartsWith(root.path, stringComparison))
            {
                throw new ArgumentOutOfRangeException("root");
            }

            return new Path(path.Substring(root.path.Length + DirectorySeparator.Length));
        }

        public void EnsureNotExists()
        {
            var ln = this;
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

        public void EnsureParentDirectoryExists()
        {
            ParentDirectory.EnsureDirectoryExists();
        }

        public void EnsureDirectoryExists()
        {
            if (!Directory.Exists(this))
            {
                Directory.Create(this);
            }
        }

        public string Extension
        {
            get
            {
                return System.IO.Path.GetExtension(Name);
            }
        }

        public string FileNameWithoutExtension
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(Name);
            }
        }
    }

}
