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
using System.Data.Common;

namespace Sidi.Persistence
{
    public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        class Record
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

        public Dictionary(string a_path, string a_table)
        {
            Init(a_path, a_table);
        }

        public void Close()
        {
            collection.Close();
        }

        public void Init(string a_path, string a_table)
        {
            collection = new Collection<Record>(a_path, a_table);
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
            get { throw new Exception("The method or operation is not implemented."); }
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
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue result;
                if (!TryGetValue(key, out result))
                {
                    throw new IndexOutOfRangeException();
                }
                return result;
            }
            set
            {
                this.Add(key, value);
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Clear()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool IsReadOnly
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
