using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidi.Util;

namespace Sidi.IO.Long
{
    public class FileList : List<Path>
    {
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
                .Select(x => new Sidi.IO.Long.Path(x)));
            return fileList;
        }

        public static FileList Parse(string files)
        {
            if (files.Equals(":paste", StringComparison.InvariantCultureIgnoreCase))
            {
                return ReadClipboard();
            }

            return new FileList(files.Split(new[] { ";" }, StringSplitOptions.None).Select(x => new Sidi.IO.Long.Path(x)));
        }

        public override string ToString()
        {
            return this.Join(";");
        }
    }
}
