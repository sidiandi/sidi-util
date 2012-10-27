// Copyright (c) 2011, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.Threading;

namespace Sidi.Cache
{
    [TestFixture]
    public class LruCacheBackgroundTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Caching()
        {
            var maxCount = 10;
            Func<int, int> provider = x =>
                {
                    Thread.Sleep(100);
                    return x * 2;
                };

            int updateCount = 0;
            using (var c = new LruCacheBackground<int, int>(maxCount, x => x * 2, 5))
            {
                c.EntryUpdated += (o, e) =>
                {
                    ++updateCount;
                };

                c.DefaultValueWhileLoading = x => -1;
                for (int i = 0; i < maxCount; ++i)
                {
                    Assert.AreEqual(-1, c[i]);
                }

                Thread.Sleep(200);
                Assert.AreEqual(maxCount, updateCount);

                for (int i = 0; i < maxCount; ++i)
                {
                    Assert.AreEqual(provider(i), c[i]);
                }

                c.Clear();
            }
        }
    }
}
