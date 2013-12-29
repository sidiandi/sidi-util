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
using System.Linq;
using System.Text;
using Sidi.IO;
using System.Diagnostics;
using Sidi.Util;
using Sidi.Extensions;
using System.IO;

namespace Sidi.Build
{
    /// <summary>
    /// Reads and writes streams in PDB files. Wrapper for the pdbstr.exe tool.
    /// </summary>
    public class Pdbstr
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static string Program
        {
            get
            {
                var f = new Sidi.IO.LPath(DebuggingToolsForWindows.Directory).CatDir("srcsrv", "pdbstr.exe");

                if (!f.Exists)
                {
                    throw new FileNotFoundException(String.Format(
                        "pdbstr.exe must be installed at {0}. See {1}",
                        f,
                        DebuggingToolsForWindows.DownloadUrl));
                   }
                return f;
                }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Pdbstr()
        {
        }

        /// <summary>
        /// Write a stream to a PDB file
        /// </summary>
        /// <param name="pdbFile">PDB file name</param>
        /// <param name="streamName">Name of the stream</param>
        /// <param name="content">Content of the stream</param>
        public void Write(string pdbFile, string streamName, string content)
        {
            var temp = Sidi.IO.LPath.GetTempFileName();

            using (var w = new StreamWriter(temp))
            {
                w.Write(content);
            }

            var p = new Process();
            p.StartInfo.FileName = Program;
            p.StartInfo.Arguments = String.Format("-w {0} -s:{1} {2}", ("-p:" + pdbFile).Quote(),
                streamName,
                ("-i:" + temp).Quote()
                );

            var r = p.Read().ReadToEnd();

            if (!String.IsNullOrEmpty(r))
            {
                throw new Exception(r);
            }
        }

        /// <summary>
        /// Reads a stream from a PDB file
        /// </summary>
        /// <param name="pdbFile">PDB file name</param>
        /// <param name="streamName">Stream name</param>
        /// <returns>Content of the stream</returns>
        public string Read(string pdbFile, string streamName)
        {
            var p = new Process();
            p.StartInfo.FileName = Program;
            p.StartInfo.Arguments = String.Format("-r {0} -s:{1}", ("-p:" + pdbFile).Quote(),
                streamName);

            return p.Read().ReadToEnd();
        }
    }
}
