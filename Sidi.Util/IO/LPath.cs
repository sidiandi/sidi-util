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
using System.Collections;
using System.Threading;

namespace Sidi.IO
{
    /// <summary>
    /// Value class for an absolute or relative file system path
    /// </summary>
    /// 
    /// LPath supports path lengths as long as allowed by the underlying IFileSystem implementation.
    ///
    /// A file system path has following grammar
    ///
    /// Path = Prefix [*(Name PathSeparator) Name]
    /// Prefix = Unc / Drive / RootRelative
    /// RootRelative = PathSeparator
    /// Unc = "\\?\UNC\" Host PathSeparator Share PathSeparator
    /// Drive = [A-Z] ":\"
    /// 
    [Serializable]
    public class LPath : IXmlSerializable, IComparable, IEquatable<LPath>, ISerializable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IFileSystem fileSystem;
        Prefix prefix;
        string[] parts;

        LPath()
        {
        }

        LPath(IFileSystem fileSystem, string prefix, IEnumerable<string> parts)
        {
            this.fileSystem = fileSystem;
            Initialize(CheckPrefix(prefix), parts);
        }

        public LPath(string path)
            : this(Sidi.IO.FileSystem.Current, path)
        {
        }
        public LPath(IFileSystem fileSystem, string path)
        {
            try
            {
                this.fileSystem = fileSystem;
                var text = new Sidi.Parse.Text(path);
                var t = text.Copy();
                var ast = PathParser.Path()(t);
                if (t.Length > 0)
                {
                    throw new ParserException(text);
                }
                prefix = GetPrefix(ast[0]);
                parts = ast[1].Childs.Select(x => x.Text.ToString()).ToArray();
            }
            catch (ParserException e)
            {
                throw new ArgumentOutOfRangeException(String.Format("not a valid file system path: {0}", path), e);
            }
            Initialize(prefix, parts);
        }

        static Prefix GetPrefix(Ast ast)
        {
            Ast p = null;
            try
            {
                p = ast.Childs[0];
                switch ((string)p.Name)
                {
                    case "LongUncPrefix":
                        return new UncPrefix { Text = p.Text.ToString(), Server = p["ServerName"].ToString(), Share = p["ShareName"].ToString() };
                    case "DeviceNamespacePrefix":
                        return new DeviceNamespacePrefix { Text = p.Text.ToString() };
                    case "LongPrefix":
                        return new LocalDrivePrefix { Text = p.ToString(), Drive = p["DriveLetter"].ToString() };
                    case "UncPrefix":
                        return new UncPrefix { Text = p.Text.ToString(), Server = p["ServerName"].ToString(), Share = p["ShareName"].ToString() };
                    case "LocalDrivePrefix":
                        return new LocalDrivePrefix { Text = p.ToString(), Drive = p["DriveLetter"].ToString() };
                    case "RootRelative":
                        return new RootRelativePrefix { Text = p.ToString() };
                    case "RelativePrefix":
                        return new RelativePrefix { Text = p.ToString() };
                }
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException("unknown prefix: " + p.Details, ex);
            }
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
                throw new ArgumentNullException("prefix");
            }
            if (parts == null)
            {
                throw new ArgumentNullException("path");
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
            var prefix = PathParser.Prefix()(new Text(p));
            if (prefix == null)
            {
                throw new ArgumentOutOfRangeException(String.Format("{0} is not a valid path prefix.", p.Quote()));
            }
            return GetPrefix(prefix);
        }

        public static LPath CreateRelative(params string[] parts)
        {
            return CreateRelative((IEnumerable<string>)parts);
        }

        public static LPath CreateRelative(IEnumerable<string> parts)
        {
            return CreateRelative(Sidi.IO.FileSystem.Current, parts);
        }

        public static LPath CreateRelative(IFileSystem fileSystem, IEnumerable<string> parts)
        {
            return new LPath(fileSystem, RelativePrefix, parts);
        }

        /// <summary>
        /// File system interface on which the path will operate if not specified otherwise.
        /// </summary>
        public IFileSystem FileSystem
        {
            get
            {
                return fileSystem;
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
            return path.StringRepresentation;
        }

        public string StringRepresentation
        {
            get
            {
                return Prefix + Parts.Join(DirectorySeparator);
            }
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
            return this.StringRepresentation.Quote();
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
                if (!PathParser.IsMatch(x, PathParser.NtfsFilename()))
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
                if (!PathParser.IsMatch(x, PathParser.NtfsFilenameWithWildcards()))
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

        static bool IsDriveSpecifier(string d)
        {
            return d.Length == 2 && char.IsLetter(d[0]) && d[1] == ':';
        }

        public LPath Canonic
        {
            get
            {
                return new LPath(FileSystem, Prefix, Parts.Where(x => !x.Equals(".")));
            }
        }

        /// <summary>
        /// Appends a number to the file name so that the returned path points to a file 
        /// in the same directory that does not exist yet
        /// </summary>
        /// Example: `C:\\temp\\myimage.jpg` => `C:\\temp\\myimage.1.jpg`
        /// <returns>Unique path</returns>
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
        /// Root of the path, e.g. C:\\ or \\\\server\\share\\
        /// </summary>
        public LPath Root
        {
            get
            {
                return new LPath(FileSystem, Prefix, Enumerable.Empty<string>());
            }
        }

        public bool IsAbsolute
        {
            get
            {
                return prefix is LocalDrivePrefix || prefix is UncPrefix || prefix is DeviceNamespacePrefix;
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
            return !p1.IsRelative && !p2.IsRelative && object.Equals(p1.Root, p2.Root);
        }

        /// <summary>
        /// Returns the drive letter for local absolute paths, null otherwise
        /// </summary>
        public string DriveLetter
        {
            get
            {
                var p = prefix as LocalDrivePrefix;
                if (p == null) return null;
                return p.Drive;
            }
        }

        /// <summary>
        /// Returns the server name for UNC paths, null otherwise
        /// </summary>
        public string Server
        {
            get
            {
                var p = prefix as UncPrefix;
                if (p == null) return null;
                return p.Server;
            }
        }

        /// <summary>
        /// Returns the share name for UNC paths, null otherwise
        /// </summary>
        public string Share
        {
            get
            {
                var p = prefix as UncPrefix;
                if (p == null) return null;
                return p.Share;
            }
        }

        public static readonly char DirectorySeparatorChar = System.IO.Path.DirectorySeparatorChar;

        public IFileSystemInfo Info
        {
            get
            {
                return FileSystem.GetInfo(this);
            }
        }

        public IHardLinkInfo HardLinkInfo
        {
            get
            {
                return FileSystem.GetHardLinkInfo(this);
            }
        }

        private LPath GetFullPathImpl()
        {
            if (prefix is RootRelativePrefix)
            {
                return FileSystem.CurrentDirectory.Root.CatDir(Parts);
            }
            else if (prefix is RelativePrefix)
            {
                return FileSystem.CurrentDirectory.CatDir(this.Parts);
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

        public LPath CatDir(IEnumerable<string> relativePaths)
        {
            return CatDir(relativePaths.Select(x => LPath.CreateRelative(x)));
        }

        public LPath CatDir(IEnumerable<LPath> relativePaths)
        {
            foreach (var i in relativePaths)
            {
                if (!i.IsRelative)
                {
                    throw new ArgumentOutOfRangeException(String.Format("cannot CatDir non-relative path: {0}", i));
                }
            }
            return new LPath(FileSystem, Prefix, Parts.Concat(relativePaths.SelectMany(_ => _.Parts)));
        }

        public LPath CatDir(params LPath[] relativePaths)
        {
            return CatDir((IEnumerable<LPath>)relativePaths);
        }

        /// <summary>
        /// Attaches a postfix to the filename. can for example be used to add an extension to a path
        /// </summary>
        /// <param name="namePostfix"></param>
        /// <returns>Modified path</returns>
        public LPath CatName(string namePostfix)
        {
            return new LPath(FileSystem, Prefix, Parts.TakeAllBut(1).Concat(new[] { FileName + namePostfix }));
        }

        public const int MaxFilenameLength = 255;

        internal const int MaxPathLength = 32000;

        public static bool IsValid(string path)
        {
            var text = new Sidi.Parse.Text(path);
            var t = text.Copy();
            try
            {
                var ast = PathParser.Path()(t);
                return t.Length == 0;
            }
            catch (Exception)
            {
                return false;
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
            return new LPath(FileSystem, Prefix, p);
        }

        public LPath Sibling(string siblingName)
        {
            var p = Parent;
            if (p == null)
            {
                throw new InvalidOperationException("the path cannot have siblings since it does not have a parent");
            }
            return p.CatDir(siblingName);
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

            return new LPath(FileSystem, String.Empty, result);
        }
        
        /// <summary>
        /// Parent of this path, or null if path is a root path
        /// </summary>
        public LPath Parent
        {
            get
            {
                if (Parts.Any())
                {
                    return new LPath(FileSystem, Prefix, Parts.TakeAllBut(1));
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// List of all parents of this path down to the path root, starting with this path
        /// </summary>
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
            return ToListOfPath(FileSystem.FindFile(searchPath));
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

        static IList<LPath> ToListOfPath(IEnumerable<IFileSystemInfo> list)
        {
            return list.Select(x => x.FullName).ToList();
        }

        public static readonly string AllFilesWildcard = "*";

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
            return StringRepresentation;
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
                return IsRoot && this.prefix is LocalDrivePrefix;
            }
        }

        public bool IsUnc
        {
            get
            {
                return prefix is UncPrefix;
            }
        }

        public override int GetHashCode()
        {
            int hc = Prefix.ToLower().GetHashCode();
            foreach (var i in Parts)
            {
                hc += 17 * i.ToLower().GetHashCode();
            }
            return hc;
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
                && FileSystem == other.FileSystem
                && StringComparer.InvariantCultureIgnoreCase.Compare(Prefix, other.Prefix) == 0
                && Parts.SequenceEqual(other.Parts, StringComparer.InvariantCultureIgnoreCase);
        }
        
        public bool Contains(string value)
        {
            return this.StringRepresentation.IndexOf(value, StringComparison) >= 0;
        }

        static StringComparer partComparer = StringComparer.InvariantCultureIgnoreCase;

        static void AssertCompatibleFileSystems(params LPath[] paths)
        {
            var e = ((IEnumerable<LPath>)paths).GetEnumerator();
            if (!e.MoveNext())
            {
                return;
            }
            IFileSystem fs = e.Current.FileSystem;
            for (; e.MoveNext(); )
            {
                if (e.Current.FileSystem != fs)
                {
                    throw new ArgumentOutOfRangeException("file systems are incompatible");
                }
            }
        }

        public LPath RelativeTo(LPath root)
        {
            AssertCompatibleFileSystems(this, root);

            var p = Parts;
            var rp = root.Parts;

            if (!rp.SequenceEqual(p.Take(rp.Length), partComparer))
            {
                throw new ArgumentOutOfRangeException("root");
            }

            var difference = Parts.Skip(rp.Length);

            if (difference.Any())
            {
                return LPath.CreateRelative(this.FileSystem, difference);
            }
            else
            {
                return LPath.CreateRelative(this.FileSystem, new[] { "..", this.FileName });
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
                    this.DeleteFile();
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
                throw new System.IO.IOException("Cannot delete {0}".F(this));
            }
        }

        /// <summary>
        /// Ensures that this path does not exist. Warning: will delete the whole directory tree if
        /// path points to a directory. Will not follow Junction Points, i.e. will only delete the 
        /// junction point, but not the directory it points to.
        /// </summary>
        public void EnsureNotExists()
        {
            var info = this.Info;
            
            if (!info.Exists)
            {
                return;
            }
            else if (info.IsFile)
            {
                FileSystem.DeleteFile(this);
                return;
            }
            else if (info.IsDirectory)
            {
                for (int i = 0; i < 100; ++i)
                {
                    try
                    {
                        FileSystem.RemoveDirectory(this);
                        return;
                    }
                    catch (System.IO.IOException)
                    {
                    }

                    foreach (var child in this.GetChildren())
                    {
                        child.EnsureNotExists();
                    }

                    if (i > 0)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(100));
                    }
                }
            }
            else
            {
                throw new NotImplementedException(this.ToString());
            }
        }

        public void EnsureParentDirectoryExists()
        {
            Parent.EnsureDirectoryExists();
        }

        public void EnsureDirectoryExists()
        {
            FileSystem.EnsureDirectoryExists(this);
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
        /// Returns the extension of the file name without the dot (.)
        /// </summary>
        /// Returns null if no extension exists
        /// Example: c:\\image.jpg => jpg
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
                fileSystem = Sidi.IO.FileSystem.Current;
                prefix = p.prefix;
                parts = p.parts;
                reader.ReadEndElement();
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteString(StringRepresentation);
        }

        public int CompareTo(object obj)
        {
            var r = obj as LPath;
            if (r == null)
            {
                throw new System.ArgumentException("Parameter must be of Type LPath", "obj");
            }
            return this.StringRepresentation.CompareTo(r.StringRepresentation);
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
            EnsureParentDirectoryExists();

            return FileSystem.Open(this,
                System.IO.FileMode.Create,
                System.IO.FileAccess.ReadWrite,
                System.IO.FileShare.Read);
        }

        public System.IO.Stream OpenRead()
        {
            return FileSystem.Open(this,
                System.IO.FileMode.Open,
                System.IO.FileAccess.Read,
                System.IO.FileShare.ReadWrite);
        }

        /// <summary>
        /// The file:// URI represenation of the path
        /// </summary>
        public Uri Uri
        {
            get
            {
                return new Uri(StringRepresentation, UriKind.Absolute);
            }
        }

        public static LPath GetRandomFileName()
        {
            return new LPath(System.IO.Path.GetRandomFileName());
        }

        /// <summary>
        /// True, if this path is a descendant of or identical to other
        /// </summary>
        /// <param name="ancestor"></param>
        /// <returns></returns>
        public bool IsDescendantOrSelf(LPath ancestor)
        {
            if (object.Equals(this, ancestor))
            {
                return true;
            }

            var p = Parent;
            if (p == null)
            {
                return false;
            }

            return p.IsDescendantOrSelf(ancestor);
        }

        /// <summary>
        /// True, if this path is a descendant of other
        /// </summary>
        /// <param name="ancestor"></param>
        /// <returns></returns>
        public bool IsDescendant(LPath ancestor)
        {
            var p = this.Parent;
            if (p == null)
            {
                return false;
            }

            return p.IsDescendantOrSelf(ancestor);
        }

        public static LPath GetUncRoot(string server, string share)
        {
            return new LPath(Sidi.IO.FileSystem.Current, shortUncPrefix + server + DirectorySeparator + share + DirectorySeparator, Enumerable.Empty<string>());
        }

        public static LPath GetDriveRoot(char driveLetter)
        {
            return new LPath(Sidi.IO.FileSystem.Current, String.Format(@"{0}:\", driveLetter), Enumerable.Empty<string>());
        }

        public static LPath GetDriveRoot(string driveLetter)
        {
            if (driveLetter.Length != 1)
            {
                throw new ArgumentOutOfRangeException("driveLetter");
            }
            return GetDriveRoot(driveLetter[0]);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("p", StringRepresentation);
        }

        protected LPath(SerializationInfo info, StreamingContext context)
        : this(info.GetString("p"))
        {
        }

        /// <summary>
        /// Converts an absolute path into a relative path
        /// </summary>
        /// Converts an absolute path into a relative path by converting its prefix into file names
        /// Example: C:\\temp => local\\C\\temp
        /// Example: \\\\server\\share\\somfile => unc\\server\\share\\somefile
        /// <returns>Relative path</returns>
        public LPath ToRelative()
        {
            if (prefix is LocalDrivePrefix)
            {
                return CreateRelative("local", LPath.GetValidFilename(DriveLetter)).CatDir(Parts);
            }
            else if (prefix is UncPrefix)
            {
                return CreateRelative("unc", LPath.GetValidFilename(Server), LPath.GetValidFilename(Share)).CatDir(Parts);
            }
            else
            {
                return this;
            }
        }
    }
}
