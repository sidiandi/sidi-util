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
using Sidi.Extensions;

namespace Sidi.Extensions
{
    [TestFixture]
    public class IEnumerableExtensionsTest
    {
        [Test]
        public void Best()
        {
            var r = Enumerable.Range(0, 25).Select(x => x.ToString());
            Assert.AreEqual("24", r.Best(x => Int32.Parse(x)));
        }

        [Test]
        public void SafeSelect()
        {
            var x = Enumerable.Range(0, 10).ToList();
            var y = x.SafeSelect(i => 100 / i);
            Assert.AreEqual(x.Count() - 1, y.Count());
        }
    }
}
