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
using System.IO;
using Sidi.Util;
using Sidi.Extensions;
using Sidi.Test;

namespace Sidi.Util
{
    [TestFixture]
    public class StringExTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        [Test]
        public void GetSection()
        {
            StringWriter w = new StringWriter();
            w.WriteLine("[SectionA]");
            w.WriteLine("hello");
            w.WriteLine("[SectionB]");
            w.WriteLine("world");

            string t = w.ToString();

            Assert.AreEqual("hello", t.GetSection("SectionA"));
            Assert.AreEqual("world", t.GetSection("SectionB"));
        }

        [Test]
        public void SafeToString()
        {
            object o = null;
            Assert.AreEqual(String.Empty, o.SafeToString());

            o = "Hello";
            Assert.AreEqual("Hello", o.SafeToString());
        }

        [Test]
        public void SafeToStringEnum()
        {
            var e = Enumerable.Range(1, 100);
            Assert.IsTrue(e.SafeToString().StartsWith("[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18"));
        }

        [Test]
        public void SafeToStringNestedEnum()
        {
            var e = Enumerable.Range(1, 3).Select(_ => Enumerable.Range(1, 3));
            Assert.AreEqual("[[1, 2, 3], [1, 2, 3], [1, 2, 3]]", e.SafeToString());
        }

        [Test]
        public void SafeToStringNestedEnumTruncated()
        {
            var e = Enumerable.Range(1, 100).Select(_ => Enumerable.Range(1, 100));
            var s = e.SafeToString();
            Assert.AreEqual("[[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, ...], ...]", s);
        }
    }
}
