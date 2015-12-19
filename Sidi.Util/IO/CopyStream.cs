using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    public class CopyStream : ReadForwardStream
    {
        Stream input;
        Stream[] copyDestination;

        /// <summary>
        /// Constructs 
        /// </summary>
        /// <param name="input">Input stream. Its content will be returned when reading this stream.</param>
        /// <param name="copyDestination">When content is read from the constructed stream, it will also be copied to the copyDestination streams.</param>
        public CopyStream(Stream input, params Stream[] copyDestination)
        {
            this.input = input;
            this.copyDestination = copyDestination;
        }

        protected override int ReadImpl(byte[] buffer, int offset, int count)
        {
            var bytesRead = input.Read(buffer, offset, count);
            if (bytesRead > 0)
            {
                foreach (var d in copyDestination)
                {
                    d.Write(buffer, offset, bytesRead);
                }
            }
            return bytesRead;
        }
    }
}
