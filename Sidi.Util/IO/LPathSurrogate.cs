using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    [ProtoContract]
    public class LPathSurrogate
    {
        public LPathSurrogate(LPath p)
        {
            Value = p;
        }

        [ProtoMember(1)]
        public readonly string Value;

        public static implicit operator LPathSurrogate(LPath x)
        {
            return x == null ? null : new LPathSurrogate(x);
        }

        public static implicit operator LPath (LPathSurrogate x)
        {
            return x == null ? null : new LPath(x.Value);
        }
    }
}
