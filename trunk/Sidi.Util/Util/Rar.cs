// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Sidi.Util
{
    class Rar : IArchive
    {
        static string rarBin = @"C:\Programme\bin\rar.exe";

        string path;
        string password;

        public Rar(string a_path)
        {
            path = a_path;
        }

        public string Password
        {
            set
            {
                password = value;
            }
        }

        public void Extract(string destinationDirectory, string memberName)
        {
            Directory.CreateDirectory(destinationDirectory);
            RunRar(String.Format("x -p{3} -o- \"-w{1}\" \"{0}\" \"{2}\"", path, destinationDirectory, memberName, password), destinationDirectory);
        }

        public void ExtractAll(string destinationDirectory)
        {
            Directory.CreateDirectory(destinationDirectory);
            RunRar(String.Format("x -p{2} -o- \"-w{1}\" \"{0}\"", path, destinationDirectory, password), destinationDirectory);
        }

        void RunRar(string arguments, string workingDirectory)
        {
            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = rarBin;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WorkingDirectory = workingDirectory;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                StreamReader sr = p.StandardOutput;
                for (; ; )
                {
                    string line = sr.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                }
            }
        }
    }
}
