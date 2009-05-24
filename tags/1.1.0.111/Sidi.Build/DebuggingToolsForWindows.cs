using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;
using System.IO;

namespace Sidi.Build
{
    public static class DebuggingToolsForWindows
    {
        public static string DownloadUrl
        {
            get
            {
                return "http://www.microsoft.com/whdc/devtools/debugging/default.mspx";
            }
        }
        
        public static string Directory
        {
            get
            {
                string d = FileUtil.CatDir(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Debugging Tools for Windows");

                if (!System.IO.Directory.Exists(d))
                {
                    throw new DirectoryNotFoundException(String.Format("The Debugging Tools for Windows must be installed at {0}. See {1}",
                        d,
                        DownloadUrl));
                }

                return d;
            }
        }
    }
}
