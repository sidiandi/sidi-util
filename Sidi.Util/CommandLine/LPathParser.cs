using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;
using System.Windows.Forms;
using Sidi.Util;
using Sidi.Extensions;

namespace Sidi.CommandLine
{
    [Usage("File system path")]
    [Example(@"C:\temp\hello.txt")]
    [Example(@":current", NoTest = true)]
    [Example(@":selected", NoTest = true)]
    [Example(@":paste", NoTest = true)]
    public class LPathParser : ValueContainer<LPath>, CommandLineHandler2
    {
        [Usage("Prompt for file")]
        public void Ask()
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Value = dlg.FileName;
            }
            else
            {
                throw new Exception("Canceled by user");
            }
        }

        [Usage("Use directory currently open in Windows Explorer")]
        public void Current()
        {
            Value = new Shell().GetOpenDirectory();
        }

        [Usage("Use file that is currently selected in Windows Explorer")]
        public void Selected()
        {
            Value = new Shell().SelectedFiles.First();
        }

        [Usage("Use clipboard content")]
        public void Paste()
        {
        }

        public void BeforeParse(IList<string> args, Parser p)
        {
            p.Prefix[typeof(CommandLine.Action)] = new[] { ":" };
        }

        public void UnknownArgument(IList<string> args, Parser p)
        {
            Value = LPath.Parse(args.PopHead());
        }
    }
}
