using Sidi.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Util
{
    public class HtmlCellFormat<T>
    {
        static HtmlGenerator h = new HtmlGenerator();

        public Func<T, int, ListFormat<T>.Column, object> Format = (r, i, c) => h.Text(c.GetText(r, i));
        public bool HasDedicatedRow = false;
    }
}
