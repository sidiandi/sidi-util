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
using L = Sidi.IO;
using Sidi.Util;
using System.Reflection;
using log4net.Repository.Hierarchy;
using log4net;
using log4net.Appender;
using log4net.Layout;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace Sidi.Test
{
    public class TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            hierarchy.Root.Level = log4net.Core.Level.All;
            hierarchy.Configured = true;
        }

        public TestBase()
        {
        }

        protected Sidi.IO.LPath TestFile(Sidi.IO.LPath relPath)
        {
            var testFile =
                Paths.BinDir
                .Parent
                .CatDir("test", relPath);
            return testFile;
        }

        protected Sidi.IO.LPath NewTestFile(Sidi.IO.LPath relPath)
        {
            var tf = TestFile(relPath);
            tf.EnsureNotExists();
            tf.EnsureParentDirectoryExists();
            return tf;
        }

        [Serializable]
        class TestObjectGraph<T>
        {
            public T p;
        }

        protected static void TestSerialize<T>(T p)
        {
            var b = new BinaryFormatter();
            var m = new MemoryStream();
            var og1 = new TestObjectGraph<T> { p = p };
            b.Serialize(m, og1);
            m.Seek(0, SeekOrigin.Begin);
            var og2 = (TestObjectGraph<T>)b.Deserialize(m);
            if (!object.Equals(og1.p, og2.p))
            {
                throw new Exception("object not equal after serialization and deserialization");
            }
        }

        protected static void TestXmlSerialize<T>(T n)
        {
            var s = new XmlSerializer(typeof(T));
            var t = new System.IO.StringWriter();
            s.Serialize(t, n);
            log.Info(t.ToString());
            var n1 = s.Deserialize(new System.IO.StringReader(t.ToString()));
            if (!object.Equals(n, n1))
            {
                throw new Exception("object not equal after serialization and deserialization");
            }
        }
    }
}
