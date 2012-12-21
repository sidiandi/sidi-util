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
    public class LPath : IXmlSerializable, IComparable
    {
        static LPath()
        {
            StringComparison = StringComparison.OrdinalIgnoreCase;
        }
        
        string path;
        static LPath empty = new LPath();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const string pathPrefix = @"\\?\";
        const string longUncPrefix = @"\\?\UNC\";
        const string shortUncPrefix = @"\\";
        const string extensionSeparator = ".";

        static Regex invalidFilenameRegexWithoutWildcards = new Regex(
            System.IO.Path.GetInvalidFileNameChars()
            .Where(x => x != '*' && x != '?')
            .Select(n => Regex.Escape(new String(n, 1)))
            .Join("|"));

        static Regex invalidFilenameRegex = new Regex(
            System.IO.Path.GetInvalidFileNameChars()
            .Select(n => Regex.Escape(new String(n, 1)))
            .Join("|"));

        static Regex invalidFilenameEndRegex = new Regex("[ .]+$");

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

            if (path.Exists)
            {
                var thumbs = path.CatDir("Thumbs.db");
                if (thumbs.Exists)
                {
                    LFile.Delete(thumbs);
                }
                foreach (var d in path.GetDirectories())
                {
                    d.RemoveEmptyDirectories();
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
            x = invalidFilenameRegex.Replace(x, "_");
            x = invalidFilenameEndRegex.Replace(x, m => Regex.Replace(m.Value, ".", "_"));
            return Truncate(x, LPath.MaxFilenameLength);
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

        /// <summary>
        /// Appends a number (.02) to the file name so that the returned path points to a file 
        /// in the same directory that does not exist yet
        /// Example: C:\temp\myimage.jpg => C:\temp\myimage.1.jpg
        /// </summary>
        /// <returns></returns>
        public LPath UniqueFileName()
        {
            if (!Exists)
            {
                return this;
            }

            for (int i = 1; i < 1000; ++i)
            {
                var u = Parent.CatDir(JoinFileName(new string[] { FileNameWithoutExtension, i.ToString(), ExtensionWithoutDot }));
                if (!u.Exists)
                {
                    return u;
                }
            }
            throw new System.IO.IOException(String.Format("{0} cannot be made unique.", this));
        }

        /// <summary>
        /// Parses an LPath instance from a string. Allows / and \ as directory separators.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static LPath Parse(string text)
        {
            text = text.Replace("/", LPath.DirectorySeparator);
            return new LPath(text);
        }

        public bool Exists
        {
            get
            {
                return Info.Exists;
            }
        }

        /// <summary>
        /// Throws an excpetion when !IsAbsolute
        /// </summary>
        /// <returns>Root of the file system, e.g. C: or \\server\share</returns>
        public LPath GetPathRoot()
        {
            if (IsUnc)
            {
                return new LPath(Parts.Take(4));
            }

            if (IsAbsolute)
            {
                return new LPath(Parts.Take(1));
            }
            else
            {
                throw new InvalidOperationException("path is not absolute");
            }
        }

        public bool IsAbsolute
        {
            get
            {
                if (IsUnc)
                {
                    return true;
                }
                else
                {
                    return this.path.Length >= 2 && this.path[1].Equals(':');
                }
            }
        }

        public static bool IsSameFileSystem(LPath p1, LPath p2)
        {
            return p1.IsAbsolute && p2.IsAbsolute && p1.GetPathRoot().Equals(p2.GetPathRoot());
        }

        public string DriveLetter
        {
            get
            {
                var r = GetPathRoot().ToString();
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

        public LFileSystemInfo Info
        {
            get
            {
                return new LFileSystemInfo(this.GetFullPath());
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
                if (!newExtension.StartsWith(extensionSeparator, StringComparison))
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
                return GetChildren(LPath.AllFilesWildcard);
            }
        }

        public IList<LPath> GetChildren(string pattern)
        {
            return ToPathList(Info.GetChildren(pattern));
        }

        public IList<LPath> GetFiles(string pattern)
        {
            return ToPathList(Info.GetFiles(pattern));
        }

        public IList<LPath> GetFiles()
        {
            return GetFiles(AllFilesWildcard);
        }

        public IList<LPath> GetDirectories(string pattern)
        {
            return ToPathList(Info.GetDirectories(pattern));
        }

        public IList<LPath> GetDirectories()
        {
            return GetDirectories(AllFilesWildcard);
        }

        IList<LPath> ToPathList(IList<LFileSystemInfo> list)
        {
            return list.Select(x => x.FullName).ToList();
        }

        public static readonly string AllFilesWildcard = "*";

        /// <summary>
        /// Gets all files that match the wildcard searchPath
        /// </summary>
        /// <param name="searchPath">search path that can contain wild cards</param>
        /// <returns></returns>
        public static IList<LPath> Get(LPath searchPath)
        {
            return LDirectory.FindFile(searchPath)
                .Select(x => x.FullName)
                .ToList();
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
            return Param.ToLower().GetHashCode();
        }

        static public StringComparison StringComparison { get; set; }

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
                return Param.Equals(((LPath)obj).Param, StringComparison);
            }
            else
            {
                return false;
            }
        }

        public bool Contains(string value)
        {
            return this.path.IndexOf(value, StringComparison) >= 0;
        }

        public bool EndsWith(string value)
        {
            return path.EndsWith(value, StringComparison);
        }

        public bool StartsWith(string value)
        {
            return path.StartsWith(value, StringComparison);
        }

        public LPath RelativeTo(LPath root)
        {
            if (!path.StartsWith(root.path, StringComparison))
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
                    foreach (var c in LDirectory.FindFile(ln.CatDir(LPath.AllFilesWildcard)).ToList())
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

        public string[] FileNameParts
        {
            get
            {
                return FileName.Split(new string[] { extensionSeparator }, StringSplitOptions.None);
            }
        }
        
        /// <summary>
        /// Joins parts with the extension separator (.)
        /// null parts will be ignored.
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        public static LPath JoinFileName(params string[] parts)
        {
            return new LPath(parts.Where(x => x != null).Join(extensionSeparator));
        }

        /// <summary>
        /// returns the extension of the file name without the . 
        /// returns null if no extension exists
        /// example: c:\image.jpg => jpg
        /// </summary>
        public string ExtensionWithoutDot
        {
            get
            {
                var p = FileNameParts;
                if (p.Length <= 1)
                {
                    return null;
                }
                else
                {
                    return p[p.Length - 1];
                }
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
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            path = reader.ReadString();
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteString(path);
        }

        public int CompareTo(object obj)
        {
            var r = obj as LPath;
            if (r == null)
            {
                throw new System.ArgumentException("obj");
            }
            return this.ToString().CompareTo(r.ToString());
        }
    }
}
