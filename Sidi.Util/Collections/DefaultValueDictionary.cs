using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Collections
{
    /// <summary>
    /// A dictionary that calculates a default value on lookup if the key is not found.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DefaultValueDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        readonly IDictionary<TKey, TValue> backingStore;
        readonly Func<TKey, TValue> provideDefaultValue;

        public bool StoreDefaults { get; set; }

        public DefaultValueDictionary(Func<TKey, TValue> provideDefaultValue = null, IDictionary<TKey, TValue> backingStore = null)
        {
            if (provideDefaultValue == null)
            {
                provideDefaultValue = _ => default(TValue);
            }
            this.provideDefaultValue = provideDefaultValue;

            if (backingStore == null)
            {
                backingStore = new Dictionary<TKey, TValue>();
            }
            this.backingStore = backingStore;
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue v;
                if (!backingStore.TryGetValue(key, out v))
                {
                    v = provideDefaultValue(key);
                    if (StoreDefaults)
                    {
                        backingStore[key] = v;
                    }
                }
                return v;
            }

            set
            {
                backingStore[key] = value;
            }
        }

        public int Count
        {
            get
            {
                return backingStore.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return backingStore.IsReadOnly;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return backingStore.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return backingStore.Values;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            backingStore.Add(item);
        }

        public void Add(TKey key, TValue value)
        {
            backingStore.Add(key, value);
        }

        public void Clear()
        {
            backingStore.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return backingStore.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return backingStore.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            backingStore.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return backingStore.GetEnumerator();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return backingStore.Remove(item);
        }

        public bool Remove(TKey key)
        {
            return backingStore.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = this[key];
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
