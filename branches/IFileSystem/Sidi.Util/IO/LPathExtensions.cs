using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    public static class LPathExtensions
    {
        public static void WriteAllText(this LPath path, string text)
        {
            path.EnsureParentDirectoryExists();
            using (var w = path.WriteText())
            {
                w.Write(text);
            }
        }

        public static string ReadAllText(this LPath path)
        {
            using (var r = path.ReadText())
            {
                return r.ReadToEnd();
            }
        }
    }
}
