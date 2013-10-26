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
using System.IO;
using Sidi.Extensions;

namespace Sidi.Build
{
    /// <summary>
    /// Extract source information from a PDB file. Wrapper for srctool.exe. 
    /// "Debugging Tools for Windows (x86)" must be installed.
    /// </summary>
    public class Srctool
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Default constructor
        /// </summary>
        public Srctool()
        {
        }

        Sidi.IO.LPath Program
        {
            get
            {
                return DebuggingToolsForWindows.Directory.CatDir("srcsrv", "srctool.exe");
            }
        }

        /// <summary>
        /// Dumps the original source file names
        /// </summary>
        /// <param name="pdbFile">PDB file name</param>
        /// <returns>Sequence of source file names</returns>
        public IEnumerable<string> DumpRaw(string pdbFile)
        {
            var pdbFilePath = new Sidi.IO.LPath(pdbFile);
            if (!pdbFilePath.IsFile)
            {
                throw new FileNotFoundException(pdbFile);
            }

            Process p = new Process();
            p.StartInfo.FileName = Program;
            p.StartInfo.Arguments = String.Format("-r {0}", pdbFile.Quote());
            var files = p.ReadLines().ToList();
            return files.Take(files.Count - 1);
        }

        public void Extract(string pdbFile)
        {
            var pdbFilePath = new Sidi.IO.LPath(pdbFile);
            if (!pdbFilePath.IsFile)
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
