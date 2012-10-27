using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.IO.Long
{
    public class FileType
    {
        /// <summary>
        /// Specify all extensions you want to match
        /// </summary>
        /// <param name="extensions">list of extensions without "."</param>
        public FileType(params string[] extensions)
        {
            e = new HashSet<string>(extensions.Select(x => "." + x.ToLower()));
        }

        HashSet<string> e;

        public bool Is(Path fileName)
        {
            return e.Contains(fileName.Extension.ToLower());
        }
    }

}
