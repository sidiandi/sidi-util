﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;

namespace Sidi.IO.Long
{
    public static class LongNameEx
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static LongName Long(this string x)
        {
            return new LongName(x);
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

        public LongName(string path)
        {
            if (path.StartsWith(longNamePrefix))
            {
                this.path = path;
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

        public IList<string> Parts
        {
            get
            {
                return this.path.Split(System.IO.Path.DirectorySeparatorChar).Skip(3).ToList();
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

        void Check()
        {
            if (this.path.Length > 32000)
            {
                throw new System.IO.PathTooLongException(this.path);
            }

            var parts = Parts;
            var tooLong = parts.FirstOrDefault(x => x.Length > 255);
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
                var i = path.LastIndexOf(DirectorySeparator);
                return new LongName(path.Substring(0, i));
            }
        }

        public string Name
        {
            get
            {
                var i = path.LastIndexOf(DirectorySeparator);
                return path.Substring(i + 1);
            }
        }

        public const string DirectorySeparator = @"\";

        public string NoPrefix
        {
            get
            {
                return path.Substring(longNamePrefix.Length);

            }
        }

        public override string ToString()
        {
            return Param;
        }

        public string Param
        {
            get
            {
                return path;
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
