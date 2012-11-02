using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidi.Util;
using Sidi.Extensions;

namespace Sidi.IO
{
    public class PathList : List<LPath>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PathList(IEnumerable<LPath> paths)
        : base(paths)
        {
        }

        public PathList()
        {
        }

        public static PathList ReadClipboard()
        {
            var fileList = new PathList();
            fileList.AddRange(Clipboard.GetFileDropList()
                .Cast<string>()
                .Select(x => new Sidi.IO.LPath(x)));
            return fileList;
        }

        public static PathList Parse(string files)
        {
            if (files.Equals(":paste", StringComparison.InvariantCultureIgnoreCase))
            {
                return ReadClipboard();
            }

            return new PathList(files.Split(new[] { ";" }, StringSplitOptions.None).Select(x => new Sidi.IO.LPath(x)));
        }

        public override string ToString()
        {
            return this.Join(";");
        }
    }
}
