// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
