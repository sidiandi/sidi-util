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
using Sidi.IO;
using Sidi.Extensions;
using Sidi.Test;
using Sidi.Util;

namespace Sidi.Caching
{
    [TestFixture]
    public class SqliteCacheTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Serializable]
        class Key
        {
            public int Value;

            public override int GetHashCode()
            {
                return 0;
            }

            public override bool Equals(object obj)
            {
                var r = (Key)obj;
                return r != null && Value == r.Value;
            }
        }

        class Demo
        {
            public string Lookup(string id)
            {
                return SqliteCache.Get(id, LookupUncached);
            }

            public string LookupUncached(string id)
            {
                return String.Format("Looked up: {0}", id);
            }

            public int Add(int x, int y)
            {
                return SqliteCache.Get(x, y, AddUncached);
            }

            public int AddUncached(int x, int y)
            {
                return x + y;
            }
        }

        [Test]
        public void TestSimpleCache()
        {
            var demo = new Demo();
            {
                var result = demo.Lookup("John");
                Assert.AreEqual("Looked up: John", result);

                Assert.AreEqual(2, demo.Add(1, 1));
                Assert.AreEqual(3, demo.Add(1, 2));
                Assert.AreEqual(3, demo.Add(2, 1));
            }
        }

        [Test]
        public void HashCodeCollision()
        {
            var c = SqliteCache.Local(MethodBase.GetCurrentMethod());
            {
                var key1 = new Key() { Value = 1 };
                var key2 = new Key() { Value = 2 };
                c.Clear();
                Assert.AreEqual(1, c.GetCached(key1, _ => 1));
                Assert.AreEqual(2, c.GetCached(key2, _ => 2));
                var cacheDir = Paths.GetLocalApplicationDataDirectory(typeof(Cache));
            }
        }

        [Test]
        public void Clear()
        {
            var c = SqliteCache.Local(MethodBase.GetCurrentMethod());
            {
                c.Clear();
                Assert.AreEqual(1, c.GetCached(1, _ => 1));
                Assert.IsTrue(c.Contains(1));
                c.Reset(1);
                Assert.IsFalse(c.Contains(1));
                Assert.AreEqual(2, c.GetCached(1, _ => 2));
            }
        }

        [Test]
        public void DoCache()
        {
            {
                var c = SqliteCache.Local(MethodBase.GetCurrentMethod());
                c.Clear();

                for (int i = 0; i < 10; ++i)
                {
                    var result = c.GetCached(i, _ => _ * _);
                    Assert.AreEqual(i * i, result);
                }
            }

            {
                var c = SqliteCache.Local(MethodBase.GetCurrentMethod());
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
        }

        [Test]
        public void ExceptionRemembered()
        {
            var c = SqliteCache.Local(MethodBase.GetCurrentMethod());
            {
                c.Clear();
                c.RememberExceptions = true;

                var d = 0;
                try
                {
                    var r = c.GetCached(d, x => 1 / x);
                    Assert.IsTrue(false);
                }
                catch (Exception e)
                {
                    Assert.IsTrue(e is System.DivideByZeroException);
                }

                try
                {
                    // this should not raise an exception
                    // but since RememberExceptions = true, 
                    // the cached DivideByZeroException will be returned.
                    var r = c.GetCached(d, _ => 1 / 1);
                    Assert.IsTrue(false);
                }
                catch (Exception e)
                {
                    Assert.IsTrue(e is System.DivideByZeroException);
                }
            }
        }

        [Test]
        public void ExceptionNotRemembered()
        {
            var c = SqliteCache.Local(MethodBase.GetCurrentMethod());
            {
                c.Clear();
                c.RememberExceptions = false;

                var d = 0;
                try
                {
                    var r = c.GetCached(d, _ => 1 / _);
                    Assert.IsTrue(false);
                }
                catch (Exception e)
                {
                    Assert.IsTrue(e is System.DivideByZeroException);
                }

                // this should not raise an exception
                // and since RememberExceptions = true, 
                // the cached DivideByZeroException will not be returned.
                {
                    var r = c.GetCached(d, _ => 1 / 1);
                }
            }
        }

        [Test]
        public void Performance()
        {
            {
                var cache = SqliteCache.Local(MethodBase.GetCurrentMethod());
                cache.Clear();
                int count = 1000;
                using (new LogScope(log.Info, "Caching {0} items", count))
                {
                    for (int i = 0; i < count; ++i)
                    {
                        cache.GetCached(i, _ => _.ToString());
                    }
                }
            }

            {
                var cache = SqliteCache.Local(MethodBase.GetCurrentMethod());
                int count = 1000;
                using (new LogScope(log.Info, "Caching {0} items", count))
                {
                    for (int i = 0; i < count; ++i)
                    {
                        cache.GetCached(i, _ => _.ToString());
                    }
                }
            }
        }

        [Test]
        public void ReadFile()
        {
            var content = "hello";
            var file = TestFile("file-with-content");
            file.WriteAllText(content);
            var cache = SqliteCache.Local(MethodBase.GetCurrentMethod());
            {
                var readContent = cache.GetCached(new LPathWriteTime(file), _ => _.Path.ReadAllText());
                Assert.AreEqual(content, readContent);
                content = "new content";
                file.WriteAllText(content);
                readContent = cache.GetCached(new LPathWriteTime(file), _ => _.Path.ReadAllText());
                Assert.AreEqual(content, readContent);

                Assert.IsTrue(cache.IsCached(new LPathWriteTime(file)));
                cache.Clear();
                Assert.IsFalse(cache.IsCached(new LPathWriteTime(file)));
            }
        }
    }
}
