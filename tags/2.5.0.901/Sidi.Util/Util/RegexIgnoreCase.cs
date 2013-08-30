using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sidi.Util
{
    public class RegexIgnoreCase : Regex
    {
        public RegexIgnoreCase(string pattern)
        : base(pattern, RegexOptions.IgnoreCase)
        {
        }
    }
}
