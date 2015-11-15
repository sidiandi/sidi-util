using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    /// <summary>
    /// Abstract stream base class for streams that can only read forward and have unknown length - very much like an IEnumerable
    /// </summary>
    public abstract class ReadForwardStream : Stream
    {
        public override bool CanRead
        {
            get
            {
                return true;
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
                return false;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        long position = 0;

        public override long Position
        {
            get
            {
                return position;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var r = ReadImpl(buffer, offset, count);
            position += r;
            return r;
        }

        protected abstract int ReadImpl(byte[] buffer, int offset, int count);

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
            throw new NotImplementedException();
        }
    }

}
