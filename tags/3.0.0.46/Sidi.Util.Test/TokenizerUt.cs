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
using System.Linq;
#endregion

//Test Specific Imports
using Sidi.CommandLine;
using System.IO;
using Sidi.Test;

namespace Sidi.Util
{
    [TestFixture]
    public class TokenizerUt : TestBase
    {
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
