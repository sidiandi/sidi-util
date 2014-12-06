// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using System.IO;
using Sidi.IO;
using Sidi.Util;
using Microsoft.Build.Utilities;

namespace Sidi.Build
{
    /// <summary>
    /// Modifies the symbol files (*.pdb) of specified modules so that the debugger can download 
    /// the source files from a URL
    /// </summary>
    public class SourceIndex : Task
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Name of the stream in the PDB file containing the source information
        /// </summary>
        public static string SrcsrvStream
        {
            get
            {
                return "srcsrv";
            }
        }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public SourceIndex()
        {
        }

        /// <summary>
        /// Local source directory
        /// </summary>
        public string Directory { set; get; }
        
        /// <summary>
        /// URL that contains the same source files as the local source directory
        /// </summary>
        public string Url { set; get; }

        /// <summary>
        /// Modules to be source-indexed
        /// </summary>
        public ITaskItem[] Modules { set; get; }

        /// <summary>
        /// Executes the task
        /// </summary>
        /// <returns></returns>
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
                var pdbFile = new Sidi.IO.LPath(i.ItemSpec).ChangeExtension("pdb");
                AddSourceIndex(pdbFile);
            }
            return true;
        }

        /// <summary>
        /// Modifies pdbFile so that it references the source files not over a local directory but an URL
        /// </summary>
        /// <param name="pdbFile"></param>
        public void AddSourceIndex(string pdbFile)
        {
            var pdbFilePath = new Sidi.IO.LPath(pdbFile);
            if (!pdbFilePath.IsFile)
            {
                throw new FileNotFoundException(pdbFile);
            }

            StringWriter w = new StringWriter();

            Srctool srctool = new Srctool();

            w.WriteLine("SRCSRV: ini ------------------------------------------------");
            w.WriteLine("VERSION=2");
            w.WriteLine("INDEXVERSION=2");
            w.WriteLine("VERCTRL=http");
            w.WriteLine("DATETIME={0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
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
