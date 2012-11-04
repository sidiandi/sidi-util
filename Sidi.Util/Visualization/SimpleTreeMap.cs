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
    class LookupTree : Tree
    {
        public LookupTree(LookupTree parent)
        : base(parent)
        {
        }

        Dictionary<object, LookupTree> lookup = new Dictionary<object, LookupTree>();

        public void Add(object item, object[] lineage, double size, int level)
        {
            if (level < lineage.Length - 1)
            {
                var pathPart = lineage[level];
                ProvideChild(pathPart).Add(item, lineage, size, level + 1);
            }
            else
            {
                var pathPart = item;
                var c = ProvideChild(pathPart);
                c.Size += size;
            }
        }

        LookupTree ProvideChild(object pathPart)
        {
            LookupTree c;
            if (!lookup.TryGetValue(pathPart, out c))
            {
                c = new LookupTree(this);
                c.Object = pathPart;
                lookup[pathPart] = c;
            }
            return c;
        }
    }

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

            /*
            this.ItemMouseHover += (s,e) =>
                {
                    toolTip.SetToolTip(this,
                        e.Layout.Tree.Up.Select(x => String.Format("{0}: {1}", x.Size, GetText(x.Object))).Join());
                };
             */
        }

        ToolTip toolTip = new ToolTip();

        protected override void OnPaint(PaintEventArgs e)
        {
            if (treeUpdateRequired)
            {
                var tree = BuildTree(Items, GroupBy, GetSize);
                tree.Sort(x => x.ToString());
                this.Tree = tree;
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

            var tree = new LookupTree(null);
            foreach (var i in items)
            {
                var lin = lineage(i).Cast<object>().ToArray();
                tree.Add(i, lin, size(i), 0);
            }
            tree.UpdateSize();
            for (; tree.Children.Count() == 1; tree = (LookupTree) tree.Children.First())
            {
            }

            return tree;
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
