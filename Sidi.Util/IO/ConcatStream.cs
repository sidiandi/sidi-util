using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    public class ConcatStream : ReadForwardStream
    {
        IEnumerator<Stream> inputStreams;

        public ConcatStream(IEnumerable<Stream> inputStreams)
        {
            this.inputStreams = inputStreams.GetEnumerator();
            this.inputStreams.MoveNext();
        }

        void NextStream()
        {
            if (inputStreams == null)
            {
            }
            else
            {
                inputStreams.Current.Dispose();
                if (inputStreams.MoveNext())
                {
                }
                else
                {
                    inputStreams.Dispose();
                    inputStreams = null;
                }
            }
        }

        protected override int ReadImpl(byte[] buffer, int offset, int count)
        {
            if (inputStreams == null)
            {
                return 0;
            }
            else
            {
                var r = inputStreams.Current.Read(buffer, offset, count);
                if (r == 0)
                {
                    NextStream();
                    return ReadImpl(buffer, offset, count);
                }
                return r;
            }
        }
    }
}
