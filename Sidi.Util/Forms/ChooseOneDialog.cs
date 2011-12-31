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
