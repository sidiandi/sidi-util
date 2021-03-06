// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using Sidi.Util;
using Sidi.Extensions;

namespace Sidi.Net
{
    public class HtmlCodeWriter : TextWriter
    {
        TextWriter o;
        TextWriter ownO;

        public HtmlCodeWriter(TextWriter o)
        {
            this.o = o;
            this.o.WriteLine("<code>");
        }

        public HtmlCodeWriter(Stream o)
            : this(new StreamWriter(o))
        {
            this.ownO = this.o;
        }

        public override Encoding Encoding
        {
            get { return ownO.Encoding; }
        }

        public override void WriteLine(string value)
        {
            foreach (var line in value.Lines())
            {
                o.WriteLine(line);
                o.WriteLine("<br>");
            }
        }

        protected override void Dispose(bool disposing)
        {
            o.WriteLine("</code>");
            if (ownO != null)
            {
                ownO.Close();
            }
            base.Dispose(disposing);
        }
    }
}
