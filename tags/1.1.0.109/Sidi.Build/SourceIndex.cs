using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using System.IO;
using Sidi.IO;
using Sidi.Util;
using Microsoft.Build.Utilities;

namespace Sidi.Build
{
    public class SourceIndex : Task
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string SrcsrvStream
        {
            get
            {
                return "srcsrv";
            }
        }
        
        public SourceIndex()
        {
        }

        public string Directory { set; get; }
        public string Url { set; get; }

        public ITaskItem[] Modules { set; get; }

        public override bool Execute()
        {
            if (Directory == null)
            {
                throw new ArgumentNullException("Directory");
            }

            if (Url == null)
            {
                throw new ArgumentNullException("Url");
            }

            if (!System.IO.Directory.Exists(Directory))
            {
                throw new DirectoryNotFoundException(Directory);
            }

            foreach (var i in Modules)
            {
                string pdbFile = i.ItemSpec.ReplaceExtension("pdb");
                AddSourceIndex(pdbFile);
            }
            return true;
        }

        public void AddSourceIndex(string pdbFile)
        {
            if (!File.Exists(pdbFile))
            {
                throw new FileNotFoundException(pdbFile);
            }

            StringWriter w = new StringWriter();

            Srctool srctool = new Srctool();

            w.WriteLine("SRCSRV: ini ------------------------------------------------");
            w.WriteLine("VERSION=2");
            w.WriteLine("INDEXVERSION=2");
            w.WriteLine("VERCTRL=http");
            w.WriteLine("DATETIME={0}", DateTime.Now.ToString());
            w.WriteLine("SRCSRV: variables ------------------------------------------");
            w.WriteLine("SRCSRVVERCTRL=http");
            w.WriteLine("HTTP_ALIAS={0}", Url);
            w.WriteLine("HTTP_EXTRACT_TARGET=%HTTP_ALIAS%/%var2%");
            w.WriteLine("SRCSRVTRG=%http_extract_target%");
            w.WriteLine("SRCSRVCMD=");
            w.WriteLine("SRCSRV: source files ---------------------------------------");

            DirectoryInfo d = new DirectoryInfo(Directory);
            int offset = d.FullName.Length + 1;

            foreach (string i in srctool.DumpRaw(pdbFile))
            {
                FileInfo f = new FileInfo(i);
                if (!f.FullName.ToLower().StartsWith(d.FullName.ToLower()))
                {
                    Log.LogWarning("Source file {0} is not in root directory {1} and will not be source indexed", i, Directory);
                    continue;
                }

                string url = f.FullName.Substring(offset);
                url = url.Replace(@"\", "/");

                w.WriteLine(
                    String.Join("*", new string[]
                    {
                        i, url,
                    }));
            }

            w.WriteLine("SRCSRV: end ------------------------------------------------");

            Pdbstr pdbstr = new Pdbstr();
            pdbstr.Write(pdbFile, SourceIndex.SrcsrvStream, w.ToString());

            Log.LogMessage("Symbol file {0} source indexed to {1}", pdbFile, Url);
        }
    }
}
