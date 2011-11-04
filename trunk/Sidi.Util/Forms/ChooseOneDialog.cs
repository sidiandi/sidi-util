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
    public partial class ChooseOneDialog : System.Windows.Forms.Form
    {
        public ChooseOneDialog()
        {
            InitializeComponent();
            Columns = new IColumnInfo[] { new ColumnInfo<object>("Object", x => Sidi.Forms.Support.SafeToString(x)) };
        }

        public object SelectedObject
        {
            get
            {
                return listViewObjects.SelectedItems.Cast<ListViewItem>().First().Tag;
            }
        }

        public IEnumerable<IColumnInfo> Columns { set; get; }

        public IEnumerable<object> Objects
        {
            set
            {
                if (value.Any())
                {
                    this.Text = "Choose one {0}".F(value.First().GetType());
                }

                listViewObjects.Items.Clear();
                listViewObjects.Columns.Clear();
                listViewObjects.HeaderStyle = ColumnHeaderStyle.Clickable;
                
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
