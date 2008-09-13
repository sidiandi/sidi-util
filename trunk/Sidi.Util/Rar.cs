// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
            Process p = new Process();
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
