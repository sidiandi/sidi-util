using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;

namespace Sidi
{
    public class TestBase
    {
        protected string TestFile(string relPath)
        {
            return FileUtil.BinFile(relPath);
        }
    }
}
