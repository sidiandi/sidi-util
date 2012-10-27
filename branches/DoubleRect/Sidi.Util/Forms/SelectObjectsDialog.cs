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
