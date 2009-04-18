using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Sidi.Util
{
    public partial class UpdateForm : Form
    {
        UpdateCheck updateCheck;

        public UpdateForm(UpdateCheck updateCheck)
        {
            this.updateCheck = updateCheck;

            InitializeComponent();

            labelInfo.Text = String.Format(labelInfo.Text,
                updateCheck.InstalledVersion,
                updateCheck.AvailableVersion,
                updateCheck.Assembly.GetName().Name,
                updateCheck.Message);
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = updateCheck.DownloadUrl;
            p.StartInfo.UseShellExecute = true;
            p.Start();
        }
    }
}
