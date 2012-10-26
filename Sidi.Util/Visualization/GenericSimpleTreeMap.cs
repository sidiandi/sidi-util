using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;

namespace Sidi.Visualization
{
    public class GenericSimpleTreeMap<T> : SimpleTreeMap
    {
        public GenericSimpleTreeMap()
        {
            Activate = x => MessageBox.Show(x.ToString());
            ItemActivate += new ItemEventHandler(GenericSimpleTreeMap_ItemActivate);
        }

        void GenericSimpleTreeMap_ItemActivate(object sender, TreeMapControl.ItemEventEventArgs e)
        {
            Activate((T)e.Tree.Object);
        }

        new public Func<T, T> ParentSelector
        {
            set
            {
                base.ParentSelector = x => value((T)x);
            }
        }

        public Func<T, Color> Color
        {
            set
            {
                base.Color = x => value((T)x);
            }
        }

        new public Func<T, object> DistinctColor
        {
            set
            {
                base.DistinctColor = x => value((T)x);
            }
        }

        new public Func<T, float> Size
        {
            set
            {
                base.Size = x => value((T)x);
            }
        }

        new public IList<T> Items
        {
            set
            {
                base.Items = (IList)value;
            }

            get
            {
                return (IList<T>)base.Items;
            }
        }

        public Action<T> Activate;

        new public Func<T, string> Text
        {
            set
            {
                base.Text = x =>
                {
                    var t = (T)x;
                    return t == null ? String.Empty : value(t);
                };
            }
        }

        public Func<T, IComparable> PercentileColorMap
        {
            set
            {
                var pcm = new PercentileColorMap(Items.Select(value));
                Color = x => pcm.GetColor(value(x));
            }
        }
    }
}
