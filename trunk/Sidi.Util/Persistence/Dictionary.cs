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
using System.Data.Common;
using System.Linq;
using System.Data.SQLite;

namespace Sidi.Persistence
{
    public sealed class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
    {
        public class Record
        {
            public Record()
            {
            }

            public Record(TKey a_key, TValue a_value)
            {
                key = a_key;
                data = a_value;
            }

            [RowId]
            public long Id =0;

            [Data, Indexed, Unique]
            public TKey key;

            [Data]
            public TValue data;
        }

        Collection<Record> collection;
        public Collection<Record> Collection
        {
            get
            {
                return collection;
            }
        }

        public Dictionary(string a_path, string a_table)
        {
            collection = new Collection<Record>(a_path, a_table);
        }

        public Dictionary(SharedConnection connection, string a_table)
        {
            collection = new Collection<Record>(connection, a_table);
        }

        public DbTransaction BeginTransaction()
        {
            return collection.BeginTransaction();
        }

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            collection.Add(new Record(key, value));
        }

        public bool ContainsKey(TKey key)
        {
            Record r = collection.Find("key = @key", "key", key);
            return r != null;
        }

        public ICollection<TKey> Keys
        {
            get { return collection.Select(r => r.key).ToList(); }
        }

        public bool Remove(TKey key)
        {
            if (ContainsKey(key))
            {
                collection.Remove(new Record(key, default(TValue)));
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            Record r = collection.Find("key = @key", "key", key);
            if (r == null)
            {
                value = default(TValue);
                return false;
            }
            value = r.data;
            return true;
        }

        public ICollection<TValue> Values
        {
            get { return collection.Select(r => r.data).ToList(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue result;
                if (!TryGetValue(key, out result))
                {
                    throw new KeyNotFoundException(key.ToString());
                }
                return result;
            }
            set
            {
                this.Add(key, value);
            }
        }

        #endregion

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this[item.Key] = item.Value;
        }

        public void Clear()
        {
            collection.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key) && item.Equals(this[item.Key]);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            var i = arrayIndex;
            foreach (var r in collection.Select("1"))
            {
                array[i++] = new KeyValuePair<TKey,TValue>(r.key, r.data);
            }
        }

        public int Count
        {
            get { return collection.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (Contains(item))
            {
                this.Remove(item.Key);
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return collection
                .Select(i => new KeyValuePair<TKey, TValue>(i.key, i.data))
                .GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return collection
                .Select(i => new KeyValuePair<TKey, TValue>(i.key, i.data))
                .GetEnumerator();
        }

        public void Dispose()
        {
            collection.Dispose();
        }
    }
}
