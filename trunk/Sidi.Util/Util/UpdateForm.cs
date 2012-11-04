// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = updateCheck.DownloadUrl;
            p.StartInfo.UseShellExecute = true;
            p.Start();
        }
    }
}
