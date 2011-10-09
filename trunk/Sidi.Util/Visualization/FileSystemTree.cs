using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Sidi.Visualization
{
    public class FileSystemTree
    {
        public static Tree<Sidi.IO.Long.FileSystemInfo> Get(Sidi.IO.Long.LongName dir)
        {
            return GetRecursive(null, new Sidi.IO.Long.FileSystemInfo(dir));
        }

        static Tree<Sidi.IO.Long.FileSystemInfo> GetRecursive(
            Tree<Sidi.IO.Long.FileSystemInfo> parent,
            Sidi.IO.Long.FileSystemInfo i)
        {
            var t = new Tree<Sidi.IO.Long.FileSystemInfo>(parent);
            t.Data = i;
            if (i.IsDirectory)
            {
                t.Children = i.GetChilds()
                    .Select(x => GetRecursive(t, x))
                    .OrderByDescending(x => x.Size)
                    .ToList();
                t.Size = t.ChildSize;
            }
            else
            {
                t.Size = i.Length;
            }
            return t;
        }

        public static Color ExtensionToColor(Sidi.IO.Long.FileSystemInfo i)
        {
            string extString = i.Extension;
            if (extString.Length > 1)
            {
                extString = extString.Substring(1);
            }
            else
            {
                extString = String.Empty;
            }

            var ext = SortPos(extString);
            var hsl = new HSLColor(Color.Red);
            hsl.Hue = 360.0 * ext;
            return hsl;
        }

        static double SortPos(string x)
        {
            double f = 1.0;
            double y = 0.0;
            foreach (var c in x.ToLower())
            {
                f /= (double)('z' - 'a');
                y += (double)(c - 'a') * f;
            }
            return y;
        }
    }
}
