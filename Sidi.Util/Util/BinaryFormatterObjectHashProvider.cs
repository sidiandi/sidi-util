using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Util
{
    public class BinaryFormatterObjectHashProvider : IObjectHashProvider
    {
        IHashProvider hashProvider;

        public BinaryFormatterObjectHashProvider(IHashProvider hashProvider)
        {
            this.hashProvider = hashProvider;
        }

        public Hash Get(object x)
        {
            if (x is MethodBase)
            {
                return Get(x.ToString());
            }

            using (var m = new MemoryStream())
            {
                if (x != null)
                {
                    var ser = new BinaryFormatter();
                    ser.Serialize(m, x);
                }
                m.Seek(0, SeekOrigin.Begin);
                return hashProvider.Get(m);
            }
        }
    }
}
