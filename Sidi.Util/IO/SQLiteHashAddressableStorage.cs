using Sidi.IO;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.Persistence;
using System.Data.SQLite;

namespace Sidi.IO
{
    public class SQLiteHashAddressableStorage : IHashAddressableStorage, IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SQLiteHashAddressableStorage(LPath databaseFile)
        {
            entries = new Collection<Entry>(databaseFile);
            transaction = entries.BeginTransaction();
        }

        void Flush()
        {
            ++writes;
            if (writes >= FlushAfterNWrites)
            {
                if (transaction != null)
                {
                    transaction.Commit();
                    transaction.Dispose();
                    transaction = entries.BeginTransaction();
                    writes = 0;
                }
            }
        }

        Sidi.Persistence.Collection<Entry> entries;
        SQLiteTransaction transaction = null;
        int writes = 0;
        int FlushAfterNWrites = 100;

        class Entry
        {
            [RowId]
            public long Id = 0;

            [Data, Indexed, Unique]
            public byte[] Key;

            [Data]
            public DateTime LastWriteTimeUtc;

            [Data]
            public byte[] Value;
        }

        class WriteStream : MemoryStream
        {
            SQLiteHashAddressableStorage storage;
            Hash key;

            public WriteStream(SQLiteHashAddressableStorage storage, Hash key)
            {
                this.storage = storage;
                this.key = key;
            }

            public override void Close()
            {
                base.Close();
                if (storage != null)
                {
                    storage.Write(key, this.ToArray());
                    storage = null;
                }
            }
        }

        void Write(Hash key, byte[] value)
        {
            var e = new Entry { Key = key.Value.ToArray(), LastWriteTimeUtc = DateTime.UtcNow, Value = value };
            Remove(key);
            entries.Add(e);
            Flush();
        }

        public Stream Write(Hash key)
        {
            return new WriteStream(this, key);
        }

        public Stream Read(Hash key)
        {
            var e = Find(key);
            if (e == null)
            {
                throw new ArgumentOutOfRangeException("key", key, "not found");
            }
            return new MemoryStream(e.Value);
        }

        public bool ContainsKey(Hash key)
        {
            return Find(key) != null;
        }

        Entry Find(Hash key)
        {
            return entries.Find("Key = @key", "key", key.Value.ToArray());
        }

        public bool Remove(Hash key)
        {
            var e = Find(key);
            if (e == null)
            {
                return false;
            }
            else
            {
                entries.Remove(e);
                return true;
            }
        }

        public bool TryGetInfo(Hash key, out StorageItemInfo info)
        {
            var e = Find(key);
            if (e == null)
            {
                info = null;
                return false;
            }
            else
            {
                info = new StorageItemInfo(e.Value.Length, e.LastWriteTimeUtc);
                return true;
            }
        }

        public void Clear()
        {
            entries.Clear();
        }

        public void Dispose()
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction.Dispose();
                transaction = null;
            }

            if (entries != null)
            {
                entries.Dispose();
                entries = null;
            }
        }
    }
}
