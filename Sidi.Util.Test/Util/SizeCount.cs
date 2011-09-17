using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;

namespace Sidi.Tool
{
    public class SizeCount
    {
        public long Size { private set; get; }
        public long Count { private set; get; }

        public void Add(long s)
        {
            Size += s;
            ++Count;
        }

        public override string ToString()
        {
            var b = new BinaryPrefix();
            return String.Format(b, "{0} files ({1:B}B)", Count, Size);
        }
    }
}
