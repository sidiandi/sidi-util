// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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

        public virtual ColumnHeader Header()
        {
            ColumnHeader c = new ColumnHeader();
            c.Text = HeaderText;
            return c;
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
