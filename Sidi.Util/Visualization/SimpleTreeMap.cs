using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidi.Util;
using System.Drawing;
using Sidi.Extensions;
using System.Text.RegularExpressions;

namespace Sidi.Visualization
{
    public class SimpleTreeMap : TreeMapControl
    {
        public SimpleTreeMap()
        {
            PathSeparator = new Regex(@"\\");
            Color = x => System.Drawing.Color.White;

            this.ItemMouseHover += (s,e) =>
                {
                    toolTip.SetToolTip(this,
                        e.Layout.Tree.Up.Select(x => String.Format("{0}: {1}", x.Size, x.Object)).Join());
                };
        }

        ToolTip toolTip = new ToolTip();

        public IList Items
        {
            get
            {
                return items;
            }

            set
            {
                items = value;
                UpdateTree();
            }
        }

        IList items;

        void UpdateTree()
        {
            this.Tree = BuildTree(Items, Lineage, Size);
        }

        public static Tree BuildTree(IList items, Func<object, IEnumerable<object>> lineage, Func<object, float> size)
        {
            if (items == null)
            {
                return null;
            }

            var tree = new Tree(null);
            foreach (var i in items)
            {
                var lin = lineage(i).ToArray();
                Add(tree, i, lin, size(i), 0);
            }
            tree.UpdateSize();
            for (; tree.Children.Count() == 1; tree = tree.Children.First())
            {
            }

            return tree;
        }

        static void Add(Tree tree, object item, object[] lineage, float size, int level)
        {
            if (level < lineage.Length)
            {
                var pathPart = lineage[level];
                var c = tree.Children.FirstOrDefault(i => i.Object.Equals(pathPart));
                if (c == null)
                {
                    c = new Tree(tree);
                    c.Object = pathPart;
                }
                Add(c, item, lineage, size, level + 1);
            }
            else
            {
                tree.Size += size;
            }
        }

        public Func<object, Color> Color
        {
            set
            {
                base.CushionPainter.NodeColor = value;
            }
        }
        public Func<object, float> Size = x => 1.0f;
        public Func<object, IEnumerable<object>> Lineage;
        public Func<object, object> ParentSelector
        {
            set
            {
                Lineage = x => IEnumerableExtensions
                    .Chain(x, value)
                    .Reverse();
            }
        }
        public Regex PathSeparator
        {
            get
            {
                return pathSeparator;
            }
            
            set 
            {
                pathSeparator = value;
                Lineage = x => pathSeparator.Split(x.SafeToString()).Cast<object>();
            }
        }

        public Func<object, object> DistinctColor
        {
            set
            {
                var dc = new DistinctColor();
                Color = x => dc.ToColor(value(x));
            }
        }

        Regex pathSeparator;

        /*
        static Item GetItem(Tree t)
        {
            return GetData(t).Item;
        }

        static Data GetData(Tree t)
        {
            return (Data)t.Object;
        }

        public static Tree MakeTree(IEnumerable<Item> data)
        {
            var t = new Tree(null)
            {
                Object = new Data(),
            };

            foreach (var i in data)
            {
                Add(t, i.Lineage.ToArray(), i, 0);
            }
            t.UpdateSize();

            while (t.Children.Count() == 1)
            {
                t = t.Children.First();
            }

            return t;
        }

        public TreeMapControl CreateControl()
        {
            var tree = MakeTree(Items);
            var tm = new TreeMapControl() { Tree = tree };

            tm.CushionPainter.NodeColor = t => ((Data)t).Item.Color;

            var lp = tm.CreateLabelPainter();
            
            var ti = new ToolTip();
            tm.ItemMouseHover += (s, e) =>
            {
                if (Control.ModifierKeys == Keys.Shift)
                {
                    ti.SetToolTip(tm, GetItem(e.Layout.TreeNode).Lineage.Where(i => i != null).Join(LineageSeparator).ToString());
                    lp.Focus(tm.PointToClient(Control.MousePosition));
                    tm.Invalidate();
                }
            };

            tm.Paint += (s, e) =>
                {
                    lp.Paint(e);
                };

            tm.KeyDown += (s, e) =>
            {
                if (e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D9)
                {
                    int index = e.KeyCode - Keys.D1;
                    if (e.Modifiers == Keys.Control)
                    {
                        lp.ShowLevels(index);
                    }
                    else
                    {
                        lp.ToggleLevelVisibility(index);
                    }
                    tm.Invalidate();
                }
                else if (e.KeyCode == Keys.Space)
                {
                    lp.Focus(tm.PointToClient(Control.MousePosition));
                    tm.Invalidate(); 
                }
            };

            return tm;
        }

        public IEnumerable<Item> Items { get; set; }

        public void Show()
        {
            var c = CreateControl();
            Sidi.Forms.Util.RunFullScreen(c);
        }

        public static void Show(IEnumerable<Item> items )
        {
            var st = new SimpleTreeMap();
            st.Items = items;
            st.Show();
        }
         */
    }
}
