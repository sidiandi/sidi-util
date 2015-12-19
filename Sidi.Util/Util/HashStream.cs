using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Util
{
    /// <summary>
    /// Calculates a hash of all the data written to.
    /// Usage: write data, close, GetHash()
    /// </summary>
    public class HashStream : Stream
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public HashStream(HashAlgorithm hashAlgorithm)
        {
            this.hashAlgorithm = hashAlgorithm;
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
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

        public override void Flush()
        {
        }

        public override void Close()
        {
            if (hashAlgorithm != null)
            {
                base.Close();
                hashAlgorithm.TransformFinalBlock(new byte[] { }, 0, 0);
                hash = new Hash(hashAlgorithm.Hash);
                hashAlgorithm.Dispose();
                hashAlgorithm = null;
            }
        }

        /// <summary>
        /// Get the hash of the data written to the stream. Implicitly closes the stream.
        /// </summary>
        /// <returns></returns>
        public Hash GetHash()
        {
            Close();
            return hash;
        }

        Hash hash;

        HashAlgorithm hashAlgorithm;

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
            hashAlgorithm.TransformBlock(buffer, offset, count, buffer, offset);
            position += count;
        }

        long position = 0;
    }
}
