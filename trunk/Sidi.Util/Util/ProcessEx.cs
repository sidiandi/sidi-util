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
using System.Diagnostics;
using System.IO;
using L = Sidi.IO;
using System.Linq;

namespace Sidi.Util
{
    public static class ProcessExtensions
    {
        public static IEnumerable<string> ReadLines(this Process process)
        {
            var r = process.Read();
            for (; ; )
            {
                string line = r.ReadLine();
                if (line == null)
                {
                    break;
                }
                yield return line;
            }
        }

        public static TextReader Read(this Process process)
        {
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            return process.StandardOutput;
        }

        public static string DetailedInfo(this Process p)
        {
            return String.Format("{0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments);
        }

        public static bool IsAlreadyRunning(this Process p)
        {
            var sp = Sidi.IO.SearchPath.PATH;
            sp.Paths.Insert(0, p.StartInfo.WorkingDirectory);
            string pFilename = Path.GetFullPath(sp.Find(p.StartInfo.FileName)).ToLower();

            return Process.GetProcesses().Any(x =>
                {
                    try
                    {
                        string filename = Path.GetFullPath(x.MainModule.FileName).ToLower();
                        return filename.Equals(pFilename);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                });
        }

        public static string ExitCodeInfo(this Process p)
        {
            return String.Format("exited with exit code {0} : {1}", p.ExitCode, p.DetailedInfo());
        }

        public static void CheckResult(this Process p)
        {
            p.CheckResult(0);
        }

        public static void CheckResult(this Process p, int expectedExitCode)
        {
            if (p.ExitCode != expectedExitCode)
            {
                throw new ProcessFailedException(p);
            }
        }

    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly"), Serializable]
    public class ProcessFailedException : Exception
    {
        Process process;

        public ProcessFailedException(Process p)
        {
            process = p;
        }

        public override string Message
        {
            get
            {
                return process.ExitCodeInfo();
            }
        }
    }

}
