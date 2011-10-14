using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sidi.Net
{
    [TestFixture]
    public class WebServerTest
    {
        public class HelloWorld : HtmlGenerator
        {
            public object Hello(string name)
            {
                return html(head(), body("Hello, ", name));
            }

            public object Multiply(int x)
            {
                return html(head(), body(
                    table(
                        th(td("x"), td("y")),
                        TableRows(Enumerable.Range(0, 10), n => n, n => x * n)
                        )
                        )
                        );
            }

            public object Index()
            {
                return html(head(), body(a(href("Hello"), "Hello")));
            }
        }
        
        public void Serve()
        {
            var hw = new HelloWorld();

            var ws = new WebServer();
            ws.Handlers[String.Empty] = hw;

            ws.Run();
        }
    }
}
