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
        static IHashProvider hashProvider = HashProvider.GetDefault();

        public static void Write(this IHashAddressableStorage s, object key, object value)
        {
            var hash = hashProvider.GetObjectHash(key);
            using (var stream = s.Write(hash))
            {
                var b = new BinaryFormatter();
                b.Serialize(stream, value);
            }
        }

        public static object Read(this IHashAddressableStorage s, object key)
        {
            var hash = hashProvider.GetObjectHash(key);
            using (var stream = s.Read(hash))
            {
                var b = new BinaryFormatter();
                return b.Deserialize(stream);
            }
        }

        public static T Read<T>(this IHashAddressableStorage s, Hash key, Func<Stream, T> reader)
        {
            var hash = hashProvider.GetObjectHash(key);
            using (var stream = s.Read(hash))
            {
                return reader(stream);
            }
        }
    }
}
