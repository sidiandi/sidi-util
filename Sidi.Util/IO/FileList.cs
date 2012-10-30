using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidi.Util;
using Sidi.Extensions;

namespace Sidi.IO
{
    public class FileList : List<Path>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FileList(IEnumerable<Path> paths)
        : base(paths)
        {
        }

        public FileList()
        {
        }

        public static FileList ReadClipboard()
        {
            var fileList = new FileList();
            fileList.AddRange(Clipboard.GetFileDropList()
                .Cast<string>()
                .Select(x => new Sidi.IO.Path(x)));
            return fileList;
        }

        public static FileList Parse(string files)
        {
            if (files.Equals(":paste", StringComparison.InvariantCultureIgnoreCase))
            {
                return ReadClipboard();
            }

            return new FileList(files.Split(new[] { ";" }, StringSplitOptions.None).Select(x => new Sidi.IO.Path(x)));
        }

        public override string ToString()
        {
            return this.Join(";");
        }
    }
}
