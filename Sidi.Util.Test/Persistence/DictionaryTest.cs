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

//Test Specific Imports
//TODO - Add imports your going to test here
using Sidi.Persistence;
using System.IO;
using System.Data.Common;
using System.Linq;

namespace Sidi.Persistence
{

    [TestFixture]
    public class DictionaryTest
    {
        #region "Custom Trace Listener"
        MyListener listener = new MyListener();

        internal class MyListener : TraceListener
        {
            public override void Write(string message)
            {
                Console.Write(message);
            }


            public override void WriteLine(string message)
            {
                Console.WriteLine(message);
            }
        }
        #endregion

        Sidi.Persistence.Dictionary<string, string> dictionary;

        [SetUp()]
        public void SetUp()
        {
            //Setup our custom trace listener
            if (!Trace.Listeners.Contains(listener))
            {
                Trace.Listeners.Add(listener);
            }

            //TODO - Setup your test objects here
            string path = @"d:\temp\dictionary_test.sqlite";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            dictionary = new Sidi.Persistence.Dictionary<string, string>(path, "options");
        }

        [TearDown()]
        public void TearDown()
        {
            //Remove our custom trace listener
            if (Trace.Listeners.Contains(listener))
            {
                Trace.Listeners.Remove(listener);
            }

            //TODO - Tidy up your test objects here
            dictionary.Close();
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
        public void MassReadWrite()
        {
            Stopwatch w = new Stopwatch();
            w.Start();
            using (DbTransaction t = dictionary.BeginTransaction())
            {
                for (long i = 0; i < 100000; ++i)
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