using Sidi.Util;
using System;
using System.IO;

namespace Sidi.IO
{
    [Serializable]
    public class StorageItemInfo
    {
        public StorageItemInfo(long length, DateTime lastWriteTimeUtc)
        {
            this.Length = length;
            this.LastWriteTimeUtc = lastWriteTimeUtc;
        }

        public long Length { get; private set; }
        public DateTime LastWriteTimeUtc { get; private set; }
    }

    public interface IHashAddressableStorage
    {
        Stream Write(Hash key);
        Stream Read(Hash key);

        bool Contains(Hash hash);
        void Remove(Hash key);
        bool TryGetInfo(Hash key, out StorageItemInfo info);

        void Clear();
    }
}
