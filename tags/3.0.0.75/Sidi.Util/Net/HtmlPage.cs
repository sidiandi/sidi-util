using Sidi.IO;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Net
{
    public class HtmlPage
    {
        public static void Show(Action<TextWriter> page)
        {
            var tempFile =  Paths.GetTempFile(".html");
            using (var w = tempFile.WriteText())
            {
                page(w);
            }
            Process.Start(tempFile);
        }
    }
}
