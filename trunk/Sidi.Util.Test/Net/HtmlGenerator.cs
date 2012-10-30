using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Sidi.Util;
using Sidi.Extensions;

namespace Sidi.Net
{
    [TestFixture]
    public class HtmlGeneratorTest : HtmlGenerator
    {
        [Test]
        public void HelloWorld()
        {
            var name = "Andreas";
            var page =
                html(
                    head(title("A Hello world page with ä ü & < >")),
                    body(
                        h1("Hello, ", name),
                        Enumerable.Range(0,10).Select(i => h2(i))
                    )
                );

            page(Console.Out);
        }

        [Test, ExpectedException(typeof(NullReferenceException))]
        public void ExceptionHandling()
        {
            string name = null;
            var o = new StringWriter();
            html(head(), body(name.Length))(o);
        }

        [Test]
        public void CatchException()
        {
            this.Catch = e => div(e);
            string name = null;
            var o = new StringWriter();
            html(head(), body(new Action<TextWriter>(tw => div(name.Length)(tw))))
                (o);

            Assert.IsTrue(o.ToString().Contains("NullReferenceException"));
        }

        [Test, ExpectedException(typeof(NullReferenceException))]
        public void ExceptionWhileRendering()
        {
            var g = new HtmlGenerator();
            string name = null;
            var o = new StringWriter();
            g.html(g.head(), g.body(
                g.img(
                    g.div(new Action<TextWriter>(x => x.WriteLine(name.Length)))
                )
                ))(o);
            Console.WriteLine(o.ToString());
        }

        [Test]
        public void Table()
        {
            var e = Enumerable.Range(0, 10);

            table(
                th("Value", "Square"), 
                TableRows(e, x => x, x => x * x))
            (Console.Out);
        }

        /// <summary>
        /// Generates attribute and tag for form HtmlGenerator.cs
        /// </summary>
        [Test, Explicit("not really a test")]
        public void GenerateCode()
        {
            var web = new HtmlWeb();
            var d = web.Load("http://dev.w3.org/html5/spec/index.html");
            var t = d.DocumentNode.SelectSingleNode("//h3[@id=\"elements-1\"]").ParentNode.SelectSingleNode("descendant::tbody");
            var tags = t.SelectNodes("descendant::tr")
                .SelectMany(tr => tr.SelectNodes("descendant::th").Take(1))
                .SelectMany(th => Regex.Split(th.InnerText, @",\s+"))
                .Select(tag => "public Action<TextWriter> " + tag + "(params object[] childs) { return Tag(\"" + tag + "\", childs); }");

            Console.WriteLine(tags.Join());

            var attributes = 
                d.DocumentNode.SelectSingleNode("//h3[@id=\"attributes-1\"]")
                .NextSibling.NextSibling
                .SelectSingleNode("descendant::tbody")
                .SelectNodes("descendant::tr")
                .Select(tr => tr.SelectSingleNode("descendant::th").InnerText.Trim())
                .Distinct()
                .Select(attribute => "        public Attribute " + attribute + "(object value) { return Att(" + attribute.Quote() + ", value); }");

            Console.WriteLine(attributes.Join());
        }
    }
}
