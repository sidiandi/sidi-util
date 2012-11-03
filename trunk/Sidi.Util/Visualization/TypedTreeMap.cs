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
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using Sidi.Extensions;

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

        new public Func<T, double> GetSize
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
                    if (x is T)
                    {
                        var t = (T)x;
                        return t == null ? String.Empty : value(t);
                    }
                    else
                    {
                        return x.ToString();
                    }
                };
            }
        }

        public Func<T, IEnumerable> GetLineage
        {
            set
            {
                base.GroupBy = x => value((T)x);
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

        public void SetPercentileColorScale(Func<T, IComparable> func, Color[] colorScale)
        {
            var pcm = new PercentileColorScale(Items.SafeSelect(func), colorScale);
            GetColor = x => pcm.GetColor(func(x));
        }
    }
}
