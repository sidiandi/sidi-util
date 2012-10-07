﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidi.Util;
using System.Drawing;
using Sidi.Extensions;

namespace Sidi.Visualization
{
    public class SimpleTreeMap
    {
        public class ItemBase
        {
            public IEnumerable<object> Lineage;
            public float Size;
        }

        public class Item : ItemBase
        {
            public Color Color;
        }

        static void Add(Tree<Data> tree, object[] lineage, Item data, int level)
        {
            if (level < lineage.Length)
            {
                var value = lineage[level];
                var c = tree.Children.FirstOrDefault(i => i.Data.Value.Equals(value));
                if (c == null)
                {
                    c = new Tree<Data>(tree);
                    tree.Children.Add(c);
                    c.Data = new Data() { Value = value, Item = data };
                }
                Add(c, lineage, data, level + 1);
            }
            else
            {
                tree.Size += data.Size;
            }
        }

        public class Data
        {
            public object Value;
            public Item Item;
            public override string ToString()
            {
                return Value.SafeToString();
            }
        }

        public class ColorMapItem : ItemBase
        {
            public object Color;
        }

        public static IEnumerable<Item> ToItems(IEnumerable<ColorMapItem> data)
        {
            return null;
        }

        public class LinearColorMapItem : ItemBase
        {
            public double Color;
        }

        public static IEnumerable<Item> ToItems(IEnumerable<LinearColorMapItem> data)
        {
            var min = Double.MaxValue;
            var max = Double.MinValue;
            foreach (var i in data)
            {
                min = Math.Min(i.Color, min);
                max = Math.Max(i.Color, max);
            }
            var scale = ColorMap.BlueRed(min, max);
            return data.Select(i => new Item() { Lineage = i.Lineage, Size = i.Size, Color = scale.ToColor(i.Color) });
        }

        public static Tree<Data> MakeTree(IEnumerable<Item> data)
        {
            var t = new Tree<Data>(null);
            t.Data = new Data();
            foreach (var i in data)
            {
                Add(t, i.Lineage.ToArray(), i, 0);
            }
            t.UpdateSize();

            while (t.Children.Count == 1)
            {
                t = t.Children.First();
            }

            return t;
        }

        public string LineageSeparator = "/";
        
        public TreeMapControl<Tree<Data>> CreateControl()
        {
            var tree = MakeTree(Items);
            var tm = tree.CreateTreemapControl();

            tm.CushionPainter.NodeColor = t => t.Data.Item.Color;

            var lp = tm.CreateLabelPainter();
            
            var ti = new ToolTip();
            tm.ItemMouseHover += (s, e) =>
            {
                if (Control.ModifierKeys == Keys.Shift)
                {
                    ti.SetToolTip(tm, e.Item.Lineage.Where(i => i != null).Join(LineageSeparator).ToString());
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

            tm.MouseWheel += (s, e) =>
                {
                    if (e.Delta > 0)
                    {
                        tm.ZoomIn(e.Location, 1);
                    }
                    else if (e.Delta < 0)
                    {
                        tm.ZoomOut(1);
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
    }
}
