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
using System.Diagnostics.CodeAnalysis;

namespace Sidi.Visualization
{
    public class SimpleTreeMap : TreeMap
    {
        public SimpleTreeMap()
        {
            PathSeparator = @"\";
            GetColor = x => System.Drawing.Color.White;
            GetText = x => x.ToString();
            GetSize = x => 1.0f;

            LabelPainter = new LabelPainter(this);
            LabelPainter.HotkeysEnabled = true;
            LabelPainter.InteractMode = Visualization.InteractionMode.MouseFocus;
            LabelPainter.Text = x => GetText(x.Object);

            this.ItemMouseHover += (s,e) =>
                {
                    toolTip.SetToolTip(this,
                        e.Layout.Tree.Up.Select(x => String.Format("{0}: {1}", x.Size, GetText(x.Object))).Join());
                };
        }

        ToolTip toolTip = new ToolTip();

        protected override void OnPaint(PaintEventArgs e)
        {
            if (treeUpdateRequired)
            {
                this.Tree = BuildTree(Items, GroupBy, GetSize);
                treeUpdateRequired = false;
            }

            base.OnPaint(e);
        }

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
        bool treeUpdateRequired = true;

        void UpdateTree()
        {
            treeUpdateRequired = true;
            Invalidate();
        }

        public LabelPainter LabelPainter;

        [SuppressMessage("Microsoft.Design", "CA1006")]
        public static Tree BuildTree(IList items, Func<object, IEnumerable> lineage, Func<object, double> size)
        {
            if (items == null)
            {
                return null;
            }

            var tree = new Tree(null);
            foreach (var i in items)
            {
                var lin = lineage(i).Cast<object>().ToArray();
                Add(tree, i, lin, size(i), 0);
            }
            tree.UpdateSize();
            for (; tree.Children.Count() == 1; tree = tree.Children.First())
            {
            }

            return tree;
        }

        static void Add(Tree tree, object item, object[] lineage, double size, int level)
        {
            if (level < lineage.Length - 1)
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
                var pathPart = item;
                var c = tree.Children.FirstOrDefault(i => i.Object.Equals(pathPart));
                if (c == null)
                {
                    c = new Tree(tree);
                    c.Object = pathPart;
                }
                c.Size += size;
            }
        }

        public Func<object, Color> GetColor
        {
            set
            {
                base.CushionPainter.GetColor = x => { try { return value(x); } catch { return Color.Red; } };
                UpdateTree();
            }

            get
            {
                return base.CushionPainter.GetColor;
            }
        }

        public Func<object, double> GetSize
        {
            set
            {
                myGetSize = x => { try { return value(x); } catch { return 0; } };
                UpdateTree();
            }
        
            get
            {
                return myGetSize;
            }
        }
        Func<object, double> myGetSize;

        public Func<object, IEnumerable> GroupBy
        {
            set
            {
                myGroupBy = x => { try { return value(x); } catch { return new System.Collections.ArrayList(); } };
                UpdateTree();
            }

            get
            {
                return myGroupBy;
            }
        }
        
        Func<object, IEnumerable> myGroupBy;

        public Func<object, object> GetParent
        {
            set
            {
                GroupBy = x => IEnumerableExtensions
                    .Chain(x, value)
                    .Reverse();
            }
        }

        public string PathSeparator
        {
            get
            {
                return myPathSeparator;
            }
            
            set 
            {
                myPathSeparator = value;
                GroupBy = x => x.SafeToString()
                    .Split(new string[] { myPathSeparator }, StringSplitOptions.RemoveEmptyEntries)
                    .JoinSelect(myPathSeparator)
                    .Cast<object>();
            }
        }

        string myPathSeparator;

        Dictionary<Type, Func<object, string>> textMapper = new Dictionary<Type,Func<object,string>>();
        
        public Func<object, string> GetText
        {
            set
            {
                myGetText = x => { try { return value(x); } catch { return String.Empty; } };
                Invalidate();
            }

            get
            {
                return myGetText;
            }
        }
        Func<object, string> myGetText;

        public Func<object, object> GetDistinctColor
        {
            set
            {
                var dc = new DistinctColor();
                GetColor = x => dc.ToColor(value(x));
            }
        }
    }
}
