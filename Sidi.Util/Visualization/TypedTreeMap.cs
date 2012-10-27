using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;

namespace Sidi.Visualization
{
    public class TypedTreeMap<T> : SimpleTreeMap
    {
        public TypedTreeMap()
        {
            Activate = x => MessageBox.Show(x.ToString());
            ItemActivate += new EventHandler<TreeEventArgs>(GenericSimpleTreeMap_ItemActivate);
        }

        void GenericSimpleTreeMap_ItemActivate(object sender, TreeEventArgs e)
        {
            Activate((T)e.Tree.Object);
        }

        new public Func<T, T> GetParent
        {
            set
            {
                base.GetParent = x => value((T)x);
            }
        }

        new public Func<T, Color> GetColor
        {
            set
            {
                base.GetColor = x => value((T)x);
            }
        }

        new public Func<T, object> GetDistinctColor
        {
            set
            {
                base.GetDistinctColor = x => value((T)x);
            }
        }

        new public Func<T, float> GetSize
        {
            set
            {
                base.GetSize = x => value((T)x);
            }
        }

        new public Func<T, string> GetText
        {
            set
            {
                base.GetText = x =>
                {
                    var t = (T)x;
                    return t == null ? String.Empty : value(t);
                };
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

        public Func<T, IComparable> PercentileColorMap
        {
            set
            {
                var pcm = new PercentileColorMap(Items.Select(value));
                GetColor = x => pcm.GetColor(value(x));
            }
        }
    }
}
