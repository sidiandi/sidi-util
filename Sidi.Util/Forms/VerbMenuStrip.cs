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
