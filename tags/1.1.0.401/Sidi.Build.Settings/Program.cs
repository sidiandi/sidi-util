using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.CommandLine;

namespace Sidi.Build
{
    class SettingsProgram
    {
        static void Main(string[] args)
        {
            var s = new Sidi.Build.GoogleCode.Upload();
            Parser.Run(s, args);
        }
    }
}
