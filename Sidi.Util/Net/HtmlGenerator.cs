using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Web;

namespace Sidi.Net
{
    public class HtmlGenerator
    {
        public Action<TextWriter> Tag(string tag, params object[] childs)
        {
            return o =>
            {
                o.WriteLine("<{0}>", tag);
                foreach (var i in childs)
                {
                    RenderChild(o, i);
                }
                o.WriteLine("</{0}>", tag);
            };
        }

        void RenderChild(TextWriter o, object i)
        {
            if (i is Action<TextWriter>)
            {
                var f = (Action<TextWriter>)i;
                f(o);
            }
            if (i is string)
            {
                o.WriteLine(HttpUtility.HtmlEncode(i));
            }
            else if (i is System.Collections.IEnumerable)
            {
                foreach (object j in ((System.Collections.IEnumerable)i))
                {
                    RenderChild(o, j);
                }
            }
            else
            {
                o.WriteLine(HttpUtility.HtmlEncode(i));
            }
        }

        public Action<TextWriter> html(params object[] childs)
        {
            return o =>
            {
                o.WriteLine(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01//EN"" ""http://www.w3.org/TR/html4/strict.dtd"">");
                Tag("html", childs)(o);
            };
        }
        public Action<TextWriter> head(params object[] childs) { return Tag(MethodBase.GetCurrentMethod().Name, childs); }
        public Action<TextWriter> title(params object[] childs) { return Tag(MethodBase.GetCurrentMethod().Name, childs); }
        public Action<TextWriter> body(params object[] childs) { return Tag(MethodBase.GetCurrentMethod().Name, childs); }
        public Action<TextWriter> p(params object[] childs) { return Tag(MethodBase.GetCurrentMethod().Name, childs); }
        public Action<TextWriter> h1(params object[] childs) { return Tag(MethodBase.GetCurrentMethod().Name, childs); }
        public Action<TextWriter> h2(params object[] childs) { return Tag(MethodBase.GetCurrentMethod().Name, childs); }
        public Action<TextWriter> h3(params object[] childs) { return Tag(MethodBase.GetCurrentMethod().Name, childs); }
        public Action<TextWriter> h4(params object[] childs) { return Tag(MethodBase.GetCurrentMethod().Name, childs); }
        public Action<TextWriter> h5(params object[] childs) { return Tag(MethodBase.GetCurrentMethod().Name, childs); }
        public Action<TextWriter> h6(params object[] childs) { return Tag(MethodBase.GetCurrentMethod().Name, childs); }
        public Action<TextWriter> small(params object[] childs) { return Tag(MethodBase.GetCurrentMethod().Name, childs); }
        public Action<TextWriter> hr(params object[] childs) { return Tag(MethodBase.GetCurrentMethod().Name, childs); }
        public Action<TextWriter> div(params object[] childs) { return Tag(MethodBase.GetCurrentMethod().Name, childs); }
    }
}
