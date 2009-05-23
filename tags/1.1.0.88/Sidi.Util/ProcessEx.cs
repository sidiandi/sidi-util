using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Sidi.Util
{
    public static class ProcessEx
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
    }
}
