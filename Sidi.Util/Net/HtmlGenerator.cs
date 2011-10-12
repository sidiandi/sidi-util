using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Web;
using Sidi.Util;

namespace Sidi.Net
{
    public class HtmlGenerator
    {
        public HtmlGenerator()
        {
            ErrorMessage = e => 
            html(head(), body(Verbose(o => o.WriteLine(e.ToString()))));
        }

        public Action<TextWriter> Tag(string tag, params object[] childs)
        {
            return o =>
            {
                try
                {
                    o.Write("<{0} ", tag);
                    foreach (var i in childs.OfType<Attribute>())
                    {
                        i.Render(o);
                    }
                    o.WriteLine(">", tag);
                    foreach (var i in childs)
                    {
                        RenderChild(o, i);
                    }
                }
                catch (Exception e)
                {
                    if (tag == "body")
                    {
                        this.Verbose(x => x.WriteLine(e.ToString()))(o);
                    }
                    else
                    {
                        throw new Exception(tag, e);
                    }
                }
                finally
                {
                    o.WriteLine("</{0}>", tag);
                }
            };
        }

        void RenderChild(TextWriter o, object i)
        {
            if (i is Action<TextWriter>)
            {
                var f = (Action<TextWriter>)i;
                f(o);
            }
            else if (i is string)
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
            else if (i is Attribute)
            {
            }
            else
            {
                o.WriteLine(HttpUtility.HtmlEncode(i));
            }
        }

        /// <summary>
        /// Exception-safe way to render the html object to a TextWriter
        /// </summary>
        /// <param name="?"></param>
        public void Write(TextWriter o, Func<object> htmlGenerator)
        {
            try
            {
                RenderChild(o, htmlGenerator());    
            }
            catch (Exception e)
            {
                ErrorMessage(e)(o);
            }
        }

        public Func<Exception, Action<TextWriter>> ErrorMessage;

        public Action<TextWriter> html(params object[] childs)
        {
            return o =>
            {
                o.WriteLine(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01//EN"" ""http://www.w3.org/TR/html4/strict.dtd"">");
                Tag("html", childs)(o);
            };
        }

        public class Attribute
        {
            public Attribute(object[] p)
            {
                if ((p.Length % 2) != 0)
                {
                    throw new ArgumentException("number of arguments must be even.");
                }
                this.p = p;
            }

            object[] p;

            public void Render(TextWriter o)
            {
                var e = p.GetEnumerator();
                for (; e.MoveNext(); )
                {
                    var key = e.Current;
                    e.MoveNext();
                    var value = e.Current;
                    o.Write("{0}={1} ", key, HttpUtility.HtmlEncode(value.ToString()).Quote());
                }
            }
        }

        public Attribute Att(params object[] childs)
        {
            return new Attribute(childs);
        }

        public Action<TextWriter> Verbose(Action<TextWriter> a)
        {
            return new Action<TextWriter>(x =>
            {
                using (var cw = new HtmlCodeWriter(x))
                {
                    a(cw);
                }
            });
        }

        public Action<TextWriter> a(params object[] childs) { return Tag("a", childs); }
        public Action<TextWriter> abbr(params object[] childs) { return Tag("abbr", childs); }
        public Action<TextWriter> address(params object[] childs) { return Tag("address", childs); }
        public Action<TextWriter> area(params object[] childs) { return Tag("area", childs); }
        public Action<TextWriter> article(params object[] childs) { return Tag("article", childs); }
        public Action<TextWriter> aside(params object[] childs) { return Tag("aside", childs); }
        public Action<TextWriter> audio(params object[] childs) { return Tag("audio", childs); }
        public Action<TextWriter> b(params object[] childs) { return Tag("b", childs); }
        public Action<TextWriter> base_(params object[] childs) { return Tag("base", childs); }
        public Action<TextWriter> bdi(params object[] childs) { return Tag("bdi", childs); }
        public Action<TextWriter> bdo(params object[] childs) { return Tag("bdo", childs); }
        public Action<TextWriter> blockquote(params object[] childs) { return Tag("blockquote", childs); }
        public Action<TextWriter> body(params object[] childs) { return Tag("body", childs); }
        public Action<TextWriter> br(params object[] childs) { return Tag("br", childs); }
        public Action<TextWriter> button(params object[] childs) { return Tag("button", childs); }
        public Action<TextWriter> canvas(params object[] childs) { return Tag("canvas", childs); }
        public Action<TextWriter> caption(params object[] childs) { return Tag("caption", childs); }
        public Action<TextWriter> cite(params object[] childs) { return Tag("cite", childs); }
        public Action<TextWriter> code(params object[] childs) { return Tag("code", childs); }
        public Action<TextWriter> col(params object[] childs) { return Tag("col", childs); }
        public Action<TextWriter> colgroup(params object[] childs) { return Tag("colgroup", childs); }
        public Action<TextWriter> command(params object[] childs) { return Tag("command", childs); }
        public Action<TextWriter> datalist(params object[] childs) { return Tag("datalist", childs); }
        public Action<TextWriter> dd(params object[] childs) { return Tag("dd", childs); }
        public Action<TextWriter> del(params object[] childs) { return Tag("del", childs); }
        public Action<TextWriter> details(params object[] childs) { return Tag("details", childs); }
        public Action<TextWriter> dfn(params object[] childs) { return Tag("dfn", childs); }
        public Action<TextWriter> div(params object[] childs) { return Tag("div", childs); }
        public Action<TextWriter> dl(params object[] childs) { return Tag("dl", childs); }
        public Action<TextWriter> dt(params object[] childs) { return Tag("dt", childs); }
        public Action<TextWriter> em(params object[] childs) { return Tag("em", childs); }
        public Action<TextWriter> embed(params object[] childs) { return Tag("embed", childs); }
        public Action<TextWriter> fieldset(params object[] childs) { return Tag("fieldset", childs); }
        public Action<TextWriter> figcaption(params object[] childs) { return Tag("figcaption", childs); }
        public Action<TextWriter> figure(params object[] childs) { return Tag("figure", childs); }
        public Action<TextWriter> footer(params object[] childs) { return Tag("footer", childs); }
        public Action<TextWriter> form(params object[] childs) { return Tag("form", childs); }
        public Action<TextWriter> h1(params object[] childs) { return Tag("h1", childs); }
        public Action<TextWriter> h2(params object[] childs) { return Tag("h2", childs); }
        public Action<TextWriter> h3(params object[] childs) { return Tag("h3", childs); }
        public Action<TextWriter> h4(params object[] childs) { return Tag("h4", childs); }
        public Action<TextWriter> h5(params object[] childs) { return Tag("h5", childs); }
        public Action<TextWriter> h6(params object[] childs) { return Tag("h6", childs); }
        public Action<TextWriter> head(params object[] childs) { return Tag("head", childs); }
        public Action<TextWriter> header(params object[] childs) { return Tag("header", childs); }
        public Action<TextWriter> hgroup(params object[] childs) { return Tag("hgroup", childs); }
        public Action<TextWriter> hr(params object[] childs) { return Tag("hr", childs); }
        public Action<TextWriter> i(params object[] childs) { return Tag("i", childs); }
        public Action<TextWriter> iframe(params object[] childs) { return Tag("iframe", childs); }
        public Action<TextWriter> img(params object[] childs) { return Tag("img", childs); }
        public Action<TextWriter> input(params object[] childs) { return Tag("input", childs); }
        public Action<TextWriter> ins(params object[] childs) { return Tag("ins", childs); }
        public Action<TextWriter> kbd(params object[] childs) { return Tag("kbd", childs); }
        public Action<TextWriter> keygen(params object[] childs) { return Tag("keygen", childs); }
        public Action<TextWriter> label(params object[] childs) { return Tag("label", childs); }
        public Action<TextWriter> legend(params object[] childs) { return Tag("legend", childs); }
        public Action<TextWriter> li(params object[] childs) { return Tag("li", childs); }
        public Action<TextWriter> link(params object[] childs) { return Tag("link", childs); }
        public Action<TextWriter> map(params object[] childs) { return Tag("map", childs); }
        public Action<TextWriter> mark(params object[] childs) { return Tag("mark", childs); }
        public Action<TextWriter> menu(params object[] childs) { return Tag("menu", childs); }
        public Action<TextWriter> meta(params object[] childs) { return Tag("meta", childs); }
        public Action<TextWriter> meter(params object[] childs) { return Tag("meter", childs); }
        public Action<TextWriter> nav(params object[] childs) { return Tag("nav", childs); }
        public Action<TextWriter> noscript(params object[] childs) { return Tag("noscript", childs); }
        public Action<TextWriter> object_(params object[] childs) { return Tag("object", childs); }
        public Action<TextWriter> ol(params object[] childs) { return Tag("ol", childs); }
        public Action<TextWriter> optgroup(params object[] childs) { return Tag("optgroup", childs); }
        public Action<TextWriter> option(params object[] childs) { return Tag("option", childs); }
        public Action<TextWriter> output(params object[] childs) { return Tag("output", childs); }
        public Action<TextWriter> p(params object[] childs) { return Tag("p", childs); }
        public Action<TextWriter> param(params object[] childs) { return Tag("param", childs); }
        public Action<TextWriter> pre(params object[] childs) { return Tag("pre", childs); }
        public Action<TextWriter> progress(params object[] childs) { return Tag("progress", childs); }
        public Action<TextWriter> q(params object[] childs) { return Tag("q", childs); }
        public Action<TextWriter> rp(params object[] childs) { return Tag("rp", childs); }
        public Action<TextWriter> rt(params object[] childs) { return Tag("rt", childs); }
        public Action<TextWriter> ruby(params object[] childs) { return Tag("ruby", childs); }
        public Action<TextWriter> s(params object[] childs) { return Tag("s", childs); }
        public Action<TextWriter> samp(params object[] childs) { return Tag("samp", childs); }
        public Action<TextWriter> script(params object[] childs) { return Tag("script", childs); }
        public Action<TextWriter> section(params object[] childs) { return Tag("section", childs); }
        public Action<TextWriter> select(params object[] childs) { return Tag("select", childs); }
        public Action<TextWriter> small(params object[] childs) { return Tag("small", childs); }
        public Action<TextWriter> source(params object[] childs) { return Tag("source", childs); }
        public Action<TextWriter> span(params object[] childs) { return Tag("span", childs); }
        public Action<TextWriter> strong(params object[] childs) { return Tag("strong", childs); }
        public Action<TextWriter> style(params object[] childs) { return Tag("style", childs); }
        public Action<TextWriter> sub(params object[] childs) { return Tag("sub", childs); }
        public Action<TextWriter> summary(params object[] childs) { return Tag("summary", childs); }
        public Action<TextWriter> sup(params object[] childs) { return Tag("sup", childs); }
        public Action<TextWriter> table(params object[] childs) { return Tag("table", childs); }
        public Action<TextWriter> tbody(params object[] childs) { return Tag("tbody", childs); }
        public Action<TextWriter> td(params object[] childs) { return Tag("td", childs); }
        public Action<TextWriter> textarea(params object[] childs) { return Tag("textarea", childs); }
        public Action<TextWriter> tfoot(params object[] childs) { return Tag("tfoot", childs); }
        public Action<TextWriter> th(params object[] childs) { return Tag("th", childs); }
        public Action<TextWriter> thead(params object[] childs) { return Tag("thead", childs); }
        public Action<TextWriter> time(params object[] childs) { return Tag("time", childs); }
        public Action<TextWriter> title(params object[] childs) { return Tag("title", childs); }
        public Action<TextWriter> tr(params object[] childs) { return Tag("tr", childs); }
        public Action<TextWriter> track(params object[] childs) { return Tag("track", childs); }
        public Action<TextWriter> u(params object[] childs) { return Tag("u", childs); }
        public Action<TextWriter> ul(params object[] childs) { return Tag("ul", childs); }
        public Action<TextWriter> var(params object[] childs) { return Tag("var", childs); }
        public Action<TextWriter> video(params object[] childs) { return Tag("video", childs); }
        public Action<TextWriter> wbr(params object[] childs) { return Tag("wbr", childs); }

        public Attribute accept(object value) { return Att("accept", value); }
        public Attribute accept_charset(object value) { return Att("accept-charset", value); }
        public Attribute accesskey(object value) { return Att("accesskey", value); }
        public Attribute action(object value) { return Att("action", value); }
        public Attribute alt(object value) { return Att("alt", value); }
        public Attribute async(object value) { return Att("async", value); }
        public Attribute autocomplete(object value) { return Att("autocomplete", value); }
        public Attribute autofocus(object value) { return Att("autofocus", value); }
        public Attribute autoplay(object value) { return Att("autoplay", value); }
        public Attribute border(object value) { return Att("border", value); }
        public Attribute challenge(object value) { return Att("challenge", value); }
        public Attribute charset(object value) { return Att("charset", value); }
        public Attribute checked_(object value) { return Att("checked", value); }
        public Attribute cite(object value) { return Att("cite", value); }
        public Attribute class_(object value) { return Att("class", value); }
        public Attribute cols(object value) { return Att("cols", value); }
        public Attribute colspan(object value) { return Att("colspan", value); }
        public Attribute content(object value) { return Att("content", value); }
        public Attribute contenteditable(object value) { return Att("contenteditable", value); }
        public Attribute contextmenu(object value) { return Att("contextmenu", value); }
        public Attribute controls(object value) { return Att("controls", value); }
        public Attribute coords(object value) { return Att("coords", value); }
        public Attribute crossorigin(object value) { return Att("crossorigin", value); }
        public Attribute data(object value) { return Att("data", value); }
        public Attribute datetime(object value) { return Att("datetime", value); }
        public Attribute default_(object value) { return Att("default", value); }
        public Attribute defer(object value) { return Att("defer", value); }
        public Attribute dir(object value) { return Att("dir", value); }
        public Attribute dirname(object value) { return Att("dirname", value); }
        public Attribute disabled(object value) { return Att("disabled", value); }
        public Attribute draggable(object value) { return Att("draggable", value); }
        public Attribute dropzone(object value) { return Att("dropzone", value); }
        public Attribute enctype(object value) { return Att("enctype", value); }
        public Attribute for_(object value) { return Att("for", value); }
        public Attribute form(object value) { return Att("form", value); }
        public Attribute formaction(object value) { return Att("formaction", value); }
        public Attribute formenctype(object value) { return Att("formenctype", value); }
        public Attribute formmethod(object value) { return Att("formmethod", value); }
        public Attribute formnovalidate(object value) { return Att("formnovalidate", value); }
        public Attribute formtarget(object value) { return Att("formtarget", value); }
        public Attribute headers(object value) { return Att("headers", value); }
        public Attribute height(object value) { return Att("height", value); }
        public Attribute hidden(object value) { return Att("hidden", value); }
        public Attribute high(object value) { return Att("high", value); }
        public Attribute href(object value) { return Att("href", value); }
        public Attribute hreflang(object value) { return Att("hreflang", value); }
        public Attribute http_equiv(object value) { return Att("http-equiv", value); }
        public Attribute icon(object value) { return Att("icon", value); }
        public Attribute id(object value) { return Att("id", value); }
        public Attribute ismap(object value) { return Att("ismap", value); }
        public Attribute keytype(object value) { return Att("keytype", value); }
        public Attribute kind(object value) { return Att("kind", value); }
        public Attribute label(object value) { return Att("label", value); }
        public Attribute lang(object value) { return Att("lang", value); }
        public Attribute list(object value) { return Att("list", value); }
        public Attribute loop(object value) { return Att("loop", value); }
        public Attribute low(object value) { return Att("low", value); }
        public Attribute manifest(object value) { return Att("manifest", value); }
        public Attribute max(object value) { return Att("max", value); }
        public Attribute maxlength(object value) { return Att("maxlength", value); }
        public Attribute media(object value) { return Att("media", value); }
        public Attribute mediagroup(object value) { return Att("mediagroup", value); }
        public Attribute method(object value) { return Att("method", value); }
        public Attribute min(object value) { return Att("min", value); }
        public Attribute multiple(object value) { return Att("multiple", value); }
        public Attribute muted(object value) { return Att("muted", value); }
        public Attribute name(object value) { return Att("name", value); }
        public Attribute novalidate(object value) { return Att("novalidate", value); }
        public Attribute open(object value) { return Att("open", value); }
        public Attribute optimum(object value) { return Att("optimum", value); }
        public Attribute pattern(object value) { return Att("pattern", value); }
        public Attribute placeholder(object value) { return Att("placeholder", value); }
        public Attribute poster(object value) { return Att("poster", value); }
        public Attribute preload(object value) { return Att("preload", value); }
        public Attribute pubdate(object value) { return Att("pubdate", value); }
        public Attribute radiogroup(object value) { return Att("radiogroup", value); }
        public Attribute readonly_(object value) { return Att("readonly", value); }
        public Attribute rel(object value) { return Att("rel", value); }
        public Attribute required(object value) { return Att("required", value); }
        public Attribute reversed(object value) { return Att("reversed", value); }
        public Attribute rows(object value) { return Att("rows", value); }
        public Attribute rowspan(object value) { return Att("rowspan", value); }
        public Attribute sandbox(object value) { return Att("sandbox", value); }
        public Attribute spellcheck(object value) { return Att("spellcheck", value); }
        public Attribute scope(object value) { return Att("scope", value); }
        public Attribute scoped(object value) { return Att("scoped", value); }
        public Attribute seamless(object value) { return Att("seamless", value); }
        public Attribute selected(object value) { return Att("selected", value); }
        public Attribute shape(object value) { return Att("shape", value); }
        public Attribute size(object value) { return Att("size", value); }
        public Attribute sizes(object value) { return Att("sizes", value); }
        public Attribute span(object value) { return Att("span", value); }
        public Attribute src(object value) { return Att("src", value); }
        public Attribute srcdoc(object value) { return Att("srcdoc", value); }
        public Attribute srclang(object value) { return Att("srclang", value); }
        public Attribute start(object value) { return Att("start", value); }
        public Attribute step(object value) { return Att("step", value); }
        public Attribute style(object value) { return Att("style", value); }
        public Attribute tabindex(object value) { return Att("tabindex", value); }
        public Attribute target(object value) { return Att("target", value); }
        public Attribute title(object value) { return Att("title", value); }
        public Attribute type(object value) { return Att("type", value); }
        public Attribute typemustmatch(object value) { return Att("typemustmatch", value); }
        public Attribute usemap(object value) { return Att("usemap", value); }
        public Attribute value(object value) { return Att("value", value); }
        public Attribute width(object value) { return Att("width", value); }
        public Attribute wrap(object value) { return Att("wrap", value); }
    }
}
