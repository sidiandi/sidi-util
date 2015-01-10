using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace Sidi.Forms
{
    public partial class ControlTree : UserControl
    {
        public ControlTree()
        {
            InitializeComponent();

            treeView.AfterSelect += (s, e) =>
            {
                GotoPage((Control)treeView.SelectedNode.Tag);
            };

        }

        IEnumerable<TreeNode> Recurse(TreeNodeCollection nodes)
        {
            foreach (var i in nodes.Cast<TreeNode>())
            {
                yield return i;
                foreach (var c in Recurse(i.Nodes))
                {
                    yield return c;
                }
            }
        }

        public void AddPage(Control page, Control parent)
        {
            page.Dock = DockStyle.Fill;
            this.splitContainer1.Panel2.Controls.Add(page);
            var parentNode = Recurse(treeView.Nodes).FirstOrDefault(x => x.Tag == parent);
            var node = new TreeNode()
            {
                Text = page.Text,
                Tag = page,
            };
            if (parentNode == null)
            {
                treeView.Nodes.Add(node);
            }
            else
            {
                parentNode.Nodes.Add(node);
            }
            page.Hide();
        }

        Control visiblePage = null;

        void GotoPage(Control page)
        {
            if (visiblePage != null)
            {
                visiblePage.Hide();
            }

            visiblePage = page;

            visiblePage.Show();
        }
    }
}
