using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;
using Sidi.Util;
using System.Windows.Forms;

namespace Sidi.CommandLine
{
    [Usage("File system path list")]
    [Example(@"C:\temp\somefile.txt")]
    [Example(@"C:\temp\somefile.txt;C:\temp\someotherfile.txt")]
    [Example(@"(C:\temp\somefile.txt C:\temp\someotherfile.txt)")]
    [Example(@":current", NoTest=true)]
    [Example(@":selected", NoTest=true)]
    [Example(@":paste", NoTest = true)]
    public class PathListParser : ValueContainer<PathList>, CommandLineHandler2
    {
        [Usage("Prompt for files")]
        public void Ask()
        {
            var dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Value = new PathList(dlg.FileNames.Select(x => new LPath(x)));
            }
            else
            {
                throw new Exception("Canceled by user");
            }
        }

        [Usage("Use file that is currently selected in Windows Explorer")]
        public void Selected()
        {
            Value = new PathList(new Shell().SelectedFiles);
        }

        [Usage("Use clipboard content")]
        public void Paste()
        {
            Value = PathList.ReadClipboard();
        }

        public void BeforeParse(IList<string> args, Parser p)
        {
            p.Prefix[typeof(CommandLine.Action)] = new[] { ":" };
        }

        public void UnknownArgument(IList<string> args, Parser p)
        {
            if (Value == null)
            {
                Value = new PathList();
            }
            Value.Add(p.ParseValue<LPath>(args));
        }
    }
}
