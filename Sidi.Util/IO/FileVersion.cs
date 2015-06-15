using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    /// <summary>
    /// Identifies a version of a file or directory by path, last write time and length
    /// </summary>
    /// For a directory, the last write time is latest last write of any file in this directory (recursively)
    /// For a directory, the length is the sum of the lengths of all files in this directory (recursively)
    [Serializable]
    public sealed class FileVersion : IEquatable<FileVersion>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override string ToString()
        {
            return String.Format("{0} [{1}, {2}]", Path, LastWriteTimeUtc, Length);
        }

        FileVersion()
        {
        }
        
        public static FileVersion Get(LPath path)
        {
            return Get(path.Info);
        }

        public static FileVersion Get(IFileSystemInfo info)
        {
            var fv = new FileVersion { Path = info.FullName };

            if (info.IsDirectory)
            {
                var versions = info.GetChildren().Select(x => FileVersion.Get(x)).ToList();
                if (versions.Any())
                {
                    fv.Length = versions.Sum(_ => _.Length);
                    fv.LastWriteTimeUtc = new[] { info.LastWriteTimeUtc }.Concat(versions.Select(_ => _.LastWriteTimeUtc)).Max();
                }
                else
                {
                    fv.Length = 0;
                    fv.LastWriteTimeUtc = info.LastAccessTimeUtc;
                }
            }
            else
            {
                fv.Length = info.Length;
                fv.LastWriteTimeUtc = info.LastWriteTimeUtc;
            }
            return fv;
        }

        public FileVersion(LPath path, long length, DateTime lastWriteTimeUtc)
        {
            this.Path = path;
            this.Length = length;
            this.LastWriteTimeUtc = lastWriteTimeUtc;
        }

        public string Path { get; set; }
        public long Length { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((FileVersion)obj);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                hash = hash * 16777619 ^ Path.GetHashCode();
                hash = hash * 16777619 ^ Length.GetHashCode();
                hash = hash * 16777619 ^ LastWriteTimeUtc.GetHashCode();
                return hash;
            }
        }

        public bool Equals(FileVersion other)
        {
            return object.Equals(Path, other.Path)
                && object.Equals(Length, other.Length)
                && object.Equals(LastWriteTimeUtc, other.LastWriteTimeUtc);
        }
    }
}
