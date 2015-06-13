using Sidi.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Util;

namespace Sidi.Persistence
{
    public sealed class Set<T> : IDisposable
    {
        public Set(LPath a_path, string a_table)
        {
            collection = new Collection<Record>(a_path, a_table);
        }

        Collection<Record> collection;

        class Record
        {
            public Record()
            {
            }

            public Record(byte[] a_hash)
            {
                HashValue = a_hash;
            }

            [RowId]
            public long Id = 0;

            [Data, Indexed, Unique]
            public byte[] HashValue;
        }

        // Summary:
        //     Adds the specified element to a set.
        //
        // Parameters:
        //   item:
        //     The element to add to the set.
        //
        // Returns:
        //     true if the element is added to the System.Collections.Generic.HashSet<T>
        //     object; false if the element is already present.
        public void Add(T item)
        {
            collection.Add(GetRecord(item));
        }

        Sidi.Util.IObjectHashProvider hashProvider = ObjectHashProvider.GetDefault();

        Record GetRecord(T item)
        {
            return new Record
            {
                HashValue = hashProvider.Get(item).Value.ToArray()
            };
        }

        /// <summary>
        /// Removes all elements.
        /// </summary>
        public void Clear()
        {
            collection.Clear();
        }

        /// <summary>
        /// Determines whether a Set<T> object contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in the System.Collections.Generic.HashSet<T> object.</param>
        /// <returns>true if the System.Collections.Generic.HashSet<T> object contains the specified element; otherwise, false.</returns>
        public bool Contains(T item)
        {
            var hash = GetRecord(item).HashValue;
            return collection.Find("HashValue = @hash", "hash", hash) != null;
        }

        /// <summary>
        /// Removes the specified element from a Set<T> object.
        /// </summary>
        /// <param name="item">The element to remove.</param>
        /// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if item is not found in the Set<T> object.</returns>
        public bool Remove(T item)
        {
            var deleteCommand = collection.CreateCommand("delete from @table where HashValue = @param0", GetRecord(item).HashValue);
            var rowsAffected = deleteCommand.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public void Dispose()
        {
            if (collection != null)
            {
                collection.Dispose();
                collection = null;
            }
        }

        public int Count
        {
            get
            {
                return collection.Count;
            }
        }
    }
}
