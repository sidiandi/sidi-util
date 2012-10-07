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
            get { throw new NotImplementedException(); }
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
