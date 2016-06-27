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

        public Color Color { get; set; }

        public string Text { get; set; }

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
                return parent;
            }

            set
            {
                if (Parent != null)
                {
                    Parent.MutableNodes.Remove(this);
                }

                parent = value;

                if (Parent != null)
                {
                    Parent.MutableNodes.Add(this);
                }
            }
        }

        TreeNode parent;

        public IReadOnlyList<TreeNode> Nodes
        {
            get
            {
                return (IReadOnlyList < TreeNode> ) MutableNodes;
            }
        }

        IList<TreeNode> MutableNodes
        {
            get
            {
                if (nodes == null)
                {
                    nodes = new List<TreeNode>();
                }
                return nodes;
            }
        }
        IList<TreeNode> nodes;

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
                    return "[no name]";
                }
            }
        }

        public bool IsLeaf { get { return Nodes == null || Nodes.Count == 0; } }

        public override string ToString()
        {
            return String.Format("{0}, s={1}, c={2}", ToStringDescriptiveText, Size, Color);
        }
    }
}
