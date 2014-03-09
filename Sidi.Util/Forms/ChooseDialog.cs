// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidi.Util;

namespace Sidi.Forms
{
    public partial class ChooseDialog : System.Windows.Forms.Form
    {
        public ChooseDialog()
        {
            InitializeComponent();
            Columns = new IColumnInfo[] { new ColumnInfo<object>("Object", (index, x) => Sidi.Forms.Support.SafeToString(x)) };
        }

        public object SelectedObject
        {
            get
            {
                return listViewObjects.SelectedObjects.First();
            }
        }

        public IEnumerable<IColumnInfo> Columns
        {
            set
            {
                this.listViewObjects.ColumnDefinition = value.ToList();
                listViewObjects.UpdateDisplay();
            }
            get
            {
                return listViewObjects.ColumnDefinition;
            }
        }

        public IEnumerable<object> Objects
        {
            set
            {
                listViewObjects.Items = value.ToList();
            }

            get
            {
                return listViewObjects.Items.Cast<object>();
            }
        }

        public event EventHandler ObjectSelected;

        private void listViewObjects_ItemActivate(object sender, EventArgs e)
        {
            if (ObjectSelected != null)
            {
                ObjectSelected(this, EventArgs.Empty);
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
