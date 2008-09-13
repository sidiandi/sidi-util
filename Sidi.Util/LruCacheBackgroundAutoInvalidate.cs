// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
            cache.EntryUpdated += new LruCacheBackground<Key, Value>.EntryUpdatedHandler(cache_EntryUpdated);
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
