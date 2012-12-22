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

namespace Sidi.Forms
{
    public partial class SelectObjectsDialog : Form
    {
        public SelectObjectsDialog()
        {
            InitializeComponent();
            Columns = new IColumnInfo[] { new ColumnInfo<object>("Object", x => Sidi.Forms.Support.SafeToString(x)) };
        }

        public void SelectAll(bool selected)
        {
            foreach (ListViewItem i in listViewObjects.Items)
            {
                i.Selected = selected;
            }
        }

        public IEnumerable<IColumnInfo> Columns { set; get; }

        public IEnumerable<object> Objects
        {
            set
            {
                listViewObjects .Items.Clear();

                foreach (var c in Columns)
                {
                    listViewObjects.Columns.Add(c.Name, -1);
                }
                if (listViewObjects.Columns.Count > 0)
                {
                    listViewObjects.Columns[listViewObjects.Columns.Count - 1].Width = -2;
                }
                foreach (object i in value)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = Columns.First().Value(i);
                    item.Tag = i;
                    item.Selected = true;
                    foreach (var c in Columns.Skip(1))
                    {
                        item.SubItems.Add(c.Value(i));
                    }
                    listViewObjects.Items.Add(item);
                }
            }

            get
            {
                List<object> objects = new List<object>();
                foreach (ListViewItem i in listViewObjects.Items)
                {
                    objects.Add(i.Tag);
                }
                return objects;
            }
        }

        public IEnumerable<object> SelectedObjects
        {
            get
            {
                return listViewObjects.Items
                    .Cast<ListViewItem>()
                    .Where(x => x.Selected)
                    .Select(x => x.Tag);
            }

            set
            {
                HashSet<object> o = new HashSet<object>(value);
                foreach (ListViewItem i in listViewObjects.Items)
                {
                    i.Selected = o.Contains(i.Tag);
                }
            }
        }

    }
}
