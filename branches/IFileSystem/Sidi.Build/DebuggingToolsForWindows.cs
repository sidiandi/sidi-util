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

using Sidi.IO;
using System;
using System.IO;
using System.Linq;

namespace Sidi.Build
{
    public class DebuggingToolsForWindows
    {
        public DebuggingToolsForWindows()
        {
            Directory = GetDirectory();
        }

        public static string DownloadUrl
        {
            get
            {
                return "http://www.microsoft.com/whdc/devtools/debugging/default.mspx";
            }
        }

        public LPath Directory { get; private set; }
        
        static LPath GetDirectory()
        {
            LPath[] searchPath =
            {
                Paths.GetFolderPath(Environment.SpecialFolder.ProgramFiles).CatDir(@"Windows Kits\8.0\Debuggers\x86"),
                Paths.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).CatDir("Debugging Tools for Windows (x86)"),
                Paths.GetFolderPath(Environment.SpecialFolder.ProgramFiles).CatDir("Debugging Tools for Windows")
            };

            try
            {
                return searchPath.First(d => d.IsDirectory);
            }
            catch (InvalidOperationException exception)
            {
                throw new DirectoryNotFoundException(String.Format("The Debugging Tools for Windows must be installed at {0}. See {1}",
                    searchPath[0], DownloadUrl), exception);
            }
        }
    }
}
