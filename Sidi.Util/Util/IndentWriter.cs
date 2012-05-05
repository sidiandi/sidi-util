using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sidi.Util
{
    public class IndentWriter : TextWriter
    {
        public IndentWriter(TextWriter output, string prefix, bool initialPrefix)
        {
            this.output = output;
            this.prefix = prefix;
            doWritePrefix = initialPrefix;
        }

        TextWriter output;
        string prefix;

        public override Encoding Encoding
        {
            get { return output.Encoding; }
        }

        public override void Write(char value)
        {
            WritePrefix();
            output.Write(value);
            doWritePrefix = value == 10;
        }

        public override void WriteLine(string value)
        {
            WritePrefix();
            output.WriteLine(value);
            doWritePrefix = true;
        }

        void WritePrefix()
        {
            if (doWritePrefix)
            {
                output.Write(prefix);
                doWritePrefix = false;
            }
        }

        bool doWritePrefix = true;
    }
}
