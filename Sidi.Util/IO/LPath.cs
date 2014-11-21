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
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using Sidi.Extensions;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Reflection;
using System.IO;

namespace Sidi.IO
{
    [Serializable]
    public class LPath : IXmlSerializable, IComparable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public LPath(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentOutOfRangeException("path");
            }

            // remove trailing slash
            if (path.EndsWith(@"\"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            if (prefixes.Any(_ => path.StartsWith(_)))
            {
                m_internalPathRepresentation = path;
            }
            else
            {
                // handle normal path

                // UNC
                if (RemovePrefix(path, shortUncPrefix, out m_internalPathRepresentation))
                {
                    m_internalPathRepresentation = longUncPrefix + m_internalPathRepresentation;
                }
                else if (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':')
                {
                    m_internalPathRepresentation = longPrefix + path;
                }
                else
                {
                    m_internalPathRepresentation = path;
                }
            }

            if (m_internalPathRepresentation.Length > MaxPathLength)
            {
                throw new ArgumentOutOfRangeException(String.Format("string is {0} characters long. Maximal path length: {1}", m_internalPathRepresentation.Length, MaxPathLength));
            }

            var p = Parts;

            var invalidFileName = p.Take(1).FirstOrDefault(_ => !IsValidFilenameWithWildcards(_) && !IsDriveSpecifier(_));
            if (invalidFileName != null)
            {
                throw new ArgumentOutOfRangeException(String.Format("{0} is not a valid file name or a drive specifier", invalidFileName.Quote()));
            }

            invalidFileName = p.Skip(1).FirstOrDefault(_ => !IsValidFilenameWithWildcards(_));
            if (invalidFileName != null)
            {
                throw new ArgumentOutOfRangeException(String.Format("{0} is not a valid file name", invalidFileName.Quote()));
            }
        }

        LPath(string prefix, IEnumerable<string> parts)
            : this(
                ((prefix == null) ? String.Empty : prefix)
                + String.Join(DirectorySeparator, parts))
        {

        }

        LPath()
        {
            m_internalPathRepresentation = null;
        }

        public static LPath Join(string prefix, IEnumerable<string> parts)
        {
            return new LPath(prefix, parts);
        }

        string Prefix
        {
            get
            {
                foreach (var p in prefixes)
                {
                    if (m_internalPathRepresentation.StartsWith(p))
                    {
                        return p;
                    }
                }
                return String.Empty;
            }
        }

        public string[] Parts
        {
            get
            {
                return m_internalPathRepresentation.Substring(Prefix.Length).Split(new[] { DirectorySeparator }, StringSplitOptions.None);
            }
        }

        FileSystem FS
        {
            get
            {
                return FileSystem.Current;
            }
        }
        
        static LPath()
        {
            StringComparison = StringComparison.OrdinalIgnoreCase;
        }

        string m_internalPathRepresentation;

        const string longPrefix = @"\\?\";
        const string longUncPrefix = @"\\?\UNC\";
        public const string shortUncPrefix = @"\\";
        const string deviceNamespacePrefix = @"\\.\";
        const string shortRootRelativePrefix = @"\";

        public const string ExtensionSeparator = ".";

        static readonly string[] prefixes = new[]
        {
            longUncPrefix,
            longPrefix,
            deviceNamespacePrefix,
            shortUncPrefix,
            shortRootRelativePrefix
        };

        static readonly string[] fullPathPrefixes = new[]
        {
            longUncPrefix,
            longPrefix,
            deviceNamespacePrefix,
            shortUncPrefix,
        };

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
            return path.NoPrefix;
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
            return CheckFilename(x) == null;
        }

        static Exception CheckFilename(string x)
        {
            if (x.Length > LPath.MaxFilenameLength)
            {
                return new System.IO.PathTooLongException("file name is {0} characters too long. Actual characters: {1}, allowed characters: {2}, file name: {3}".F(
                    x.Length - MaxFilenameLength,
                    x.Length,
                    MaxFilenameLength,
                    x));
            }

            var m = invalidFilenameRegex.Match(x);
            if (m.Success)
            {
                return new System.IO.IOException("file name contains invalid character at {0}. File name: {1}".F(
                    m.Index,
                    x));
            }
            return null;
        }

        static Exception CheckFilenameWithWildcards(string x)
        {
            if (String.IsNullOrEmpty(x))
            {
                return new ArgumentException("file name cannot be empty.");
            }

            if (x.Length > LPath.MaxFilenameLength)
            {
                return new ArgumentException("file name is {0} characters too long. Actual characters: {1}, allowed characters: {2}, file name: {3}".F(
                    x.Length - MaxFilenameLength,
                    x.Length,
                    MaxFilenameLength,
                    x));
            }

            var m = invalidFilenameRegexWithoutWildcards.Match(x);
            if (m.Success)
            {
                return new ArgumentException("file name contains invalid character at {0}. File name: {1}".F(
                    m.Index,
                    x));
            }
            return null;
        }

        public static bool IsValidFilenameWithWildcards(string x)
        {
            return CheckFilenameWithWildcards(x) == null;
        }

        static bool RemovePrefix(string text, string prefix, out string result)
        {
            if (text.StartsWith(prefix))
            {
                result = text.Substring(prefix.Length);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        static bool IsDriveSpecifier(string d)
        {
            return d.Length == 2 && char.IsLetter(d[0]) && d[1] == ':';
        }

        public static LPath Join(params string[] parts)
        {
            return new LPath(parts.Join(DirectorySeparator));
        }

        public LPath Canonic
        {
            get
            {
                return new LPath(Prefix, Parts.Where(x => !x.Equals(".")));
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
        /// Accepts file:// URLs
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static LPath Parse(string text)
        {
            if (text.StartsWith("file://"))
            {
                return new LPath(new Uri(text).LocalPath);
            }
            else if (text.StartsWith(":paste", StringComparison.OrdinalIgnoreCase) || text.Equals(":sel", StringComparison.OrdinalIgnoreCase))
            {
                return PathList.Parse(text).First();
            }

            if (text.StartsWith(":current", StringComparison.OrdinalIgnoreCase))
            {
                return new Shell().GetOpenDirectory();
            }

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
        /// Throws an exception when !IsFullPath
        /// </summary>
        /// <returns>Root of the file system, e.g. C: or \\server\share</returns>
        public LPath GetPathRoot()
        {
            if (IsFullPath)
            {
                if (IsUnc)
                {
                    return new LPath(Prefix, Parts.Take(2));
                }
                else
                {
                    return new LPath(Prefix, Parts.Take(1));
                }
            }
            else
            {
                throw new InvalidOperationException("path is not absolute");
            }
        }

        /// <summary>
        /// As IsFullPath, but returns true for paths without drive specifier, such as \a\b\c
        /// Because you normally would like to check for a fully qualified path, i.e. which is not 
        /// changed by current drive or directory anymore, you should use IsFullPath instead
        /// </summary>
        [Obsolete("use IsFullPath instead")]
        public bool IsAbsolute
        {
            get
            {
                return IsFullPath;
            }
        }

        /// <summary>
        /// Returns true if GetFullPath will return the path itself
        /// </summary>
        public bool IsFullPath
        {
            get
            {
                return fullPathPrefixes.Any(_ => m_internalPathRepresentation.StartsWith(_));
            }
        }

        public static bool IsSameFileSystem(LPath p1, LPath p2)
        {
            return p1.IsFullPath && p2.IsFullPath && object.Equals(p1.GetPathRoot(), p2.GetPathRoot());
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

        public static readonly char DirectorySeparatorChar = System.IO.Path.DirectorySeparatorChar;

        public LFileSystemInfo Info
        {
            get
            {
                return FS.GetInfo(this);
            }
        }

        private LPath GetFullPathImpl()
        {
            if (IsAbsolute)
            {
                return this;
            }

            if (Prefix.Equals(shortRootRelativePrefix))
            {
                return LDirectory.Current.GetPathRoot().CatDir(Parts);
            }

            return LDirectory.Current.CatDir(this);
        }

        public LPath GetFullPath()
        {
            return GetFullPathImpl().Canonic;
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
            return new LPath(Prefix, Parts.Concat(parts));
        }

        public LPath CatDir(params string[] parts)
        {
            return new LPath(Prefix,
                this.Parts.Concat(
                parts
                .SafeSelect(p =>
                {
                    if (p is LPath)
                    {
                        var path = (LPath)p;
                        if (path.IsAbsolute)
                        {
                            throw new ArgumentOutOfRangeException("absolute paths cannot be appended");
                        }
                        return path.m_internalPathRepresentation;
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
            return new LPath(this.Param + namePostfix);
        }

        public const int MaxFilenameLength = 255;

        internal const int MaxPathLength = 32000;

        public static bool IsValid(string path)
        {
            return CheckPath(path) == null;
        }

        static Exception CheckPath(string path)
        {
            if (path.Length > MaxPathLength)
            {
                return new System.IO.PathTooLongException("Path is {0} characters too long. Actual length: {1}, allowed: {2}".F(
                    path.Length - MaxPathLength,
                    path.Length,
                    MaxPathLength));
            }

            var parts = path.Split(DirectorySeparatorChar);

            for (int i = 0; i < Math.Min(1, parts.Length); ++i)
            {
                var exception = (i == parts.Length - 1) ? 
                    CheckFilenameWithWildcards(parts[i]) : 
                    CheckFilename(parts[i]);

                if (exception != null)
                {
                    if (i == 0 && IsValidDriveRoot(parts[i]))
                    {
                        continue;
                    }
                    else
                    {
                        return exception;
                    }
                }
            }
            return null;
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
                newExtension = String.Empty;
            }
            else
            {
                if (!newExtension.StartsWith(ExtensionSeparator, StringComparison))
                {
                    newExtension = ExtensionSeparator + newExtension;
                }
            }

            var p = Parts;
            p[p.Length - 1] = FileNameWithoutExtension + newExtension;
            return Join(p);
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

            return new LPath(String.Empty, result);
        }
        
        public LPath Parent
        {
            get
            {
                var p = Parts;

                if (IsUnc)
                {
                    if (p.Length <= 2)
                    {
                        return null;
                    }
                }
                else
                {
                    if (p.Length <= 1)
                    {
                        return null;
                    }
                }

                return new LPath(Prefix, p.TakeAllBut(1));
            }
        }

        public IEnumerable<LPath> Lineage
        {
            get
            {
                for (var i = this; i != null; i = i.Parent)
                {
                    yield return i;
                }
            }
        }

        /// <summary>
        /// Same as GetChildren()
        /// </summary>
        public IList<LPath> Children
        {
            get
            {
                return GetChildren();
            }
        }

        /// <summary>
        /// Get all children of this path, directories as well as files.
        /// Will return not an exception, but return an empty list.
        /// </summary>
        /// <returns>List of children paths. Empty list for file paths or paths which do not exist.</returns>
        public IList<LPath> GetChildren()
        {
            return GetChildren(LPath.AllFilesWildcard);
        }

        /// <summary>
        /// Get all children of this path, directories as well as files.
        /// Will return not an exception, but return an empty list.
        /// </summary>
        /// <param name="pattern">wildcard pattern. See Windows API function FsRtlIsNameInExpression for details.</param>
        /// <returns>List of children paths. Empty list for file paths or paths which do not exist.</returns>
        public IList<LPath> GetChildren(string pattern)
        {
            return ToPathList(Info.GetChildren(pattern));
        }

        /// <summary>
        /// Get all file children of this path.
        /// Will return not an exception, but return an empty list.
        /// </summary>
        /// <param name="pattern">wildcard pattern. See Windows API function FsRtlIsNameInExpression for details.</param>
        /// <returns>List of children paths. Empty list for file paths or paths which do not exist.</returns>
        public IList<LPath> GetFiles(string pattern)
        {
            return ToPathList(Info.GetFiles(pattern));
        }

        /// <summary>
        /// Get all file children of this path.
        /// Will return not an exception, but return an empty list.
        /// </summary>
        /// <returns>List of children paths. Empty list for file paths or paths which do not exist.</returns>
        public IList<LPath> GetFiles()
        {
            return GetFiles(AllFilesWildcard);
        }

        /// <summary>
        /// Get all children of this path which are directories.
        /// Will return not an exception, but return an empty list.
        /// </summary>
        /// <param name="pattern">wildcard pattern. See Windows API function FsRtlIsNameInExpression for details.</param>
        /// <returns>List of children paths. Empty list for file paths or paths which do not exist.</returns>
        public IList<LPath> GetDirectories(string pattern)
        {
            return ToPathList(Info.GetDirectories(pattern));
        }

        /// <summary>
        /// Get all children of this path which are directories.
        /// Will return not an exception, but return an empty list.
        /// </summary>
        /// <returns>List of children paths. Empty list for file paths or paths which do not exist.</returns>
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
                string result;
                if (RemovePrefix(m_internalPathRepresentation, longUncPrefix, out result))
                {
                    return shortUncPrefix + result;
                }
                else if (RemovePrefix(m_internalPathRepresentation, longPrefix, out result))
                {
                    return result;
                }
                else
                {
                    result = m_internalPathRepresentation;
                    return result;
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
                if (m_internalPathRepresentation == null)
                {
                    return String.Empty;
                }
                else 
                {
                    return m_internalPathRepresentation;
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
                return NoPrefix.StartsWith(shortUncPrefix);
            }
        }

        public override int GetHashCode()
        {
            return Param.ToLower().GetHashCode();
        }

        static public StringComparison StringComparison { get; private set; }

        static LPath empty = new LPath();

        public override bool Equals(object obj)
        {
            var r = obj as LPath;
            if (r != null)
            {
                if (m_internalPathRepresentation == null)
                {
                    return r.m_internalPathRepresentation == null;
                }
                else
                {
                    return m_internalPathRepresentation.Equals(r.m_internalPathRepresentation, StringComparison);
                }
            }
            else
            {
                return false;
            }
        }

        public bool Contains(string value)
        {
            return this.ToString().IndexOf(value, StringComparison) >= 0;
        }

        public bool EndsWith(string value)
        {
            return NoPrefix.EndsWith(value, StringComparison);
        }

        public bool StartsWith(string value)
        {
            return NoPrefix.StartsWith(value, StringComparison);
        }

        static StringComparer partComparer = StringComparer.InvariantCultureIgnoreCase;

        public LPath RelativeTo(LPath root)
        {
            var p = Parts;
            var rp = root.Parts;

            if (!rp.SequenceEqual(p.Take(rp.Length), partComparer))
            {
                throw new ArgumentOutOfRangeException("root");
            }

            var difference = Parts.Skip(rp.Length);

            if (difference.Any())
            {
                return new LPath(String.Empty, difference);
            }
            else
            {
                return Join(new[] { "..", this.FileName });
            }
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

        public void EnsureFileNotExists()
        {
            if (IsFile)
            {
                try
                {
                    FS.DeleteFile(this);
                }
                catch (IOException)
                {
                    var info = Info;
                    if (Info.IsReadOnly)
                    {
                        Info.IsReadOnly = false;
                        EnsureFileNotExists();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            if (Exists)
            {
                throw new Exception("Cannot delete {0}".F(this));
            }
        }

        /// <summary>
        /// Ensures that this path does not exist. Warning: will delete the whole directory tree if
        /// path points to a directory. Will not follow Junction Points, i.e. will only delete the 
        /// junction point, but not the directory it points to.
        /// </summary>
        public void EnsureNotExists()
        {
            if (IsDirectory)
            {
                try
                {
                    FS.RemoveDirectory(this);
                }
                catch
                {
                    foreach (var c in this.Children)
                    {
                        c.EnsureNotExists();
                    }
                    FS.RemoveDirectory(this);
                }
            }
            else if (IsFile)
            {
                EnsureFileNotExists();
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
                return FileName.Split(new string[] { ExtensionSeparator }, StringSplitOptions.None);
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
            return new LPath(parts.Where(x => x != null).Join(ExtensionSeparator));
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
            if (reader.IsEmptyElement)
            {
                reader.ReadStartElement();
                m_internalPathRepresentation = null;
            }
            else
            {
                reader.ReadStartElement();
                m_internalPathRepresentation = reader.ReadString();
                reader.ReadEndElement();
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            if (m_internalPathRepresentation != null)
            {
                writer.WriteString(m_internalPathRepresentation);
            }
        }

        public int CompareTo(object obj)
        {
            var r = obj as LPath;
            if (r == null)
            {
                throw new System.ArgumentException("Parameter must be of Type LPath", "obj");
            }
            return this.ToString().CompareTo(r.ToString());
        }

        public LPath VolumePath
        {
            get
            {
                return FS.GetVolumePath(this);
            }
        }

        public System.IO.StreamWriter WriteText()
        {
            return new System.IO.StreamWriter(OpenWrite());
        }

        public System.IO.StreamReader ReadText()
        {
            return new System.IO.StreamReader(OpenRead());
        }

        /// <summary>
        /// Ensures that parent directory exists and opens file for writing
        /// </summary>
        /// <returns></returns>
        public System.IO.Stream OpenWrite()
        {
            return LFile.OpenWrite(this); 
        }

        public System.IO.Stream OpenRead()
        {
            return LFile.OpenRead(this);
        }

        /// <summary>
        /// The file:// URI represenation of the path
        /// </summary>
        public Uri Uri
        {
            get
            {
                try
                {
                    if (IsRoot)
                    {
                        return new Uri(ToString() + LPath.DirectorySeparator, UriKind.Absolute);
                    }
                    else
                    {
                        return new Uri(ToString(), UriKind.Absolute);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(ToString(), ex);
                }
            }
        }

        public static LPath GetRandomFileName()
        {
            return new LPath(System.IO.Path.GetRandomFileName());
        }
    }
}
