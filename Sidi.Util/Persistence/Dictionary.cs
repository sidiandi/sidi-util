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
