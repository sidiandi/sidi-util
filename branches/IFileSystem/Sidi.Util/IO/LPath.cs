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
using System.Runtime.Serialization;
using Sidi.Parse;

namespace Sidi.IO
{
    /// <summary>
    /// Absolute or relative file system path.
    /// A file system path has following grammar
    ///
    /// Path = Prefix [*(Name PathSeparator) Name]
    /// Prefix = Unc / Drive / RootRelative
    /// RootRelative = PathSeparator
    /// Unc = "\\?\UNC\" Host PathSeparator Share PathSeparator
    /// Drive = [A-Z] ":\"
    /// 
    /// </summary>
    [Serializable]
    public class LPath : IXmlSerializable, IComparable, IEquatable<LPath>, ISerializable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Prefix prefix;
        string[] parts;

        LPath()
        {
        }

        LPath(string prefix, IEnumerable<string> parts)
        {
            Initialize(CheckPrefix(prefix), parts);
        }
        
        public LPath(string path)
        {
            var text = new Sidi.Parse.Text(path);
            prefix = PathParser.Prefix(text);
            parts = PathParser.Names(text);
            Initialize(prefix, parts);
        }

        /// <summary>
        /// Only to be called from Ctors.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="parts"></param>
        void Initialize(Prefix prefix, IEnumerable<string> parts)
        {
            if (prefix == null)
            {
                throw new ArgumentOutOfRangeException("path", "path prefix is invalid");
            }
            if (parts == null)
            {
                throw new ArgumentOutOfRangeException("path");
            }

            this.prefix = prefix;
            this.parts = parts.ToArray();

            if (Length > MaxPathLength)
            {
                throw new ArgumentOutOfRangeException("path", String.Format("path is {0} characters long. Maximal length is {1}", Length, MaxPathLength));
            }
        }

        int Length
        {
            get
            {
                return Prefix.Length + Parts.Sum(p => p.Length) - 1;
            }
        }

        public string Prefix
        {
            get
            {
                return prefix.Text;
            }
        }

        public string[] Parts
        {
            get
            {
                return parts;
            }
        }

        static Prefix CheckPrefix(string p)
        {
            var prefix = PathParser.Prefix(new Sidi.Parse.Text(p));
            if (prefix == null)
            {
                throw new ArgumentOutOfRangeException(String.Format("{0} is not a valid path prefix.", p.Quote()));
            }
            return prefix;
        }

        public static LPath CreateRelative(params string[] parts)
        {
            return CreateRelative((IEnumerable<string>)parts);
        }

        public static LPath CreateRelative(IEnumerable<string> parts)
        {
            return new LPath(RelativePrefix, parts);
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

        private const string RelativePrefix = "";
        const string longPrefix = @"\\?\";
        const string longUncPrefix = @"\\?\UNC\";
        const string shortUncPrefix = @"\\";

        public const string ExtensionSeparator = ".";

        public static implicit operator LPath(string text)
        {
            return new LPath(text);
        }

        public static implicit operator string(LPath path)
        {
            return path.ToString();
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

        /// <summary>
        /// Converts x into a valid filename by replacing illegal characters and shortening it while keeping uniqueness.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string GetValidFilename(string x)
        {
            return PathParser.MakeValidFilename(new Text(x));
        }

        public static bool IsValidFilename(string x)
        {
            return CheckFilename(x) == null;
        }

        static Exception CheckFilename(string x)
        {
            try
            {
                if (!PathParser.IsMatch(x, PathParser.NtfsFilename))
                {
                    return new ArgumentOutOfRangeException("x");
                }
            }
            catch (Exception ex)
            {
                return ex;
            }
            return null;
        }

        static Exception CheckFilenameWithWildcards(string x)
        {
            try
            {
                if (!PathParser.IsMatch(x, PathParser.NtfsFilenameWithWildcards))
                {
                    return new ArgumentOutOfRangeException("x");
                }
            }
            catch (Exception ex)
            {
                return ex;
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
            if (!IsRelative)
            {
                return new LPath(Prefix, Enumerable.Empty<string>());
            }
            else
            {
                throw new InvalidOperationException("path is not absolute");
            }
        }

        public bool IsRelative
        {
            get
            {
                return prefix is RelativePrefix;
            }
        }

        public bool IsRootRelative
        {
            get
            {
                return prefix is RootRelativePrefix;
            }
        }

        public static bool IsSameFileSystem(LPath p1, LPath p2)
        {
            return !p1.IsRelative && !p2.IsRelative && object.Equals(p1.GetPathRoot(), p2.GetPathRoot());
        }

        public string DriveLetter
        {
            get
            {
                if (prefix is LocalDrivePrefix)
                {
                    return ((LocalDrivePrefix)prefix).Drive;
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
            if (prefix is RootRelativePrefix)
            {
                return FS.GetCurrentDirectory().GetPathRoot().CatDir(Parts);
            }
            else if (prefix is RelativePrefix)
            {
                return FS.GetCurrentDirectory().CatDir(this.Parts);
            }
            else
            {
                return this;
            }
        }

        public LPath GetFullPath()
        {
            var fp = GetFullPathImpl();
            return fp.Canonic;
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
            var relativePaths = parts.Select(x => new LPath(x)).ToList();

            foreach (var i in relativePaths)
            {
                if (!i.IsRelative)
                {
                    throw new ArgumentOutOfRangeException(String.Format("cannot CatDir non-relative path: {0}", i));
                }
            }

            return new LPath(Prefix, Parts.Concat(relativePaths.SelectMany(_ => _.Parts)));
        }

        public LPath CatDir(params string[] parts)
        {
            return CatDir((IEnumerable<string>)parts);
        }

        public LPath CatName(string namePostfix)
        {
            return new LPath(Prefix, Parts.TakeAllBut(1).Concat(new[]{FileName + namePostfix}));
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

            var p = Parts.ToArray();
            p[p.Length - 1] = FileNameWithoutExtension + newExtension;
            return new LPath(Prefix, p);
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
                if (Parts.Any())
                {
                    return new LPath(Prefix, Parts.TakeAllBut(1));
                }
                else
                {
                    return null;
                }
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
            var searchPath = this.CatDir(pattern);
            return ToListOfPath(FS.FindFile(searchPath));
        }

        /// <summary>
        /// Get all file children of this path.
        /// Will return not an exception, but return an empty list.
        /// </summary>
        /// <param name="pattern">wildcard pattern. See Windows API function FsRtlIsNameInExpression for details.</param>
        /// <returns>List of children paths. Empty list for file paths or paths which do not exist.</returns>
        public IList<LPath> GetFiles(string pattern)
        {
            return ToListOfPath(Info.GetFiles(pattern));
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
            return ToListOfPath(Info.GetDirectories(pattern));
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

        static IList<LPath> ToListOfPath(IEnumerable<LFileSystemInfo> list)
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

        public override string ToString()
        {
            return Prefix + Parts.Join(DirectorySeparator);
        }

        public string Param
        {
            get
            {
                string p = Prefix;
                if (prefix is LocalDrivePrefix)
                {
                    p = longPrefix + ((LocalDrivePrefix)prefix).Drive + @":\";
                }
                else if (prefix is UncPrefix)
                {
                    var u = (UncPrefix)prefix;
                    p = longUncPrefix + u.Server + DirectorySeparator + u.Share + DirectorySeparator;
                }

                return new LPath(p, Parts).ToString();
            }
        }

        /// <summary>
        /// Returns true if ParentDirectory would return null
        /// </summary>
        public bool IsRoot
        {
            get
            {
                return Parts.Length == 0;
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
                return PathParser.IsMatch(Prefix, PathParser.LongUncPrefix) || PathParser.IsMatch(Prefix, PathParser.UncPrefix);
            }
        }

        public override int GetHashCode()
        {
            return Param.ToLower().GetHashCode();
        }

        static public StringComparison StringComparison { get; private set; }

        static LPath empty = new LPath();


      public override bool Equals(Object right)
       {    
          // check null:
          // this pointer is never null in C# methods.
          if (Object.ReferenceEquals(right, null)) 
             return false;

          if (Object.ReferenceEquals(this, right))
             return true;
     
          if (this.GetType() != right.GetType())
             return false; 
          return this.Equals(right as LPath);
        }

        public bool Equals(LPath other)
        {
            return other != null
                && object.Equals(Prefix, other.Prefix)
                && Parts.SequenceEqual(other.Parts, StringComparer.InvariantCultureIgnoreCase);
        }
        
        public bool Contains(string value)
        {
            return this.ToString().IndexOf(value, StringComparison) >= 0;
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
                return LPath.CreateRelative(difference);
            }
            else
            {
                return LPath.CreateRelative(new[] { "..", this.FileName });
            }
        }

        public bool IsDirectory
        {
            get
            {
                // TODO: remove special treatment of path roots
                if (!Parts.Any())
                {
                    return System.IO.Directory.Exists(this);
                }

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
            }
            else
            {
                reader.ReadStartElement();
                var p = new LPath(reader.ReadString());
                prefix = p.prefix;
                parts = p.parts;
                reader.ReadEndElement();
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteString(ToString());
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
                return new Uri(ToString(), UriKind.Absolute);
            }
        }

        public static LPath GetRandomFileName()
        {
            return new LPath(System.IO.Path.GetRandomFileName());
        }

        public bool IsAncestor(LPath c)
        {
            if (c.Equals(this))
            {
                return true;
            }

            return IsAncestor(c.Parent);
        }

        public static LPath GetUncRoot(string server, string share)
        {
            return new LPath(shortUncPrefix + server + DirectorySeparator + share + DirectorySeparator, Enumerable.Empty<string>());
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("p", ToString());
        }

        protected LPath(SerializationInfo info, StreamingContext context)
        : this(info.GetString("p"))
        {
        }
    }
}
