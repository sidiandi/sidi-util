using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.IO;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Reflection;

namespace Sidi.Util
{
    /// <summary>
    /// Wrapper for a byte[] hash that was created by a cryptographic hash function
    /// </summary>
    [Serializable]
    public class Hash : IXmlSerializable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IReadOnlyCollection<byte> Value { get { return Array.AsReadOnly(hash); } }
        byte[] hash;

        public Hash(byte[] hash)
        {
            this.hash = hash;
        }

        public Hash(string hexString)
        {
            Set(hexString);
        }

        void Set(string hexString)
        {
            this.hash = IEnumerableByteExtensions.HexStringToBytes(hexString).ToArray();
        }

        Hash()
        {
        }

        public override bool Equals(object obj)
        {
            var r = obj as Hash;
            if (r == null)
            {
                return false;
            }

            return hash.SequenceEqual(r.hash);
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(hash, 0);
        }

        public override string ToString()
        {
            return hash.HexString();
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            var hexString = new LPath(reader.ReadString());
            Set(hexString);
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteString(this.Value.HexString());
        }
    }
}
