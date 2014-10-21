using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidi.IO;
using Sidi.Extensions;
using Sidi.Util;
using System.Text.RegularExpressions;

namespace Sidi.Tool
{
    public partial class RenameDialog : Form
    {
        public RenameDialog()
        {
            InitializeComponent();
        }

        public IList<LPath> Files;
        public IList<RenameOperation> RenameOperations;

        IList<RenameOperation> RenameWithRegex(IList<LPath> files, Regex regex, string replace)
        {
            return Files.Select(f =>
                {
                    try
                    {
                    return new RenameOperation()
                    {
                        From = f,
                        To = new LPath(regex.Replace(f.ToString(), replace))
                    };
                    }
                    catch
                    {
                        return new RenameOperation() { From = f, To = f };
                    }
                })
                .ToList();
        }

        public void Preview()
        {
            Regex pattern;
            try
            {
                pattern = new Regex(textBoxPattern.Text);
            }
            catch
            {
                pattern = new Regex("");
            }
            var replace = textBoxRename.Text;
            
            RenameOperations = RenameWithRegex(Files, pattern, replace);

            var previewText = RenameOperations.Select(x => String.Format(
@"{0}
{1}
", x.From, x.To)).Join();

            textBoxPreview.Text = previewText;
        }

        private void textBoxPattern_TextChanged(object sender, EventArgs e)
        {
            Preview();
        }

        private void textBoxRename_TextChanged(object sender, EventArgs e)
        {
            Preview();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Preview();
            RenameOperation.Rename(RenameOperations);
        }
    }
}
