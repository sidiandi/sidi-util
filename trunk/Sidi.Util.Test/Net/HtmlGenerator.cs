using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using NUnit.Framework;

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
    }
}
