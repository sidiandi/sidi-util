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

namespace Sidi.Persistence
{

    [TestFixture]
    public class DictionaryTest : TestBase
    {
        Sidi.Persistence.Dictionary<string, string> dictionary;

        [SetUp()]
        public void SetUp()
        {
            path = TestFile("dictionary_test.sqlite");
            path.EnsureNotExists();
            dictionary = new Sidi.Persistence.Dictionary<string, string>(path, "options");
        }

        Sidi.IO.LPath path;

        [TearDown()]
        public void TearDown()
        {
            dictionary.Dispose();
            path.EnsureNotExists();
        }

        [Test()]
        public void ReadWrite()
        {
            string verboseValue = "on";
            string userValue = "sidi";
            dictionary["verbose"] = verboseValue;
            dictionary["user"] = userValue;

            Assert.AreEqual(verboseValue, dictionary["verbose"]);
            Assert.AreEqual(userValue, dictionary["user"]);
        }

        [Test()]
        public void Enumerate()
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
            Assert.IsTrue(dictionary.Values.Any(x => x == (lasti*lasti).ToString()));

            foreach (var i in dictionary)
            {
                Console.WriteLine(i);
            }
        }

        [Test()]
        public void MassReadWrite()
        {
            Stopwatch w = new Stopwatch();
            w.Start();
            using (DbTransaction t = dictionary.BeginTransaction())
            {
                for (long i = 0; i < 1000; ++i)
                {
                    string key = String.Join(".", BitConverter.GetBytes(i).Select(x => "Parameter " + x.ToString()).ToArray());
                    string value = String.Format("Value{0}", i);
                    dictionary[key] = value;
                }
                t.Commit();
            }
            Console.WriteLine(w.Elapsed);
        }
    }
}
