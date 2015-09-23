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

#region "Mandatory NUnit Imports"
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using NUnit.Framework;
#endregion

using Sidi.Persistence;
using System.IO;
using System.Data.Common;
using System.Linq;
using Sidi.IO;
using Sidi.Test;

namespace Sidi.Persistence
{
    [TestFixture]
    public class DictionaryTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /*
        public void SetUp()
        {
            
        }

        public void TearDown()
        {
            dictionary.Dispose();
            path.EnsureNotExists();
        }
         */

        [Test()]
        public void ReadWrite()
        {
            var path = TestFile("dictionary_test.sqlite");
            path.EnsureNotExists();
            using (var dictionary = new Sidi.Persistence.Dictionary<string, string>(path, "options"))
            {
                string verboseValue = "on";
                string userValue = "sidi";
                dictionary["verbose"] = verboseValue;
                dictionary["user"] = userValue;

                Assert.AreEqual(verboseValue, dictionary["verbose"]);
                Assert.AreEqual(userValue, dictionary["user"]);
            }
        }

        [Test()]
        public void Enumerate()
        {
            var path = TestFile("dictionary_test.sqlite");
            path.EnsureNotExists();
            using (var dictionary = new Sidi.Persistence.Dictionary<string, string>(path, "options"))
            {
                var count = 10;
                var lasti = count - 1;
                foreach (var i in Enumerable.Range(0, count))
                {
                    dictionary[i.ToString()] = (i * i).ToString();
                }

                Assert.AreEqual(count, dictionary.Count);
                Assert.AreEqual(count, dictionary.Keys.Count());
                Assert.IsTrue(dictionary.Keys.Any(x => x == lasti.ToString()));
                Assert.IsTrue(dictionary.Values.Any(x => x == (lasti * lasti).ToString()));
            }
        }

        [Test()]
        public void MassReadWrite()
        {
            const int elementCount = 1000;
            
            var path = TestFile("dictionary_test.sqlite");
            path.EnsureNotExists();
            using (var dictionary = new Sidi.Persistence.Dictionary<string, string>(path, "options"))
            {
                Stopwatch w = new Stopwatch();
                w.Start();
                using (DbTransaction t = dictionary.BeginTransaction())
                {
                    for (long i = 0; i < elementCount; ++i)
                    {
                        string key = String.Join(".", BitConverter.GetBytes(i).Select(x => "Parameter " + x.ToString()).ToArray());
                        string value = String.Format("Value{0}", i);
                        dictionary[key] = value;
                    }
                    t.Commit();
                }
                log.InfoFormat("{0} to insert {1} elements", w.Elapsed, elementCount);
            }
        }

        [Test]
        public void UserSetting()
        {
            var value = "world";
            var key = "hello";
            var dictionaryName = "test";

            using (var d = Dictionary<string, string>.UserSetting(GetType(), dictionaryName))
            {
                d[key] = value;
            }

            using (var d = Dictionary<string, string>.UserSetting(GetType(), dictionaryName))
            {
                Assert.AreEqual(value, d[key]);
            }
        }

        [Test]
        public void UseSerializableTypes()
        {
            var path = TestFile("dictionary_test2.sqlite");
            path.EnsureFileNotExists();
            using (var d = new Sidi.Persistence.Dictionary<FileVersion, FileVersion>(path, "fileversion"))
            {
                var fv = new FileVersion("a", 1, DateTime.MinValue);
                d[fv] = fv;

                FileVersion outValue;
                Assert.IsTrue(d.TryGetValue(fv, out outValue));
                Assert.AreEqual(fv, outValue);
                Assert.AreEqual(1, d.Count);
                Assert.IsTrue(d.Remove(fv));
                Assert.AreEqual(0, d.Count);
            }
        }

        [Test]
        public void ReadWriteTime()
        {
            var dbFile = NewTestFile("StringDateTimeDic.sqlite");
            var d = new Sidi.Persistence.Dictionary<string, DateTime>(dbFile, "StringDateTimeDic");
            var id = "Id";
            var time = DateTime.UtcNow;
            d[id] = time;
            Assert.AreEqual(time, d[id]);
        }

    }
}
