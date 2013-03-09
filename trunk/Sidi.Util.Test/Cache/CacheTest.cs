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

namespace Sidi.Cache
{
    [TestFixture]
    public class CacheTest : TestBase
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

        [Test]
        public void HashCodeCollision()
        {
            var c = Cache.Local(MethodBase.GetCurrentMethod());
            var key1 = new Key(){ Value = 1};
            var key2 = new Key(){ Value = 2};
            c.Clear();
            Assert.AreEqual(1, c.GetCached(key1, () => 1));
            Assert.AreEqual(2, c.GetCached(key2, () => 2));
            var cacheDir = typeof(Cache).UserSetting("cache");
        }
        
        [Test]
        public void Clear()
        {
            var c = Cache.Local(MethodBase.GetCurrentMethod());
            c.Clear();
            Assert.AreEqual(1, c.GetCached(1, () => 1));
            c.Clear(1);
            Assert.AreEqual(2, c.GetCached(1, () => 2));
        }
        
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
        public void ExceptionRemembered()
        {
            var c = Cache.Local(MethodBase.GetCurrentMethod());
            c.Clear();
            c.RememberExceptions = true;

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
                // this should not raise an exception
                // but since RememberExceptions = true, 
                // the cached DivideByZeroException will be returned.
                var r = c.GetCached(d, () => 1 / 1); 
                Assert.IsTrue(false);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is System.DivideByZeroException);
            }
        }

        [Test]
        public void ExceptionNotRemembered()
        {
            var c = Cache.Local(MethodBase.GetCurrentMethod());
            c.Clear();
            c.RememberExceptions = false;

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

            // this should not raise an exception
            // and since RememberExceptions = true, 
            // the cached DivideByZeroException will not be returned.
            {
                var r = c.GetCached(d, () => 1 / 1);
            }
        }

        [Test]
        public void GetCachedFile()
        {
            var key = "hello";
            var c = Cache.Local(MethodBase.GetCurrentMethod());
            c.Clear();
            var file = c.GetCachedFile(key, () =>
                {
                    var f = TestFile("hello");
                    LFile.WriteAllText(f, "hello");
                    return f;
                });

            log.Info(file);
            Assert.IsTrue(c.IsCached(key));
            log.Info(c.GetCachedFile(key, () =>
                {
                    var f = TestFile("hello");
                    LFile.WriteAllText(f, "hello");
                    return f;
                }));
        }
    }
}
