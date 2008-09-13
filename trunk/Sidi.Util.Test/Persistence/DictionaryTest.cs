// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
