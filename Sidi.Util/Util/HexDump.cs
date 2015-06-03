using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Util
{
    /// <summary>
    /// Static helper methods to create hex dumps
    /// </summary>
    public static class HexDump
    {
        public static void Write(IEnumerable<byte> data, TextWriter output)
        {
            var m = new MemoryStream(data.ToArray());
            using (var hd = new HexDumpStream(output))
            {
                m.CopyTo(hd);
            }
        }

        public static string AsHexDump(this IEnumerable<byte> data)
        {
            var m = new MemoryStream(data.ToArray());
            var output = new StringWriter();
            using (var hd = new HexDumpStream(output))
            {
                m.CopyTo(hd);
            }
            return output.ToString();
        }
    }

    /// <summary>
    /// Dumps all data written to as hexdump to the output TextWriter
    /// </summary>
    public class HexDumpStream : Stream
    {
        public HexDumpStream(TextWriter output)
        {
            this.output = output;
            NonPrintableCharacterReplacement = '.';
        }

        private readonly TextWriter output;
        int position = 0;
        int lineBufferPosition = 0;
        byte[] lineBuffer = new byte[16];

        public int Columns
        {
            get { return lineBuffer.Length; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                OutputLine();
                lineBuffer = new byte[value];
            }
        }

        public char NonPrintableCharacterReplacement { get; set; }

        void OutputLine()
        {
            if (lineBufferPosition == 0)
            {
                return;
            }

            output.Write("{0:X8} ", position);
            for (int c = 0; c < Columns; ++c)
            {
                if (c < lineBufferPosition)
                {
                    output.Write("{0:X2} ", lineBuffer[c]);
                }
                else
                {
                    output.Write("   ");
                }
            }
            for (int c = 0; c < Columns; ++c)
            {
                    var character = (char)lineBuffer[c];
                    if (c < lineBufferPosition)
                    {
                        if (IsPrintable(character))
                        {
                            output.Write("{0}", character);
                        }
                        else
                        {
                            output.Write(NonPrintableCharacterReplacement);
                        }
                    }
                    else
                    {
                        output.Write(" ");
                    }
            }
            output.WriteLine();

            position += lineBufferPosition;
            lineBufferPosition = 0;
        }

        public static bool IsPrintable(char c)
        {
            return 0x20 <= c;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                Flush();
            }
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                return position + lineBufferPosition;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

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
            for (int s = offset; s < offset + count; ++s)
            {
                lineBuffer[lineBufferPosition++] = buffer[s];
                if (lineBufferPosition >= lineBuffer.Length)
                {
                    OutputLine();
                }
            }
        }

        public override void Flush()
        {
            OutputLine();
        }
    }
}
