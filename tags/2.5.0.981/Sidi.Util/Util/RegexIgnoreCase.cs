using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace Sidi.Util
{
    [Serializable]
    public class RegexIgnoreCase : Regex
    {
        protected RegexIgnoreCase(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
        }

        public RegexIgnoreCase(string pattern)
        : base(pattern, RegexOptions.IgnoreCase)
        {
        }
    }
}
