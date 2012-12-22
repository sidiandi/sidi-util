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
using System.ComponentModel;

namespace Sidi.Forms
{
    public class VerbMenuStrip : ContextMenuStrip
    {
        object m_object;

        public VerbMenuStrip(object o)
        {
            m_object = o;
            this.Opening += new System.ComponentModel.CancelEventHandler(VerbMenuStrip_Opening);
        }

        void VerbMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CreateItems();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void CreateItems()
        {
            if (!m_itemsCreated)
            {
                this.Items.Clear();
                // use reflection to get all public void methods that do have a display name
                foreach (MethodInfo i in m_object.GetType().GetMethods())
                {
                    if (i.GetParameters().Length == 0)
                    {
                        DisplayNameAttribute displayName = (DisplayNameAttribute)
                            Attribute.GetCustomAttribute(i, typeof(DisplayNameAttribute));

                        if (displayName != null)
                        {
                            ToolStripMenuItem item = new ToolStripMenuItem();
                            item.Text = displayName.DisplayName;
                            item.Tag = i;
                            item.Click += new EventHandler(item_Click);
                            Items.Add(item);
                        }
                    }
                }
                m_itemsCreated = false;
            }
        }

        bool m_itemsCreated = false;

        void item_Click(object sender, EventArgs e)
        {
            MethodInfo mi = (MethodInfo)((ToolStripItem)sender).Tag;
            mi.Invoke(m_object, new object[] { });
        }
    }
}
