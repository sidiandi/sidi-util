using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using Sidi.Extensions;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Reflection;

namespace Sidi.IO
{
    [Serializable]
    public class LPath : IXmlSerializable
    {
        string path;
        static LPath empty = new LPath();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const string pathPrefix = @"\\?\";
        const string longUncPrefix = @"\\?\UNC\";
        const string shortUncPrefix = @"\\";
        const string extensionSeparator = ".";

        static Regex invalidFilenameRegex = new Regex(
            System.IO.Path.GetInvalidFileNameChars()
            .Select(n => Regex.Escape(new String(n, 1)))
            .Join("|"));

        static Regex invalidFilenameRegexWithoutWildcards = new Regex(
            System.IO.Path.GetInvalidFileNameChars()
            .Where(x => x != '*' && x != '?')
            .Select(n => Regex.Escape(new String(n, 1)))
            .Join("|"));

        public static implicit operator LPath(string text)
        {
            return new LPath(text);
        }

        public static implicit operator string(LPath path)
        {
            return path.path;
        }

        public static LPath GetTempFileName()
        {
            return new LPath(System.IO.Path.GetTempFileName());
        }

        public static LPath GetTempPath()
        {
            return new LPath(System.IO.Path.GetTempPath());
        }

        public string Quote()
        {
            return this.ToString().Quote();
        }

        public void RemoveEmptyDirectories()
        {
            LPath path = this;

            if (LDirectory.Exists(path))
            {
                var thumbs = path.CatDir("Thumbs.db");
                if (thumbs.Exists)
                {
                    LFile.Delete(thumbs);
                }
                foreach (var d in LDirectory.GetChilds(path).Where(x => x.IsDirectory))
                {
                    d.FullName.RemoveEmptyDirectories();
                }

                try
                {
                    LDirectory.Delete(path);
                    log.InfoFormat("Delete {0}", path);
                }
                catch (System.IO.IOException)
                {
                }
            }
        }

        public static string GetValidFilename(string x)
        {
            return Truncate(invalidFilenameRegex.Replace(x, "_"), LPath.MaxFilenameLength);
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
            return x.Length <= LPath.MaxFilenameLength && !invalidFilenameRegex.IsMatch(x);
        }

        public static bool IsValidFilenameWithWildcards(string x)
        {
            return x.Length <= LPath.MaxFilenameLength && !invalidFilenameRegexWithoutWildcards.IsMatch(x);
        }

        public LPath()
        {
            path = String.Empty;
        }

        public LPath(string path)
        {
            Check(path);

            // remove trailing slash
            if (path.EndsWith(@"\"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            if (String.IsNullOrEmpty(path))
            {
                this.path = String.Empty;
            }
            else if (path.StartsWith(pathPrefix))
            {
                this.path = path.Substring(pathPrefix.Length);
            }
            else if (path.StartsWith(longUncPrefix))
            {
                this.path = shortUncPrefix + path.Substring(longUncPrefix.Length);
            }
            else
            {
                this.path = path;
            }
        }

        public LPath(IEnumerable<string> parts)
        : this(parts.Join(DirectorySeparator))
        {
        }

        public static LPath Join(params object[] parts)
        {
            return new LPath(
                parts
                .SafeSelect(p =>
                {
                    if (p is LPath)
                    {
                        return ((LPath)p).NoPrefix;
                    }
                    else if (p is string)
                    {
                        return (string)p;
                    }
                    else
                    {
                        return LPath.GetValidFilename(p.ToString());
                    }
                }));
        }

        public LPath Canonic
        {
            get
            {
                return new LPath(Parts.Where(x => !x.Equals(".")));
            }
        }

        public LPath UniqueFileName()
        {
            if (!new FileSystemInfo(this).Exists)
            {
                return this;
            }

            for (int i = 1; i < 1000; ++i)
            {
                var u = new LPath(String.Format("{0}.{1}", this, i));
                if (!new FileSystemInfo(u).Exists)
                {
                    return u;
                }
            }
            throw new System.IO.IOException(String.Format("{0} cannot be made unique.", this));
        }

        public static LPath Parse(string x)
        {
            return new LPath(x);
        }

        public bool Exists
        {
            get
            {
                return Info.Exists;
            }
        }

        public LPath PathRoot
        {
            get
            {
                return new LPath(Parts.Take(1));
            }
        }

        public string DriveLetter
        {
            get
            {
                var r = PathRoot.ToString();
                if (r.Length == 2 && r[1] == ':')
                {
                    return r[0].ToString();
                }
                else
                {
                    throw new NotSupportedException();
                }
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

            if (IsUnc && Parts.Length == 4)
            {
                if (System.IO.Directory.Exists(NoPrefix))
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
                else
                {
                    fd = default(FindData);
                    return false;
                }
            }

            using (var f = LDirectory.FindFileRaw(this).GetEnumerator())
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
                return this.path.Split(DirectorySeparatorChar).ToArray();
            }
        }

        public FileSystemInfo Info
        {
            get
            {
                return new FileSystemInfo(this.GetFullPath());
            }
        }

        public LPath GetFullPath()
        {
            var p = Parts;
            LPath full = null;
            if (IsUnc)
            {
                full = this;
            }
            else if (String.IsNullOrEmpty(p[0]))
            {
                // concat with drive
                full = new LPath(LDirectory.Current.Parts[0]).CatDir(this.Parts.Skip(1));
            }
            else if (IsValidDriveRoot(p[0]))
            {
                full = this;
            }
            else
            {
                // concat with current directory
                full = LDirectory.Current.CatDir(this);
            }

            full = full.Canonic;

            return full;
        }

        public bool HasExtension
        {
            get
            {
                return !String.IsNullOrEmpty(Extension);
            }
        }
        
        public LPath CatDir(IEnumerable<string> parts)
        {
            return new LPath((new string[] { this.path }.Concat(parts)).Join(DirectorySeparator));
        }

        public LPath CatDir(params object[] parts)
        {
            return new LPath(
                new string[] { this.NoPrefix }.Concat(
                parts
                .SafeSelect(p =>
                {
                    if (p is LPath)
                    {
                        return ((LPath)p).NoPrefix;
                    }
                    else if (p is string)
                    {
                        return (string)p;
                    }
                    else
                    {
                        return LPath.GetValidFilename(p.ToString());
                    }
                })));
        }

        public LPath CatName(string namePostfix)
        {
            return new LPath(this.path + namePostfix);
        }

        public const int MaxFilenameLength = 255;

        const int MaxPathLength = 32000;

        public static bool IsValid(string path)
        {
            try
            {
                new LPath(path);
                return true;
            }
            catch (System.IO.PathTooLongException)
            {
                return false;
            }
        }

        static void Check(string path)
        {
            if (path.Length > MaxPathLength)
            {
                throw new System.IO.PathTooLongException();
            }

            var parts = path.Split(DirectorySeparatorChar);

            for (int i = 0; i < Math.Min(1, parts.Length); ++i)
            {
                var x = parts[i];
                if (!IsValidFilename(x))
                {
                    if (i == 0 && IsValidDriveRoot(x))
                    {
                        continue;
                    }
                    if (i == parts.Length - 1 && IsValidFilenameWithWildcards(x))
                    {
                        continue;
                    }
                    throw new System.IO.PathTooLongException(x);
                }
            }
        }

        /// <summary>
        /// Replaces the file extension of a path. 
        /// </summary>
        /// <param name="newExtension">New extension (with or without dot)</param>
        /// <returns></returns>
        public LPath ChangeExtension(string newExtension)
        {
            if (newExtension == null)
            {
                return Parent.CatDir(FileNameWithoutExtension);
            }
            else
            {
                if (!newExtension.StartsWith(extensionSeparator, StringComparison.OrdinalIgnoreCase))
                {
                    newExtension = extensionSeparator + newExtension;
                }
                return Parent.CatDir(FileNameWithoutExtension + newExtension);
            }
        }

        public LPath Sibling(string siblingName)
        {
            return Parent.CatDir(siblingName);
        }

        /// <summary>
        /// Returns this path written relative to basePath
        /// </summary>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public LPath GetRelative(LPath basePath)
        {
            var path = this.GetFullPath();
            basePath = basePath.GetFullPath();
            List<string> result = new List<string>();

            var p = path.Parts;
            var b = basePath.Parts;

            int different = 0;
            for (different = 0; different < p.Length && different < b.Length; ++different)
            {
                if (!p[different].Equals(b[different]))
                {
                    break;
                }
            }

            for (int i = different; i < b.Length; ++i)
            {
                result.Add("..");
            }

            for (int i = different; i < p.Length; ++i)
            {
                result.Add(p[i]);
            }

            return new LPath(result);
        }
        
        public LPath Parent
        {
            get
            {
                var p = GetFullPath().Parts;

                if (IsUnc)
                {
                    if (p.Length <= 4)
                    {
                        return null;
                    }
                }

                if (p.Length <= 1)
                {
                    return null;
                }
                return new LPath(p.Take(p.Length - 1));
            }
        }

        public IList<LPath> Children
        {
            get
            {
                return this.Info.GetFileSystemInfos()
                    .Select(x => x.FullName)
                    .ToList();
            }
        }

        public string FileName
        {
            get
            {
                return GetFullPath().Parts.Last();
            }
        }

        public const string DirectorySeparator = @"\";

        public string NoPrefix
        {
            get
            {
                return path;
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
                if (String.IsNullOrEmpty(path))
                {
                    return String.Empty;
                }
                else if (path.StartsWith(shortUncPrefix))
                {
                    return longUncPrefix + path.Substring(shortUncPrefix.Length);
                }
                else
                {
                    return pathPrefix + path;
                }
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
                    return Parts.Length <= 1;
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
                var p = Parts;
                return p.Length == 1 && IsValidDriveRoot(p[0]);
            }
        }

        public static bool IsValidDriveRoot(string driveRoot)
        {
            return Regex.IsMatch(driveRoot, @"^[A-Z]\:$", RegexOptions.IgnoreCase);
        }

        public bool IsUnc
        {
            get
            {
                return path.StartsWith(shortUncPrefix);
            }
        }

        public override int GetHashCode()
        {
            return Param.GetHashCode();
        }

        const StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase;

        public static LPath Empty
        {
            get
            {
                return empty;
            }
        }
        
        public override bool Equals(object obj)
        {
            if (obj is LPath)
            {
                return Param.Equals(((LPath)obj).Param, stringComparison);
            }
            else
            {
                return false;
            }
        }

        public LPath RelativeTo(LPath root)
        {
            if (!path.StartsWith(root.path, stringComparison))
            {
                throw new ArgumentOutOfRangeException("root");
            }

            var rp = root.Parts;

            return new LPath(Parts.Skip(rp.Length));
        }

        public bool IsDirectory
        {
            get
            {
                var info = this.Info;
                return info.Exists && info.IsDirectory;
            }
        }

        public bool IsFile
        {
            get
            {
                var info = this.Info;
                return info.Exists && !info.IsDirectory;
            }
        }

        public void EnsureNotExists()
        {
            var ln = this;
            FindData fd;
            if (ln.GetFindData(out fd))
            {
                if (fd.IsDirectory)
                {
                    foreach (var c in LDirectory.FindFile(ln.CatDir("*")).ToList())
                    {
                        var cn = ln.CatDir(c.Name);
                        if (c.IsDirectory)
                        {
                            cn.EnsureNotExists();
                        }
                        else
                        {
                            LFile.Delete(cn);
                        }
                    }
                    LDirectory.Delete(ln);
                }
                else
                {
                    LFile.Delete(ln);
                }
                log.InfoFormat("Delete {0}", ln);
            }
        }

        public void EnsureParentDirectoryExists()
        {
            Parent.EnsureDirectoryExists();
        }

        public void EnsureDirectoryExists()
        {
            if (!LDirectory.Exists(this))
            {
                LDirectory.Create(this);
            }
        }

        public string Extension
        {
            get
            {
                return System.IO.Path.GetExtension(FileName);
            }
        }

        public string FileNameWithoutExtension
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(FileName);
            }
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            path = reader.ReadString();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteString(path);
        }
    }
}
