// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.Text;
using System.Windows.Forms;

namespace Sidi.Collections
{
    public class CacheAutoInvalidate<Key, Value>
    {
        LruCacheBackground<Key, Value> cache;
        List<WeakReference> invalidate = new List<WeakReference>();

        public CacheAutoInvalidate(LruCacheBackground<Key, Value> a_cache)
        {
            cache = a_cache;
            cache.EntryUpdated += new EventHandler<LruCacheBackground<Key, Value>.EntryUpdatedEventArgs>(cache_EntryUpdated);
        }

        void cache_EntryUpdated(object sender, LruCacheBackground<Key, Value>.EntryUpdatedEventArgs arg)
        {
            foreach (WeakReference i in invalidate)
            {
                Control control = (Control) i.Target;
                System.Diagnostics.Trace.WriteLine("inva: " + control.ToString() + " " + arg.key.ToString());
                control.Invalidate();
            }
            invalidate.Clear();
        }

        public Value Get(Key key, Control control)
        {
            Value v = cache[key];
            if (control != null && v == null)
            {
                WeakReference wr = new WeakReference(control);
                System.Diagnostics.Trace.WriteLine("miss: " + control.ToString() + " " + key.ToString());
                invalidate.Add(wr);
            }
            return v;
        }
    }
}
