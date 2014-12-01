using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.IO
{
    /// <summary>
    /// Path and last write time. Use to remember file versions.
    /// </summary>
    [Serializable]
    public class LPathWriteTime
    {
        public LPathWriteTime(LPath p)
        {
            this.Path = p;
            this.LastWriteTimeUtc = p.Info.LastWriteTimeUtc;
        }

        public LPath Path;
        public DateTime LastWriteTimeUtc;

        public override string ToString()
        {
            return String.Format(@"{0} written at {1}", Path, LastWriteTimeUtc);
        }

        public override bool Equals(object obj)
        {
            var r = obj as LPathWriteTime;
            return r != null && 
                object.Equals(Path, r.Path) && 
                object.Equals(LastWriteTimeUtc, r.LastWriteTimeUtc);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash += 23 * Path.GetHashCode();
            hash += 23 * LastWriteTimeUtc.GetHashCode();
            return hash;
        }
    }
}
