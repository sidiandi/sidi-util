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

        public FileVersion(LPath path)
        {
            this.path = path;
            var c = path.GetChildren();

            if (c.Any())
            {
                var versions = c.Select(x => new FileVersion(x)).ToList();
                this.length = versions.Sum(_ => _.length);
                this.lastWriteTimeUtc = versions.Max(_ => _.lastWriteTimeUtc);
            }
            else
            {
                var info = path.Info;
                this.length = info.Length;
                this.lastWriteTimeUtc = info.LastWriteTimeUtc;
            }
        }

        public LPath Path { get { return path; } }
        public long Length { get { return length; } }
        public DateTime LastWriteTimeUtc { get { return lastWriteTimeUtc; } }

        readonly LPath path;
        readonly long length;
        readonly DateTime lastWriteTimeUtc;

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
                hash = hash * 16777619 ^ path.GetHashCode();
                hash = hash * 16777619 ^ length.GetHashCode();
                hash = hash * 16777619 ^ lastWriteTimeUtc.GetHashCode();
                return hash;
            }
        }

        public bool Equals(FileVersion other)
        {
            return object.Equals(path, other.path)
                && object.Equals(length, other.length)
                && object.Equals(lastWriteTimeUtc, other.lastWriteTimeUtc);
        }
    }
}
