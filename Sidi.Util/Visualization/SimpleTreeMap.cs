﻿using System;
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

            LabelPainter = new LabelPainter(this);
            LabelPainter.HotkeysEnabled = true;
            LabelPainter.InteractMode = Visualization.LabelPainter.Mode.MouseFocus;
            LabelPainter.Text = x => Text(x.Object);

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

        public LabelPainter LabelPainter;

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
        public Func<object, IEnumerable<object>> Lineage { set; private get; }
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
        public Func<object, string> Text { set; private get; }

        public Func<object, object> DistinctColor
        {
            set
            {
                var dc = new DistinctColor();
                Color = x => dc.ToColor(value(x));
            }
        }

        Regex pathSeparator;
    }
}
