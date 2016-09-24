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
using System.Threading;
using NUnit.Framework;
using System.Globalization;

namespace Sidi.Util
{
    [TestFixture]
    public class BinaryPrefixTest
    {
        [Test]
        public void Binary()
        {
            var b = BinaryPrefix.Instance;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Assert.AreEqual("1.00 Ki", String.Format(b, "{0}", 1024));
            Assert.AreEqual("1023", String.Format(b, "{0}", 1023));
            Assert.AreEqual("1.46 Ki", String.Format(b, "{0}", 1500));
            Assert.AreEqual("1.00 Mi", String.Format(b, "{0}", 1 << 20));
            Assert.AreEqual("1.00 Mi", String.Format(b, "{0}", (long)(1 << 20)));
            Assert.AreEqual("1.00 Mi", String.Format(b, "{0}", (ulong)(1 << 20)));
            Assert.AreEqual("1.00 Ki", String.Format(b, "{0}", (Int16)(1 << 10)));
            Assert.AreEqual("1.00 Ki", String.Format(b, "{0}", (UInt16)(1 << 10)));
            Assert.AreEqual("Hello, World", String.Format(b, "{0}", "Hello, World"));
        }
    }
}
