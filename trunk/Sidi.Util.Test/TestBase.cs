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
using Sidi.IO;
using Sidi.Util;
using System.Reflection;
using log4net.Repository.Hierarchy;
using log4net;
using log4net.Appender;
using log4net.Layout;
using System.IO;

namespace Sidi
{
    public class TestBase
    {
        static TestBase()
        {
            var traceAppender = new TraceAppender()
            {
                Layout = new PatternLayout()
                {
                    ConversionPattern = "%date [%thread] %-5level %logger [%property{NDC}] - %message%newline",
                },
            };

            traceAppender.ActivateOptions();

            var hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.AddAppender(traceAppender);
            hierarchy.Configured = true;
        }

        public TestBase()
        {
        }

        protected Sidi.IO.Long.Path TestFile(string relPath)
        {
            var testFile = 
                new Sidi.IO.Long.Path(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)
                .Parent
                .Parent
                .CatDir("test", relPath);
            return testFile;
        }
    }
}
