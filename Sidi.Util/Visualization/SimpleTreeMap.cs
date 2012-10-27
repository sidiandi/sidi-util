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

            LabelPainter = new LabelPainter(this);
            LabelPainter.HotkeysEnabled = true;
            LabelPainter.InteractMode = Visualization.InteractionMode.MouseFocus;
            LabelPainter.Text = x => GetText(x.Object);

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
            this.Tree = BuildTree(Items, GetLineage, GetSize);
        }

        public LabelPainter LabelPainter;

        [SuppressMessage("Microsoft.Design", "CA1006")]
        public static Tree BuildTree(IList items, Func<object, IEnumerable> lineage, Func<object, float> size)
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

        public Func<object, Color> GetColor
        {
            set
            {
                base.CushionPainter.NodeColor = value;
            }
        }
        public Func<object, float> GetSize = x => 1.0f;
        public Func<object, IEnumerable> GetLineage { set; private get; }
        public Func<object, object> GetParent
        {
            set
            {
                GetLineage = x => IEnumerableExtensions
                    .Chain(x, value)
                    .Reverse();
            }
        }
        public string PathSeparator
        {
            get
            {
                return pathSeparator;
            }
            
            set 
            {
                pathSeparator = value;
                GetLineage = x => x.SafeToString()
                    .Split(new string[] { pathSeparator }, StringSplitOptions.RemoveEmptyEntries)
                    .JoinSelect(pathSeparator)
                    .Cast<object>();
            }
        }
        public Func<object, string> GetText { set; private get; }

        public Func<object, object> GetDistinctColor
        {
            set
            {
                var dc = new DistinctColor();
                GetColor = x => dc.ToColor(value(x));
            }
        }

        string pathSeparator;
    }
}
