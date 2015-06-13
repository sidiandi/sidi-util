using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Util
{
    public interface IObjectHashProvider
    {
        Hash Get(object x);
    }

    public class ObjectHashProvider
    {
        public static IObjectHashProvider GetDefault()
        {
            return new XmlSerializerObjectHashProvider(HashProvider.GetDefault());
        }
    }
}
