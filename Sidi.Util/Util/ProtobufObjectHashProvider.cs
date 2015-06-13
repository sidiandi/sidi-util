using ProtoBuf.Meta;
using Sidi.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace Sidi.Util
{
    public class ProtobufObjectHashProvider : IObjectHashProvider
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IHashProvider hashProvider;

        public ProtobufObjectHashProvider(IHashProvider hashProvider)
        {
            InitializeRuntimeTypeModel(RuntimeTypeModel.Default);
            this.hashProvider = hashProvider;
        }

        static void InitializeRuntimeTypeModel(RuntimeTypeModel tm)
        {
            if (!tm.CanSerialize(typeof(FileVersion)))
            {
                var mt = tm.Add(typeof(FileVersion), false);
                mt.Add("Path", "Length", "LastWriteTimeUtc");

                mt = tm.Add(typeof(LPath), false);
                mt.SetSurrogate(typeof(LPathSurrogate));
            }
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
                    ProtoBuf.Serializer.Serialize(m, x);
                    m.Seek(0, SeekOrigin.Begin);
                }
                return hashProvider.Get(m);
            }
        }
    }
}
