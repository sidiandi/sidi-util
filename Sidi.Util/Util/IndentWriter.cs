// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

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
