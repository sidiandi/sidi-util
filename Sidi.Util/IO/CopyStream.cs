using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    class CopyStream : Stream
    {
        Stream input;
        Stream copyDestination;

        public CopyStream(Stream input, Stream copyDestination)
        {
            this.input = input;
            this.copyDestination = copyDestination;
        }

        public override bool CanRead
        {
            get { return input.CanRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = input.Read(buffer, offset, count);
            if (bytesRead > 0)
            {
                copyDestination.Write(buffer, offset, bytesRead);
            }
            return bytesRead;
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
            throw new NotImplementedException();
        }
    }
}
