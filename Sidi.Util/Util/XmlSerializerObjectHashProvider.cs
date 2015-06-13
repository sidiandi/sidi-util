using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sidi.Util
{
    public class XmlSerializerObjectHashProvider : IObjectHashProvider
    {
        IHashProvider hashProvider;

        public XmlSerializerObjectHashProvider(IHashProvider hashProvider)
        {
            this.hashProvider = hashProvider;
        }

        XmlSerializer GetSerializer(Type type)
        {
            if (!object.Equals(type, lastType))
            {
                lastSerializer = new XmlSerializer(type);
                lastType = type;
            }
            return lastSerializer;
        }
        Type lastType;
        XmlSerializer lastSerializer;

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
                    var s = GetSerializer(x.GetType());
                    s.Serialize(m, x);
                    m.Seek(0, SeekOrigin.Begin);
                }
                return hashProvider.Get(m);
            }
        }
    }
}
