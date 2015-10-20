using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Treemapping
{
    public class TreeNode
    {
        public TreeNode(TreeNode parent)
        {
            this.Parent = parent;
        }

        public object Tag { get; set; }

        public double Size { get; set; }

        public RectangleD Rectangle { get; set; }

        public Color Color { get; set; }

        public string Text { get; set; }

        /// <summary>
        /// Returns the direct child node that contains point or null
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public TreeNode GetNodeAt(System.Windows.Point point)
        {
            return Nodes.FirstOrDefault(n => n.Rectangle.Contains(point));
        }

        //
        // Summary:
        //     Gets the parent tree node of the current tree node.
        //
        // Returns:
        //     A System.Windows.Forms.TreeNode that represents the parent of the current tree
        //     node.
        [Browsable(false)]
        public TreeNode Parent
        {
            get
            {
                return m_Parent;
            }

            set
            {
                if (Parent != null)
                {
                    Parent.Nodes.Remove(this);
                }

                m_Parent = value;

                if (Parent != null)
                {
                    Parent.Nodes.Add(this);
                }
            }
        }

        TreeNode m_Parent;

        public IList<TreeNode> Nodes { get { if (m_Nodes == null) { m_Nodes = new List<TreeNode>(); } return m_Nodes; } }
        IList<TreeNode> m_Nodes;

        string ToStringDescriptiveText
        {
            get
            {
                if (!String.IsNullOrEmpty(Text))
                {
                    return Text;
                }
                else if (Tag != null)
                {
                    return Tag.ToString();
                }
                else
                {
                    return base.ToString();
                }
            }
        }

        public bool IsLeaf { get { return Nodes == null || Nodes.Count == 0; } }

        public override string ToString()
        {
            return String.Format("{0}, s={1}, c={2}, b={3}", ToStringDescriptiveText, Size, Color, Rectangle);
        }
    }
}
