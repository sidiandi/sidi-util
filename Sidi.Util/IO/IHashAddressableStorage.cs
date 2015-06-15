using Sidi.Util;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

        bool ContainsKey(Hash hash);
        bool Remove(Hash key);
        bool TryGetInfo(Hash key, out StorageItemInfo info);

        void Clear();
    }

    public static class IHashAddressableStorageExtensions
    {
        public static void Write<T>(this IHashAddressableStorage s, object key, IObjectHashProvider keyHashProvider, T value, Action<Stream, T> valueWriter)
        {
            var hash = keyHashProvider.Get(key);
            using (var stream = s.Write(hash))
            {
                valueWriter(stream, value);
            }
        }

        public static T Read<T>(this IHashAddressableStorage s, object key, IObjectHashProvider keyHashProvider, Func<Stream, T> valueReader)
        {
            var hash = keyHashProvider.Get(key);
            using (var stream = s.Read(hash))
            {
                return valueReader(stream);
            }
        }

        public static T Read<T>(this IHashAddressableStorage s, Hash hash, Func<Stream, T> reader)
        {
            using (var stream = s.Read(hash))
            {
                return reader(stream);
            }
        }
    }
}
