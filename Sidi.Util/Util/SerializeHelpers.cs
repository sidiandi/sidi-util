using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Util
{
    public static class BinaryFormatterHelper
    {
        public static void Serialize<T>(Stream writer, T value)
        {
            var b = new BinaryFormatter();
            b.Serialize(writer, value);
        }

        public static T Deserialize<T>(Stream reader)
        {
            var b = new BinaryFormatter();
            return (T)b.Deserialize(reader);
        }
    }
}
