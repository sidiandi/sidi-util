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
using System.Reflection;

namespace Sidi.Cache
{
    [TestFixture]
    public class CacheTest
    {
        [Test]
        public void DoCache()
        {
            var c = Cache.Local(MethodBase.GetCurrentMethod());
            c.Clear();
            
            for (int i = 0; i < 10; ++i)
            {
                var result = c.GetCached(i, () => i * i);
                Assert.AreEqual(i * i, result);
            }

            c = Cache.Local(MethodBase.GetCurrentMethod());
            for (int i = 0; i < 10; ++i)
            {
                Assert.IsTrue(c.IsCached(i));
            }

            c.Clear();
            for (int i = 0; i < 10; ++i)
            {
                Assert.IsFalse(c.IsCached(i));
            }
        }

        [Test]
        public void Exception()
        {
            var c = Cache.Local(MethodBase.GetCurrentMethod());
            c.Clear();

            var d = 0;
            try
            {
                var r = c.GetCached(d, () => 1 / d);
                Assert.IsTrue(false);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is System.DivideByZeroException);
            }

            try
            {
                var r = c.GetCached(d, () => 1 / d);
                Assert.IsTrue(false);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is System.DivideByZeroException);
            }
        }
    }
}
