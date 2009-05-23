using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;
using System.Diagnostics;
using Sidi.Util;
using System.IO;

namespace Sidi.Build
{
    public class Srctool
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Srctool()
        {
        }

        string Program
        {
            get
            {
                return FileUtil.CatDir(
                    DebuggingToolsForWindows.Directory,
                    "srcsrv",
                    "srctool.exe");
            }
        }

        public IEnumerable<string> DumpRaw(string pdbFile)
        {
            if (!File.Exists(pdbFile))
            {
                throw new FileNotFoundException(pdbFile);
            }

            Process p = new Process();
            p.StartInfo.FileName = Program;
            p.StartInfo.Arguments = String.Format("-r {0}", pdbFile.Quote());
            return p.ReadLines();
        }

        public void Extract(string pdbFile)
        {
            if (!File.Exists(pdbFile))
            {
                throw new FileNotFoundException(pdbFile);
            }

            Process p = new Process();
            p.StartInfo.FileName = Program;
            p.StartInfo.Arguments = String.Format("-x {0}", pdbFile.Quote());
            foreach (var i in p.ReadLines())
            {
                log.Info(i);
            }
        }
    }
}
