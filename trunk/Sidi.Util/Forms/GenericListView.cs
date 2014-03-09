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
using System.Linq;
using System.Text;
using Sidi.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace Sidi.Forms
{
    public class GenericListView : System.Windows.Forms.ListView
    {
        public new System.Collections.IList Items
        {
            get { return items; }
            set { items = value; UpdateDisplay(); }
        }
        System.Collections.IList items;
        public IList<IColumnInfo> ColumnDefinition { get { return columnDefinition; } set { columnDefinition = value; UpdateDisplay(); } }
        IList<IColumnInfo> columnDefinition;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void UpdateDisplay()
        {
            this.Columns.Clear();
            this.Columns.AddRange(ColumnDefinition.Select(x => new System.Windows.Forms.ColumnHeader()
            { 
                Text = x.Name,
                Width = -1,
            }).ToArray());
            if (Columns.Count > 0)
            {
                this.Columns[Columns.Count - 1].Width = -2;
            }
            this.VirtualListSize = Items.Count;
            this.VirtualMode = true;
            this.FullRowSelect = true;
        }

        public void SetListFormat<T>(Sidi.Util.ListFormat<T> listFormat)
        {
            this.Items = listFormat.Data.ToList();
            this.ColumnDefinition = listFormat.Columns
                .Select(x => (IColumnInfo) new ColumnInfo<T>(x.Name, x.GetText))
                .ToList();
            UpdateDisplay();
        }

        public GenericListView()
        : base()
        {
            this.AllowDrop = true;
            this.DragEnter += new System.Windows.Forms.DragEventHandler(GenericListView_DragEnter);
            this.DragOver += new DragEventHandler(GenericListView_DragOver);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(GenericListView_DragDrop);
            this.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(GenericListView_RetrieveVirtualItem);
            this.VirtualItemsSelectionRangeChanged += new System.Windows.Forms.ListViewVirtualItemsSelectionRangeChangedEventHandler(GenericListView_VirtualItemsSelectionRangeChanged);
            this.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(GenericListView_ItemSelectionChanged);
            this.View = System.Windows.Forms.View.Details;
            items = new System.Collections.ArrayList();
            columnDefinition = new IColumnInfo[] { new ColumnInfo<object>("ToString", (index, x) => x.ToString()) }.ToList();
            UpdateDisplay();
        }

        void GenericListView_DragOver(object sender, DragEventArgs e)
        {
            var lvi = GetItemAtScreen(e.X, e.Y);
            if (lvi != null)
            {
            }
        }

        ListViewItem GetItemAtScreen(int x, int y)
        {
            var cp = this.PointToClient(new Point(x, y));
            return this.GetItemAt(cp.X, cp.Y);
        }

        void GenericListView_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (DragDropOnItem != null)
            {
                var lvi = GetItemAtScreen(e.X, e.Y);
                if (lvi != null)
                {
                    var item = lvi.Tag;
                    DragDropOnItem(this, new DragDropOnItemHandlerEventArgs() { Data = e.Data, Item = item });
                }
            }
        }

        public class DragDropOnItemHandlerEventArgs : EventArgs
        {
            public IDataObject Data;
            public object Item;
        }

        public event EventHandler<DragDropOnItemHandlerEventArgs> DragDropOnItem;

        void GenericListView_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            e.Effect = System.Windows.Forms.DragDropEffects.Copy;
        }

        void GenericListView_ItemSelectionChanged(object sender, System.Windows.Forms.ListViewItemSelectionChangedEventArgs e)
        {
            var interval = new Sidi.Util.Interval(e.ItemIndex, e.ItemIndex+1);
            if (e.IsSelected)
            {
                selected.Add(interval);
            }
            else
            {
                selected.Remove(interval);
            }
        }

        IntSet selected = new IntSet();

        public IEnumerable<object> SelectedObjects
        {
            get
            {
                foreach (var i in selected)
                {
                    yield return Items[i];
                }
            }
        }

        void GenericListView_VirtualItemsSelectionRangeChanged(object sender, System.Windows.Forms.ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            var interval = new Sidi.Util.Interval(e.StartIndex, e.EndIndex+1);
            if (e.IsSelected)
            {
                selected.Add(interval);
            }
            else
            {
                selected.Remove(interval);
            }
        }

        void GenericListView_RetrieveVirtualItem(object sender, System.Windows.Forms.RetrieveVirtualItemEventArgs e)
        {
            var item  = Items[e.ItemIndex];
            var texts = ColumnDefinition.Select(c => c.Value(item, e.ItemIndex));
            e.Item = new System.Windows.Forms.ListViewItem(texts.First());
            e.Item.Tag = item;
            e.Item.SubItems.AddRange(texts.Skip(1).ToArray());
        }
    }
}
