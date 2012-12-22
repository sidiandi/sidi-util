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
using System.Reflection;
using System.Web;
using Sidi.Util;
using Sidi.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Sidi.Net
{
    public class HtmlGenerator
    {
        public HtmlGenerator()
        {
        }

        public Action<TextWriter> Tag(string tag, params object[] childs)
        {
            return o =>
            {
                try
                {
                    o.Write("<{0}", tag);
                    foreach (var i in childs.OfType<AttributeItem>())
                    {
                        i.Render(o);
                    }
                    o.Write(">", tag);
                    foreach (var i in childs)
                    {
                        RenderChild(o, i);
                    }
                }
                catch (Exception ex)
                {
                    if (Catch == null)
                    {
                        throw;
                    }
                    else
                    {
                        Catch(ex)(o);
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
                o.Write(HttpUtility.HtmlEncode(i));
            }
            else if (i is System.Collections.IEnumerable)
            {
                foreach (object j in ((System.Collections.IEnumerable)i))
                {
                    RenderChild(o, j);
                }
            }
            else if (i is AttributeItem)
            {
            }
            else
            {
                o.Write(HttpUtility.HtmlEncode(i));
            }
        }

        /// <summary>
        /// Exception-safe way to render the html object to a TextWriter
        /// </summary>
        /// <param name="o"></param>
        /// <param name="htmlGenerator"></param>
        public void Write(TextWriter o, Func<object> htmlGenerator)
        {
            try
            {
                RenderChild(o, htmlGenerator());    
            }
            catch (Exception e)
            {
                Catch(e)(o);
            }
        }

        public object TableRows<T>(IEnumerable<T> e, params Func<T, object>[] format)
        {
            return e.Select(r => tr(format.Select(c => 
                {
                    var cellContent = c(r);
                    if (cellContent is String)
                    {
                        var cellString = (string)cellContent;
                        if (String.IsNullOrEmpty(cellString))
                        {
                            cellContent = "&nbsp;";
                        }
                    }
                    return td();
                })));
        }

        public Func<Exception, Action<TextWriter>> Catch;

        public Action<TextWriter> html(params object[] childs)
        {
            return o =>
            {
                o.WriteLine(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01//EN"" ""http://www.w3.org/TR/html4/strict.dtd"">");
                Tag("html", childs)(o);
            };
        }

        public class AttributeItem
        {
            public AttributeItem(object[] p)
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
                    o.Write(" {0}={1} ", key, HttpUtility.HtmlEncode(value.ToString()).Quote());
                }
            }
        }

        public AttributeItem Att(params object[] childs)
        {
            return new AttributeItem(childs);
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

        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> a(params object[] childs) { return Tag("a", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> abbr(params object[] childs) { return Tag("abbr", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> address(params object[] childs) { return Tag("address", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> area(params object[] childs) { return Tag("area", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> article(params object[] childs) { return Tag("article", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> aside(params object[] childs) { return Tag("aside", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> audio(params object[] childs) { return Tag("audio", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> b(params object[] childs) { return Tag("b", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> base_(params object[] childs) { return Tag("base", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> bdi(params object[] childs) { return Tag("bdi", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> bdo(params object[] childs) { return Tag("bdo", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> blockquote(params object[] childs) { return Tag("blockquote", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> body(params object[] childs) { return Tag("body", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> br(params object[] childs) { return Tag("br", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> button(params object[] childs) { return Tag("button", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> canvas(params object[] childs) { return Tag("canvas", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> caption(params object[] childs) { return Tag("caption", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> cite(params object[] childs) { return Tag("cite", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> code(params object[] childs) { return Tag("code", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> col(params object[] childs) { return Tag("col", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> colgroup(params object[] childs) { return Tag("colgroup", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> command(params object[] childs) { return Tag("command", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> datalist(params object[] childs) { return Tag("datalist", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> dd(params object[] childs) { return Tag("dd", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> del(params object[] childs) { return Tag("del", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> details(params object[] childs) { return Tag("details", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> dfn(params object[] childs) { return Tag("dfn", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> div(params object[] childs) { return Tag("div", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> dl(params object[] childs) { return Tag("dl", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> dt(params object[] childs) { return Tag("dt", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> em(params object[] childs) { return Tag("em", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> embed(params object[] childs) { return Tag("embed", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> fieldset(params object[] childs) { return Tag("fieldset", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> figcaption(params object[] childs) { return Tag("figcaption", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> figure(params object[] childs) { return Tag("figure", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> footer(params object[] childs) { return Tag("footer", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> form(params object[] childs) { return Tag("form", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> h1(params object[] childs) { return Tag("h1", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> h2(params object[] childs) { return Tag("h2", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> h3(params object[] childs) { return Tag("h3", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> h4(params object[] childs) { return Tag("h4", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> h5(params object[] childs) { return Tag("h5", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> h6(params object[] childs) { return Tag("h6", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> head(params object[] childs) { return Tag("head", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> header(params object[] childs) { return Tag("header", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> hgroup(params object[] childs) { return Tag("hgroup", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> hr(params object[] childs) { return Tag("hr", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> i(params object[] childs) { return Tag("i", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> iframe(params object[] childs) { return Tag("iframe", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> img(params object[] childs) { return Tag("img", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> input(params object[] childs) { return Tag("input", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> ins(params object[] childs) { return Tag("ins", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> kbd(params object[] childs) { return Tag("kbd", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> keygen(params object[] childs) { return Tag("keygen", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> label(params object[] childs) { return Tag("label", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> legend(params object[] childs) { return Tag("legend", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> li(params object[] childs) { return Tag("li", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> link(params object[] childs) { return Tag("link", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> map(params object[] childs) { return Tag("map", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> mark(params object[] childs) { return Tag("mark", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> menu(params object[] childs) { return Tag("menu", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> meta(params object[] childs) { return Tag("meta", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> meter(params object[] childs) { return Tag("meter", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> nav(params object[] childs) { return Tag("nav", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> noscript(params object[] childs) { return Tag("noscript", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> object_(params object[] childs) { return Tag("object", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> ol(params object[] childs) { return Tag("ol", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> optgroup(params object[] childs) { return Tag("optgroup", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> option(params object[] childs) { return Tag("option", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> output(params object[] childs) { return Tag("output", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> p(params object[] childs) { return Tag("p", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> param(params object[] childs) { return Tag("param", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> pre(params object[] childs) { return Tag("pre", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> progress(params object[] childs) { return Tag("progress", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> q(params object[] childs) { return Tag("q", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> rp(params object[] childs) { return Tag("rp", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> rt(params object[] childs) { return Tag("rt", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> ruby(params object[] childs) { return Tag("ruby", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> s(params object[] childs) { return Tag("s", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> samp(params object[] childs) { return Tag("samp", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> script(params object[] childs) { return Tag("script", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> section(params object[] childs) { return Tag("section", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> select(params object[] childs) { return Tag("select", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> small(params object[] childs) { return Tag("small", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> source(params object[] childs) { return Tag("source", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> span(params object[] childs) { return Tag("span", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> strong(params object[] childs) { return Tag("strong", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> style(params object[] childs) { return Tag("style", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> sub(params object[] childs) { return Tag("sub", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> summary(params object[] childs) { return Tag("summary", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> sup(params object[] childs) { return Tag("sup", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> table(params object[] childs) { return Tag("table", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> tbody(params object[] childs) { return Tag("tbody", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> td(params object[] childs) { return Tag("td", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> textarea(params object[] childs) { return Tag("textarea", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> tfoot(params object[] childs) { return Tag("tfoot", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> th(params object[] childs) { return Tag("th", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> thead(params object[] childs) { return Tag("thead", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> time(params object[] childs) { return Tag("time", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> title(params object[] childs) { return Tag("title", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> tr(params object[] childs) { return Tag("tr", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> track(params object[] childs) { return Tag("track", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> u(params object[] childs) { return Tag("u", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> ul(params object[] childs) { return Tag("ul", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> var(params object[] childs) { return Tag("var", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> video(params object[] childs) { return Tag("video", childs); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public Action<TextWriter> wbr(params object[] childs) { return Tag("wbr", childs); }
 
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem accept(object value) { return Att("accept", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem accept_charset(object value) { return Att("accept-charset", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem accesskey(object value) { return Att("accesskey", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem action(object value) { return Att("action", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem alt(object value) { return Att("alt", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem async(object value) { return Att("async", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem autocomplete(object value) { return Att("autocomplete", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem autofocus(object value) { return Att("autofocus", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem autoplay(object value) { return Att("autoplay", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem border(object value) { return Att("border", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem challenge(object value) { return Att("challenge", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem charset(object value) { return Att("charset", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem checked_(object value) { return Att("checked", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem cite(object value) { return Att("cite", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem class_(object value) { return Att("class", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem cols(object value) { return Att("cols", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem colspan(object value) { return Att("colspan", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem content(object value) { return Att("content", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem contenteditable(object value) { return Att("contenteditable", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem contextmenu(object value) { return Att("contextmenu", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem controls(object value) { return Att("controls", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem coords(object value) { return Att("coords", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem crossorigin(object value) { return Att("crossorigin", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem data(object value) { return Att("data", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem datetime(object value) { return Att("datetime", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem default_(object value) { return Att("default", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem defer(object value) { return Att("defer", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem dir(object value) { return Att("dir", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem dirname(object value) { return Att("dirname", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem disabled(object value) { return Att("disabled", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem draggable(object value) { return Att("draggable", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem dropzone(object value) { return Att("dropzone", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem enctype(object value) { return Att("enctype", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem for_(object value) { return Att("for", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem form(object value) { return Att("form", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem formaction(object value) { return Att("formaction", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem formenctype(object value) { return Att("formenctype", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem formmethod(object value) { return Att("formmethod", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem formnovalidate(object value) { return Att("formnovalidate", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem formtarget(object value) { return Att("formtarget", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem headers(object value) { return Att("headers", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem height(object value) { return Att("height", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem hidden(object value) { return Att("hidden", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem high(object value) { return Att("high", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem href(object value) { return Att("href", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem hreflang(object value) { return Att("hreflang", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem http_equiv(object value) { return Att("http-equiv", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem icon(object value) { return Att("icon", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem id(object value) { return Att("id", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem ismap(object value) { return Att("ismap", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem keytype(object value) { return Att("keytype", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem kind(object value) { return Att("kind", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem label(object value) { return Att("label", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem lang(object value) { return Att("lang", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem list(object value) { return Att("list", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem loop(object value) { return Att("loop", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem low(object value) { return Att("low", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem manifest(object value) { return Att("manifest", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem max(object value) { return Att("max", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem maxlength(object value) { return Att("maxlength", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem media(object value) { return Att("media", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem mediagroup(object value) { return Att("mediagroup", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem method(object value) { return Att("method", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem min(object value) { return Att("min", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem multiple(object value) { return Att("multiple", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem muted(object value) { return Att("muted", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem name(object value) { return Att("name", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem novalidate(object value) { return Att("novalidate", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem open(object value) { return Att("open", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem optimum(object value) { return Att("optimum", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem pattern(object value) { return Att("pattern", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem placeholder(object value) { return Att("placeholder", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem poster(object value) { return Att("poster", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem preload(object value) { return Att("preload", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem pubdate(object value) { return Att("pubdate", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem radiogroup(object value) { return Att("radiogroup", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem readonly_(object value) { return Att("readonly", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem rel(object value) { return Att("rel", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem required(object value) { return Att("required", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem reversed(object value) { return Att("reversed", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem rows(object value) { return Att("rows", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem rowspan(object value) { return Att("rowspan", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem sandbox(object value) { return Att("sandbox", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem spellcheck(object value) { return Att("spellcheck", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem scope(object value) { return Att("scope", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem scoped(object value) { return Att("scoped", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem seamless(object value) { return Att("seamless", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem selected(object value) { return Att("selected", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem shape(object value) { return Att("shape", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem size(object value) { return Att("size", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem sizes(object value) { return Att("sizes", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem span(object value) { return Att("span", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem src(object value) { return Att("src", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem srcdoc(object value) { return Att("srcdoc", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem srclang(object value) { return Att("srclang", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem start(object value) { return Att("start", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem step(object value) { return Att("step", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem style(object value) { return Att("style", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem tabindex(object value) { return Att("tabindex", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem target(object value) { return Att("target", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem title(object value) { return Att("title", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem type(object value) { return Att("type", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem typemustmatch(object value) { return Att("typemustmatch", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem usemap(object value) { return Att("usemap", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem value(object value) { return Att("value", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem width(object value) { return Att("width", value); }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        public AttributeItem wrap(object value) { return Att("wrap", value); }
    }
}
