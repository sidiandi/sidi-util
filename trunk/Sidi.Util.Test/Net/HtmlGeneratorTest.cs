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

        [Test]
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
