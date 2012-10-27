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
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace Sidi.Forms
{
    public class ColumnHandler
    {
        public virtual ListViewItem.ListViewSubItem SubItem(ListViewItem owner, object x)
        {
            return new ListViewItem.ListViewSubItem(owner, Text(x));
        }

        protected virtual string Text(object x)
        {
            return x.ToString();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public virtual ColumnHeader Header()
        {
            return new ColumnHeader()
            {
                Text = HeaderText
            };
        }

        protected virtual string HeaderText
        {
            get
            {
                return "?";
            }
        }
    }

    public class ItemDecorator
    {
        public virtual void Decorate(ListView listView, ListViewItem item, object data)
        {
        }
    }

    public class TextField : ColumnHandler
    {
        MemberInfo memberInfo;
        int width;
        
        public TextField(MemberInfo a_memberInfo, int a_width)
        {
            memberInfo = a_memberInfo;
            width = a_width;
        }

        protected override string Text(object x)
        {
            if (memberInfo is FieldInfo)
            {
                FieldInfo f = (FieldInfo)memberInfo;
                object v = f.GetValue(x);
                return Support.SafeToString(v);
            }
            return "-";
        }

        public override ColumnHeader Header()
        {
            ColumnHeader c = new ColumnHeader();
            c.Text = memberInfo.Name;
            c.Width = width;
            return c;
        }
    }

    public class ViewIList<T> : System.Windows.Forms.ListView
    {
        IList<T> data;

        public ViewIList()
        {
            this.View = View.Details;
            this.FullRowSelect = true;
            this.VirtualMode = true;
            this.DoubleBuffered = true;
        }

        public ItemDecorator ItemDecorator;

        protected override void OnRetrieveVirtualItem(RetrieveVirtualItemEventArgs e)
        {
            ListViewItem item = new ListViewItem();
            List<ColumnHandler>.Enumerator i = ColumnHandlers.GetEnumerator();
            T x = data[e.ItemIndex];
            if (i.MoveNext())
            {
                ListViewItem.ListViewSubItem s = i.Current.SubItem(item, x);
                item.Text = s.Text;
                if (ItemDecorator !=null)
                {
                    ItemDecorator.Decorate(this, item, x);
                }
            }

            while (i.MoveNext())
            {
                item.SubItems.Add(i.Current.SubItem(e.Item, data[e.ItemIndex]));
            }

            
            e.Item = item;
        }

        public void ClearSelection()
        {
        }

        public new IEnumerable<T> SelectedItems
        {
            get
            {
                foreach (int i in SelectedIndices)
                {
                    yield return Data[i];
                }
            }
        }

        public List<ColumnHandler> ColumnHandlers = new List<ColumnHandler>();

        public IList<T> Data
        {
            get { return data; }
            set
            {
                data = value;
                UpdateList();
            }
        }

        void UpdateList()
        {
            VirtualListSize = Data.Count;
        }

        public void ColumnsAllFields()
        {
            Columns.Clear();
            ColumnHandlers.Clear();
            foreach (MemberInfo m in typeof(T).GetFields())
            {
                Columns.Add(m.Name);
                ColumnHandlers.Add(new TextField(m, 300));
            }
        }

        void UpdateHeader()
        {
            Columns.Clear();
            foreach (ColumnHandler i in ColumnHandlers)
            {
                Columns.Add(i.Header());
            }
        }

        public void AddTextColumn(string name, int width)
        {
            ColumnHandlers.Add(new TextField(typeof(T).GetField(name), width));
            UpdateHeader();
        }
    }
}
