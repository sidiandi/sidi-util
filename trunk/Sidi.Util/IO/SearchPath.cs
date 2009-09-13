using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sidi.IO
{
    public class SearchPath
    {
        public List<string> Paths;

        public SearchPath()
        {
            Paths = new List<string>();
        }

        public void AppendIfNotExist(string dir)
        {
            if (Paths.Contains(dir))
            {
                return;
            }
            Paths.Add(dir);
        }

        public void PrependIfNotExist(string dir)
        {
            if (Paths.Contains(dir))
            {
                return;
            }
            Paths.Insert(0, dir);
        }

        public static SearchPath Parse(string semiColonSeparatedPath)
        {
            SearchPath p = new SearchPath();
            p.Paths = new List<string>(semiColonSeparatedPath.SplitPathList());
            return p;
        }

        public override string ToString()
        {
            return Paths.Aggregate((x, y) => x + ";" + y);
        }

        public string NewLineSeparatedString
        {
            get
            {
                return Paths.Aggregate((x, y) => x + "\r\n" + y);
            }
        }

        public string Find(string file)
        {
            string path;
            if (TryFind(file, out path))
            {
                return path;
            }

            StringBuilder b = new StringBuilder();
            foreach (string i in Paths)
            {
                b.AppendLine(FileUtil.CatDir(i, file));
            }

            throw new System.IO.FileNotFoundException(b.ToString(), file);
        }

        public bool TryFind(string file, out string path)
        {
            foreach (string i in Paths)
            {
                string p = FileUtil.CatDir(i, file);
                if (File.Exists(p))
                {
                    path = p;
                    return true;
                }
            }
            path = null;
            return false;
        }
    }
}
