using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using System.IO;
using Sidi.CommandLine;
using System.Text.RegularExpressions;

namespace Sidi.IO.Long
{
    /// <summary>
    /// Command line parser interface for Sidi.IO.Long.Enum
    /// </summary>
    public class EnumConfig
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Enum Enumerator = new Enum();
        List<Regex> exclude = new List<Regex>();
        
        public EnumConfig()
        {
            Enumerator.Output = x => !exclude.Any(e => e.IsMatch(x.Name));
            Enumerator.Follow = x => !exclude.Any(e => e.IsMatch(x.Name));
        }

        [Usage("Include a path")]
        public void Include(string p)
        {
            Enumerator.AddRoot(p.Long());
        }

        [Usage("Exclude a file name pattern")]
        public void Exclude(string p)
        {
            exclude.Add(new Regex(p));
        }
    }
}
