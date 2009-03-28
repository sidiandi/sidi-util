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
using System.Linq;
#endregion

//Test Specific Imports
using Sidi.CommandLine;
using System.IO;

namespace Sidi.Util.Test
{
    [TestFixture]
    public class TokenizerUt
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


        [SetUp()]
        public void SetUp()
        {
            //Setup our custom trace listener
            if (!Trace.Listeners.Contains(listener))
            {
                Trace.Listeners.Add(listener);
            }

            //TODO - Setup your test objects here
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
        }

        [Test()]
        public void TokenizerTest()
        {
            Tokenizer tokenizer = new Tokenizer(new StringReader("hello world \"quoted string\" # comment\r\n\"another string with \"\"quotes\"\" inside.\""));
            string[] t = tokenizer.Tokens.ToArray();
            Assert.AreEqual("hello", t[0]);
            Assert.AreEqual("world", t[1]);
            Assert.AreEqual("quoted string", t[2]);
            Assert.AreEqual("another string with \"quotes\" inside.", t[3]);
        }
    }
}
