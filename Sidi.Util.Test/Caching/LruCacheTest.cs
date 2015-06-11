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
using System.Threading;
using Sidi.Test;

namespace Sidi.Caching
{
    [TestFixture]
    public class LruCacheTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Caching()
        {
            var maxCount = 10;
            Func<int, int> provider = x => x * 2;
            var c = new LruCache<int, int>(maxCount, provider);
            Assert.AreEqual(maxCount, c.MaxCount);
            Assert.AreEqual(0, c.Count);
            for (int i = 0; i < maxCount; ++i)
            {
                Assert.AreEqual(provider(i), c[i]);
            }
            Assert.AreEqual(maxCount, c.Count);
            for (int i = 0 + maxCount; i < maxCount + maxCount; ++i)
            {
                Assert.AreEqual(provider(i), c[i]);
            }
            Assert.AreEqual(maxCount, c.Count);
            Assert.IsTrue(c.ContainsKey(maxCount));
            Assert.IsFalse(c.ContainsKey(0));
            c.Clear();
            Assert.AreEqual(0, c.Count);
        }

        public class DisposableClass : IDisposable
        {
            public static int instanceCount = 0;

            public DisposableClass()
            {
                ++instanceCount;
            }

            private bool _disposed = false;

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        --instanceCount;
                    }
                    // Free your own state (unmanaged objects).
                    // Set large fields to null.
                    _disposed = true;
                }
            }
        }

        [Test]
        public void DisposeTest()
        {
            var count = 100;
            using (var c = new LruCache<int, DisposableClass>(count, x => new DisposableClass()))
            {
                Enumerable.Range(0, count).Select(x => c[x]).ToList();
                Assert.AreEqual(count, DisposableClass.instanceCount);
            }
            Assert.AreEqual(0, DisposableClass.instanceCount);
        }
    }
}
