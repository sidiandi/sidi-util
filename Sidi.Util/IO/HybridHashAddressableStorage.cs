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
    public class HybridHashAddressableStorage : IHashAddressableStorage, IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        LPath directory;
        HashAddressableStorage fileStorage;
        Sidi.Persistence.Collection<Entry> entries;
        SQLiteTransaction transaction = null;
        int writes = 0;
        public int FlushAfterNWrites { get; set; }
        public long MaxInternalBlobSize { get; set; }

        public HybridHashAddressableStorage(LPath directory)
        {
            this.directory = directory;
            this.directory.CatDir("import").EnsureDirectoryExists();
            var databaseFile = directory.CatDir("store.sqlite");
            fileStorage = new HashAddressableStorage(directory.CatDir("content"));
            entries = new Collection<Entry>(databaseFile);
            FlushAfterNWrites = 1;
            MaxInternalBlobSize = 100 * 1024;
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
                }
                if (FlushAfterNWrites > 1)
                {
                    transaction = entries.BeginTransaction();
                }
                writes = 0;
            }
        }

        class Entry
        {
            [RowId]
            public long Id;

            [Data, Indexed, Unique]
            public byte[] Key;

            [Data]
            public DateTime LastWriteTimeUtc;

            [Data]
            public byte[] Value;
        }

        class WriteStream : Stream
        {
            HybridHashAddressableStorage storage;
            Hash key;
            LPath tempFile;
            Stream stream;

            static LPath GetTempFile(HybridHashAddressableStorage storage, Hash key)
            {
                return storage.directory.CatDir("import").CatDir(key.Value.HexString());
            }
            
            public WriteStream(HybridHashAddressableStorage storage, Hash key)
            {
                this.tempFile = GetTempFile(storage, key);
                this.storage = storage;
                this.key = key;
                this.stream = new MemoryStream();
            }

            public override void Close()
            {
                base.Close();
                if (storage != null)
                {
                    if (tempFile == null)
                    {
                        stream.Close();
                        storage.MoveInto(key, tempFile);
                    }
                    else
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        storage.DoWrite(key, stream);
                        stream.Close();
                        stream = null;
                    }

                    storage = null;
                }
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override void Flush()
            {
                stream.Flush();
            }

            public override long Length
            {
                get { throw new NotImplementedException(); }
            }

            public override long Position
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                stream.Write(buffer, offset, count);
                if (tempFile == null)
                {
                    if (stream.Position > storage.MaxInternalBlobSize)
                    {
                        tempFile = GetTempFile(storage, key);
                        var fileStream = tempFile.OpenWrite();
                        var memoryStream = (MemoryStream)stream;
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.CopyTo(fileStream);
                        stream = fileStream;
                    }
                }
            }
        }

        void MoveInto(Hash key, LPath tempFile)
        {
            Remove(key);

            var e = new Entry { Key = key.Value.ToArray(), LastWriteTimeUtc = DateTime.UtcNow, Value = null };
            fileStorage.Write(key, tempFile);
            tempFile.EnsureFileNotExists();

            entries.Add(e);
            Flush();
        }

        void Write(Hash key, LPath tempFile)
        {
            Remove(key);

            var e = new Entry { Key = key.Value.ToArray(), LastWriteTimeUtc = DateTime.UtcNow };
            var length = tempFile.Info.Length;
            if (length > MaxInternalBlobSize)
            {
                e.Value = null;
                fileStorage.Write(key, tempFile);
            }
            else
            {
                using (var r = tempFile.OpenRead())
                {
                    e.Value = new byte[length];
                    r.Read(e.Value, 0, (int) length);
                }
            }
            tempFile.EnsureFileNotExists();

            entries.Add(e);
            Flush();
        }

        void DoWrite(Hash key, Stream reader)
        {
            Remove(key);

            var e = new Entry { Key = key.Value.ToArray(), LastWriteTimeUtc = DateTime.UtcNow };
            if (reader.Length > MaxInternalBlobSize)
            {
                e.Value = null;
                using (var w = fileStorage.Write(key))
                {
                    reader.CopyTo(w);
                }
            }
            else
            {
                e.Value = new byte[reader.Length];
                reader.Read(e.Value, 0, (int)reader.Length);
            }

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
            if (e.Value == null)
            {
                return fileStorage.Read(key);
            }
            else
            {
                return new MemoryStream(e.Value);
            }
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
                if (e.Value == null)
                {
                    fileStorage.Remove(key);
                }
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
            fileStorage.Clear();
        }

        public void Dispose()
        {
            DisposeHelper.Dispose(fileStorage);

            if (transaction != null)
            {
                transaction.Commit();
                transaction.Dispose();
                transaction = null;
            }

            DisposeHelper.Dispose(ref entries);
        }
    }
}
